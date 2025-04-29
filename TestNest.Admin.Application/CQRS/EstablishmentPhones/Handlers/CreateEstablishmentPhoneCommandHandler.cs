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

public class CreateEstablishmentPhoneCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateEstablishmentPhoneCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentPhoneRepository establishmentPhoneRepository,
    IEstablishmentRepository establishmentRepository,
    IEstablishmentPhoneUniquenessChecker uniquenessChecker) :
    BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<CreateEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEstablishmentPhoneUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentPhoneResponse>> HandleAsync(CreateEstablishmentPhoneCommand command)
        => await SafeTransactionAsync(async () =>
        {
            Result<EstablishmentId> establishmentIdResult = IdHelper
                .ValidateAndCreateId<EstablishmentId>(command.CreationRequest.EstablishmentId);

            Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create(command.CreationRequest.PhoneNumber);

            var combinedValidationResult = Result.Combine(
                establishmentIdResult.ToResult(),
                phoneNumberResult.ToResult());

            if (!combinedValidationResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Validation,
                    [.. combinedValidationResult.Errors]);
            }

            Result<Establishment> establishmentResult = await _establishmentRepository
                .GetByIdAsync(establishmentIdResult.Value!);

            if (!establishmentResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    establishmentResult.ErrorType,
                    [.. establishmentResult.Errors]);
            }

            Result<bool> existsResult = await _uniquenessChecker.CheckPhoneUniquenessAsync(
                phoneNumberResult.Value!,
                establishmentIdResult.Value!
            );

            if (!existsResult.IsSuccess || existsResult.Value)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    ErrorType.Conflict,
                    new Error("Validation", $"Phone number '{phoneNumberResult.Value!.PhoneNo}' already exists for this establishment."));
            }

            Result<EstablishmentPhone> establishmentPhoneResult = EstablishmentPhone.Create(
                establishmentIdResult.Value!,
                phoneNumberResult.Value!,
                command.CreationRequest.IsPrimary);

            if (!establishmentPhoneResult.IsSuccess)
            {
                return Result<EstablishmentPhoneResponse>.Failure(
                    establishmentPhoneResult.ErrorType,
                    [.. establishmentPhoneResult.Errors]);
            }

            EstablishmentPhone newPhone = establishmentPhoneResult.Value!;
            if (newPhone.IsPrimary)
            {
                Result setNonPrimaryResult = await _establishmentPhoneRepository
                    .SetNonPrimaryForEstablishmentAsync(newPhone.EstablishmentId, EstablishmentPhoneId.Empty());

                if (!setNonPrimaryResult.IsSuccess)
                {
                    return Result<EstablishmentPhoneResponse>.Failure(setNonPrimaryResult.ErrorType, [.. setNonPrimaryResult.Errors]);
                }
            }

            _ = await _establishmentPhoneRepository.AddAsync(newPhone);
            return Result<EstablishmentPhoneResponse>.Success(newPhone.ToEstablishmentPhoneResponse());
        });
}
