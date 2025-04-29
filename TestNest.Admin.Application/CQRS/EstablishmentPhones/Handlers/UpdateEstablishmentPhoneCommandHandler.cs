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
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class UpdateEstablishmentPhoneCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateEstablishmentPhoneCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentPhoneRepository establishmentPhoneRepository,
    IEstablishmentRepository establishmentRepository,
    IEstablishmentPhoneUniquenessChecker uniquenessChecker) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<UpdateEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEstablishmentPhoneUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentPhoneResponse>> HandleAsync(UpdateEstablishmentPhoneCommand command)
        => await SafeTransactionAsync(async () =>
        {
            Result<EstablishmentId> establishmentIdResult = IdHelper
                .ValidateAndCreateId<EstablishmentId>(command.UpdateRequest.EstablishmentId);
            if (!establishmentIdResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Validation, establishmentIdResult.Errors);
            }

            EstablishmentId updateEstablishmentId = establishmentIdResult.Value!;
            bool establishmentExists = await _establishmentRepository.ExistsAsync(updateEstablishmentId);
            if (!establishmentExists)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.NotFound, new Error("NotFound", $"Establishment with ID '{updateEstablishmentId}' not found."));
            }

            Result<EstablishmentPhone> existingPhoneResult = await _establishmentPhoneRepository
                .GetByIdAsync(command.EstablishmentPhoneId);
            if (!existingPhoneResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(existingPhoneResult.ErrorType, existingPhoneResult.Errors);
            }
            EstablishmentPhone existingPhone = existingPhoneResult.Value!;
            await _establishmentPhoneRepository.DetachAsync(existingPhone);

            if (existingPhone.EstablishmentId != updateEstablishmentId)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Unauthorized,
                    new Error("Unauthorized", $"Cannot update phone. The provided EstablishmentId '{updateEstablishmentId}' does not match the existing phone's EstablishmentId '{existingPhone.EstablishmentId}'."));
            }

            Result<PhoneNumber> phoneNumberResult = PhoneNumber.Update(command.UpdateRequest.PhoneNumber);
            if (!phoneNumberResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(ErrorType.Validation, phoneNumberResult.Errors);
            }

            PhoneNumber updatedPhoneNumber = phoneNumberResult.Value!;
            EstablishmentPhone updatedPhone = existingPhone.WithPhoneNumber(updatedPhoneNumber).Value!;

            Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckPhoneUniquenessAsync(
                updatedPhoneNumber,
                updateEstablishmentId,
                command.EstablishmentPhoneId
            );

            if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Conflict,
                    new Error("Validation", $"Phone number '{updatedPhoneNumber.PhoneNo}' already exists for this establishment."));
            }

            if (command.UpdateRequest.IsPrimary != existingPhone.IsPrimary)
            {
                updatedPhone = updatedPhone.WithIsPrimary(command.UpdateRequest.IsPrimary).Value!;
                if (updatedPhone.IsPrimary)
                {
                    _ = await _establishmentPhoneRepository.SetNonPrimaryForEstablishmentAsync(updatedPhone.EstablishmentId, updatedPhone.EstablishmentPhoneId);
                }
            }

            Result<EstablishmentPhone> updateResult = await _establishmentPhoneRepository.UpdateAsync(updatedPhone);
            return updateResult.IsSuccess
                ? Result<EstablishmentPhoneResponse>.Success(updateResult.Value!.ToEstablishmentPhoneResponse())
                : Result<EstablishmentPhoneResponse>.Failure(updateResult.ErrorType, updateResult.Errors);
        });

}
