using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
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

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class CreateEstablishmentAddressCommandHandler(
    IEstablishmentAddressRepository establishmentAddressRepository,
    IEstablishmentRepository establishmentRepository,
    ILogger<CreateEstablishmentAddressCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory databaseExceptionHandlerFactory,
    IEstablishmentAddressUniquenessChecker establishmentAddressUniquenessChecker)
        : BaseService(unitOfWork, logger, databaseExceptionHandlerFactory),
          ICommandHandler<CreateEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEstablishmentAddressUniquenessChecker _establishmentAddressUniquenessChecker = establishmentAddressUniquenessChecker;

    public async Task<Result<EstablishmentAddressResponse>> HandleAsync(CreateEstablishmentAddressCommand command)
    {

        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                                      new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                                      TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentId> establishmentIdResult = IdHelper
            .ValidateAndCreateId<EstablishmentId>(command.EstablishmentAddressForCreationRequest.EstablishmentId);

        Result<Address> addressResult = Address.Create(
            command.EstablishmentAddressForCreationRequest.AddressLine,
            command.EstablishmentAddressForCreationRequest.Municipality,
            command.EstablishmentAddressForCreationRequest.City,
            command.EstablishmentAddressForCreationRequest.Province,
            command.EstablishmentAddressForCreationRequest.Region,
            command.EstablishmentAddressForCreationRequest.Country,
            (decimal)command.EstablishmentAddressForCreationRequest.Latitude,
            (decimal)command.EstablishmentAddressForCreationRequest.Longitude);

        var combinedValidationResult = Result.Combine(
            establishmentIdResult.ToResult(),
            addressResult.ToResult());

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                ErrorType.Validation,
                [.. combinedValidationResult.Errors]);
        }

        EstablishmentId establishmentIdToCheck = establishmentIdResult.Value!;
        decimal latitudeToCheck = addressResult.Value!.Latitude;
        decimal longitudeToCheck = addressResult.Value!.Longitude;

        Result<bool> uniquenessCheckResult = await _establishmentAddressUniquenessChecker.CheckAddressUniquenessAsync(
            latitudeToCheck,
            longitudeToCheck,
            establishmentIdToCheck);

        if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                ErrorType.Conflict,
                new Error("Validation", $"An address with the same latitude ({latitudeToCheck}) and longitude ({longitudeToCheck}) already exists for this establishment."));
        }

        Result<Establishment> establishmentResult = await _establishmentRepository
            .GetByIdAsync(establishmentIdToCheck);

        if (!establishmentResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                establishmentResult.ErrorType,
                [.. establishmentResult.Errors]);
        }

        if (command.EstablishmentAddressForCreationRequest.IsPrimary)
        {
            Result setNonPrimaryResult = await _establishmentAddressRepository
                .SetNonPrimaryForEstablishmentAsync(establishmentIdToCheck, EstablishmentAddressId.Empty());

            if (!setNonPrimaryResult.IsSuccess)
            {
                return Result<EstablishmentAddressResponse>.Failure(setNonPrimaryResult.ErrorType, [.. setNonPrimaryResult.Errors]);
            }
        }

        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(
            establishmentIdToCheck,
            addressResult.Value!,
            command.EstablishmentAddressForCreationRequest.IsPrimary);

        if (!establishmentAddressResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                establishmentAddressResult.ErrorType,
                [.. establishmentAddressResult.Errors]);
        }

        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        _ = await _establishmentAddressRepository.AddAsync(establishmentAddress);
        Result<EstablishmentAddress> commitResult = await SafeCommitAsync(() => Result<EstablishmentAddress>.Success(establishmentAddress));

        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentAddressResponse>.Success(commitResult.Value!.ToEstablishmentAddressResponse());
        }

        return Result<EstablishmentAddressResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
