using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class PatchEstablishmentPhoneCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<PatchEstablishmentPhoneCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentPhoneRepository establishmentPhoneRepository,
    IEstablishmentPhoneUniquenessChecker uniquenessChecker) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<PatchEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    private readonly IEstablishmentPhoneUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentPhoneResponse>> HandleAsync(PatchEstablishmentPhoneCommand command)
        => await SafeTransactionAsync(async () =>
        {
            Result<EstablishmentPhone> existingPhoneResult = await _establishmentPhoneRepository
                .GetByIdAsync(command.EstablishmentPhoneId);
            if (!existingPhoneResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(existingPhoneResult.ErrorType, existingPhoneResult.Errors);
            }
            EstablishmentPhone existingPhone = existingPhoneResult.Value!;
            await _establishmentPhoneRepository.DetachAsync(existingPhone);

            EstablishmentPhone updatedPhone = existingPhone;
            bool hasChanges = false;
            PhoneNumber? updatedPhoneNumber = null;

            if (!string.IsNullOrEmpty(command.PatchRequest.PhoneNumber) && command.PatchRequest.PhoneNumber != existingPhone.EstablishmentPhoneNumber.PhoneNo)
            {
                Result<PhoneNumber> phoneNumberResult = PhoneNumber.Update(command.PatchRequest.PhoneNumber);
                if (!phoneNumberResult.IsSuccess)
                {
                    return Result<EstablishmentPhoneResponse>.Failure(ErrorType.Validation, phoneNumberResult.Errors);
                }
                updatedPhoneNumber = phoneNumberResult.Value!;
                updatedPhone = updatedPhone.WithPhoneNumber(updatedPhoneNumber).Value!;
                hasChanges = true;
            }
            else
            {
                updatedPhoneNumber = existingPhone.EstablishmentPhoneNumber;
            }

            Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckPhoneUniquenessAsync(
                updatedPhoneNumber,
                existingPhone.EstablishmentId,
                command.EstablishmentPhoneId
            );

            if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Conflict,
                    new Error("Validation", $"Phone number '{updatedPhoneNumber.PhoneNo}' already exists for this establishment."));
            }

            if (command.PatchRequest.IsPrimary.HasValue && command.PatchRequest.IsPrimary != existingPhone.IsPrimary)
            {
                updatedPhone = updatedPhone.WithIsPrimary(command.PatchRequest.IsPrimary.Value).Value!;
                if (updatedPhone.IsPrimary)
                {
                    _ = await _establishmentPhoneRepository.SetNonPrimaryForEstablishmentAsync(updatedPhone.EstablishmentId, updatedPhone.EstablishmentPhoneId);
                }
                hasChanges = true;
            }

            if (hasChanges)
            {
                Result<EstablishmentPhone> updateResult = await _establishmentPhoneRepository.UpdateAsync(updatedPhone);
                return updateResult.IsSuccess
                    ? Result<EstablishmentPhoneResponse>.Success(updateResult.Value!.ToEstablishmentPhoneResponse())
                    : Result<EstablishmentPhoneResponse>.Failure(updateResult.ErrorType, updateResult.Errors);
            }

            return Result<EstablishmentPhoneResponse>.Success(existingPhone.ToEstablishmentPhoneResponse());
        });
}
