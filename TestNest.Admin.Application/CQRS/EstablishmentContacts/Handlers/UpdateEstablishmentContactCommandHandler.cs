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
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class UpdateEstablishmentContactCommandHandler(
    IEstablishmentContactRepository establishmentContactRepository,
    IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    ILogger<UpdateEstablishmentContactCommandHandler> logger)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<UpdateEstablishmentContactCommand, Result<EstablishmentContactResponse>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<EstablishmentContactResponse>> HandleAsync(UpdateEstablishmentContactCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                             new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                             TransactionScopeAsyncFlowOption.Enabled);

        EstablishmentContactId establishmentContactId = command.EstablishmentContactId;
        EstablishmentContactForUpdateRequest updateRequest = command.UpdateRequest;

        Result<EstablishmentId> establishmentIdResult = IdHelper
            .ValidateAndCreateId<EstablishmentId>(updateRequest.EstablishmentId.ToString());
        if (!establishmentIdResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Validation, establishmentIdResult.Errors);
        }
        EstablishmentId updateEstablishmentId = establishmentIdResult.Value!;

        bool establishmentExists = await _establishmentRepository.ExistsAsync(updateEstablishmentId);
        if (!establishmentExists)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.NotFound, new Error("NotFound", $"Establishment with ID '{updateEstablishmentId}' not found."));
        }

        Result<EstablishmentContact> existingContactResult = await _establishmentContactRepository
            .GetByIdAsync(establishmentContactId);
        if (!existingContactResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(ErrorType.NotFound, existingContactResult.Errors);
        }
        EstablishmentContact existingContact = existingContactResult.Value!;
        await _establishmentContactRepository.DetachAsync(existingContact);

        if (existingContact.EstablishmentId != updateEstablishmentId)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Unauthorized,
                new Error("Unauthorized", $"Cannot update contact. The provided EstablishmentId '{updateEstablishmentId}' does not match the existing contact's EstablishmentId '{existingContact.EstablishmentId}'."));
        }

        EstablishmentContact updatedContact = existingContact;
        bool hasChanges = false;
        PersonName? updatedPersonName = null;
        PhoneNumber? updatedPhoneNumber = null;

        if (updateRequest.ContactPersonFirstName != null ||
            updateRequest.ContactPersonMiddleName != null ||
            updateRequest.ContactPersonLastName != null)
        {
            Result<PersonName> personNameResult = PersonName.Create(
                updateRequest.ContactPersonFirstName ?? existingContact.ContactPerson.FirstName,
                updateRequest.ContactPersonMiddleName ?? existingContact.ContactPerson.MiddleName,
                updateRequest.ContactPersonLastName ?? existingContact.ContactPerson.LastName);
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

        if (updateRequest.ContactPhoneNumber != null && updateRequest.ContactPhoneNumber != existingContact.ContactPhone.PhoneNo)
        {
            Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create(updateRequest.ContactPhoneNumber);
            if (!phoneNumberResult.IsSuccess)
            {
                return Result<EstablishmentContactResponse>.Failure(phoneNumberResult.ErrorType, phoneNumberResult.Errors);
            }
            updatedPhoneNumber = phoneNumberResult.Value!;
            updatedContact = updatedContact.WithContactPhone(updatedPhoneNumber).Value!;
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
            updateEstablishmentId
        );
        if (contactExists)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Conflict,
                new Error("Validation", $"A contact with the same name and phone number already exists for this establishment."));
        }

        if (updateRequest.IsPrimary && updateRequest.IsPrimary != existingContact.IsPrimary)
        {
            if (updateRequest.IsPrimary)
            {
                Result setNonePrimaryResult = await _establishmentContactRepository
                    .SetNonPrimaryForEstablishmentContanctAsync(updateEstablishmentId, establishmentContactId);
                if (!setNonePrimaryResult.IsSuccess)
                {
                    return Result<EstablishmentContactResponse>.Failure(setNonePrimaryResult.ErrorType, setNonePrimaryResult.Errors);
                }
            }
            updatedContact = updatedContact.WithPrimaryFlag(updateRequest.IsPrimary).Value!;
            hasChanges = true;
        }

        if (!hasChanges)
        {
            return Result<EstablishmentContactResponse>.Success(updatedContact.ToEstablishmentContactResponse());
        }

        Result<EstablishmentContact> updateResult = await _establishmentContactRepository.UpdateAsync(updatedContact);
        Result<EstablishmentContact> commitResult = await SafeCommitAsync(() => updateResult);

        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentContactResponse>.Success(commitResult.Value!.ToEstablishmentContactResponse());
        }
        return Result<EstablishmentContactResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
