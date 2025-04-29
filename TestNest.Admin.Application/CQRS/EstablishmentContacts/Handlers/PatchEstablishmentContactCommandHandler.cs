using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Transactions;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class PatchEstablishmentContactCommandHandler(
    IEstablishmentContactRepository establishmentContactRepository,
    IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    ILogger<PatchEstablishmentContactCommandHandler> logger)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<PatchEstablishmentContactCommand, Result<EstablishmentContactResponse>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<EstablishmentContactResponse>> HandleAsync(PatchEstablishmentContactCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                             new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                             TransactionScopeAsyncFlowOption.Enabled);

        EstablishmentContactId establishmentContactId = command.EstablishmentContactId;
        SharedLibrary.Dtos.Requests.Establishment.EstablishmentContactPatchRequest patchRequest = command.PatchRequest;

        Result<EstablishmentContact> existingContactResult = await _establishmentContactRepository
            .GetByIdAsync(establishmentContactId);
        if (!existingContactResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(existingContactResult.ErrorType, existingContactResult.Errors);
        }
        EstablishmentContact existingContact = existingContactResult.Value!;
        await _establishmentContactRepository.DetachAsync(existingContact);

        Result<EstablishmentId> establishmentIdResult = IdHelper
            .ValidateAndCreateId<EstablishmentId>(existingContact.EstablishmentId.ToString());
        if (!establishmentIdResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(ErrorType.Validation, establishmentIdResult.Errors);
        }
        EstablishmentId requestEstablishmentId = establishmentIdResult.Value!;

        bool establishmentExists = await _establishmentRepository.ExistsAsync(requestEstablishmentId);
        if (!establishmentExists)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.NotFound,
                new Error("NotFound", $"Establishment with ID '{requestEstablishmentId}' not found."));
        }

        if (requestEstablishmentId != existingContact.EstablishmentId)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Unauthorized,
                new Error("Unauthorized", $"Cannot patch contact. The provided EstablishmentId '{requestEstablishmentId}' does not match the existing contact's EstablishmentId '{existingContact.EstablishmentId}'."));
        }

        EstablishmentContact updatedContact = existingContact;
        PersonName? updatedPersonName = null;
        PhoneNumber? updatedPhoneNumber = null;
        bool hasChanges = false;

        if (patchRequest.ContactPersonFirstName != null ||
            patchRequest.ContactPersonMiddleName != null ||
            patchRequest.ContactPersonLastName != null)
        {
            Result<PersonName> personNameResult = PersonName.Create(
                patchRequest.ContactPersonFirstName ?? existingContact.ContactPerson.FirstName,
                patchRequest.ContactPersonMiddleName ?? existingContact.ContactPerson.MiddleName,
                patchRequest.ContactPersonLastName ?? existingContact.ContactPerson.LastName);

            if (!personNameResult.IsSuccess)
            {
                return Result<EstablishmentContactResponse>.Failure(personNameResult.ErrorType, personNameResult.Errors);
            }
            updatedPersonName = personNameResult.Value!;
            updatedContact = updatedContact.WithContactPerson(updatedPersonName).Value!;
            hasChanges = true;
        }
        else
        {
            updatedPersonName = existingContact.ContactPerson;
        }

        if (patchRequest.ContactPhoneNumber != null && patchRequest.ContactPhoneNumber != existingContact.ContactPhone.PhoneNo)
        {
            Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create(patchRequest.ContactPhoneNumber);
            if (!phoneNumberResult.IsSuccess)
            {
                return Result<EstablishmentContactResponse>.Failure(phoneNumberResult.ErrorType, phoneNumberResult.Errors);
            }
            updatedPhoneNumber = phoneNumberResult.Value!;
            updatedContact = updatedContact.WithContactPhone(phoneNumberResult.Value!).Value!;
            hasChanges = true;
        }
        else
        {
            updatedPhoneNumber = existingContact.ContactPhone;
        }

        bool contactExists = await _establishmentContactRepository.ContactExistsWithSameDetailsInEstablishment(
            establishmentContactId,
            updatedPersonName,
            updatedPhoneNumber,
            requestEstablishmentId
        );
        if (contactExists)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Conflict,
                new Error("Validation", $"A contact with the same name and phone number already exists for this establishment."));
        }

        if (patchRequest.IsPrimary.HasValue && patchRequest.IsPrimary != existingContact.IsPrimary)
        {
            if (patchRequest.IsPrimary.Value)
            {
                Result setNonPrimaryResult = await _establishmentContactRepository
                    .SetNonPrimaryForEstablishmentContanctAsync(requestEstablishmentId, establishmentContactId);

                if (!setNonPrimaryResult.IsSuccess)
                {
                    return Result<EstablishmentContactResponse>.Failure(setNonPrimaryResult.ErrorType, setNonPrimaryResult.Errors);
                }
            }
            updatedContact = updatedContact.WithPrimaryFlag(patchRequest.IsPrimary.Value).Value!;
            hasChanges = true;
        }

        if (hasChanges)
        {
            Result<EstablishmentContact> updateResult = await _establishmentContactRepository.UpdateAsync(updatedContact);
            Result<EstablishmentContact> commitResult = await SafeCommitAsync(() => updateResult);
            if (commitResult.IsSuccess)
            {
                scope.Complete();
                return Result<EstablishmentContactResponse>.Success(commitResult.Value!.ToEstablishmentContactResponse());
            }
            return Result<EstablishmentContactResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
        }

        return Result<EstablishmentContactResponse>.Success(existingContact.ToEstablishmentContactResponse());
    }
}
