using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class UpdateEstablishmentAddressCommandHandler(
    IEstablishmentAddressRepository establishmentAddressRepository,
    IEstablishmentRepository establishmentRepository,
    ILogger<UpdateEstablishmentAddressCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory databaseExceptionHandlerFactory,
    IEstablishmentAddressUniquenessChecker uniquenessChecker)
    : BaseService(unitOfWork, logger, databaseExceptionHandlerFactory),
      ICommandHandler<UpdateEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    private readonly IEstablishmentAddressUniquenessChecker _uniquenessChecker = uniquenessChecker;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<EstablishmentAddressResponse>> HandleAsync(UpdateEstablishmentAddressCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
           new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
           TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentId> establishmentIdResult = IdHelper
            .ValidateAndCreateId<EstablishmentId>(command.EstablishmentAddressForUpdateRequest.EstablishmentId.ToString());
        if (!establishmentIdResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                ErrorType.Validation, establishmentIdResult.Errors);
        }

        EstablishmentId updateEstablishmentId = establishmentIdResult.Value!;
        bool establishmentExists = await _establishmentRepository.ExistsAsync(updateEstablishmentId);
        if (!establishmentExists)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                ErrorType.NotFound, new Error("NotFound", $"Establishment with ID '{updateEstablishmentId}' not found."));
        }

        Result<EstablishmentAddress> existingAddressResult = await _establishmentAddressRepository
            .GetByIdAsync(command.EstablishmentAddressId);
        if (!existingAddressResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(ErrorType.Validation, existingAddressResult.Errors);
        }

        EstablishmentAddress existingAddress = existingAddressResult.Value!;
        await _establishmentAddressRepository.DetachAsync(existingAddress);

        if (existingAddress.EstablishmentId != updateEstablishmentId)
        {
            return Result<EstablishmentAddressResponse>.Failure(
                ErrorType.Unauthorized,
                new Error("Unauthorized", $"Cannot update address. The provided EstablishmentId '{updateEstablishmentId}' does not match the existing address's EstablishmentId '{existingAddress.EstablishmentId}'."));
        }

        EstablishmentAddress? updatedAddress = existingAddress;
        bool hasChanges = false;

        decimal updatedLatitude = existingAddress.Address.Latitude;
        decimal updatedLongitude = existingAddress.Address.Longitude;

        if (HasAddressUpdate(command.EstablishmentAddressForUpdateRequest))
        {
            Result<Address> addressResult = Address.Create(
                command.EstablishmentAddressForUpdateRequest.AddressLine ?? existingAddress.Address.AddressLine,
                command.EstablishmentAddressForUpdateRequest.City ?? existingAddress.Address.City,
                command.EstablishmentAddressForUpdateRequest.Municipality ?? existingAddress.Address.Municipality,
                command.EstablishmentAddressForUpdateRequest.Province ?? existingAddress.Address.Province,
                command.EstablishmentAddressForUpdateRequest.Region ?? existingAddress.Address.Region,
                command.EstablishmentAddressForUpdateRequest.Country ?? existingAddress.Address.Country,
                (decimal)command.EstablishmentAddressForUpdateRequest.Latitude,
                (decimal)command.EstablishmentAddressForUpdateRequest.Longitude
            );
            if (!addressResult.IsSuccess)
            {
                return Result<EstablishmentAddressResponse>.Failure(addressResult.ErrorType, addressResult.Errors);
            }

            updatedAddress = updatedAddress.WithAddress(addressResult.Value!).Value!;
            updatedLatitude = addressResult.Value!.Latitude;
            updatedLongitude = addressResult.Value!.Longitude;
            hasChanges = true;
        }

        // Check for uniqueness based on Latitude and Longitude (excluding the current address being updated)
        Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckAddressUniquenessAsync(
            updatedLatitude,
            updatedLongitude,
            updateEstablishmentId,
            command.EstablishmentAddressId);

        if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
        {
            return Result<EstablishmentAddressResponse>.Failure(
            ErrorType.Conflict,
                new Error("Validation", $"An address with the same latitude ({updatedLatitude}) and longitude ({updatedLongitude}) already exists for this establishment."));
        }

        if (command.EstablishmentAddressForUpdateRequest.IsPrimary && command.EstablishmentAddressForUpdateRequest.IsPrimary != existingAddress.IsPrimary)
        {
            if (command.EstablishmentAddressForUpdateRequest.IsPrimary)
            {
                Result setNonPrimaryResult = await _establishmentAddressRepository
                    .SetNonPrimaryForEstablishmentAsync(updateEstablishmentId, command.EstablishmentAddressId);

                if (!setNonPrimaryResult.IsSuccess)
                {
                    return Result<EstablishmentAddressResponse>.Failure(
                        setNonPrimaryResult.ErrorType, setNonPrimaryResult.Errors);
                }

                updatedAddress = updatedAddress.WithIsPrimary(true).Value!;
                hasChanges = true;
            }
            else
            {
                updatedAddress = updatedAddress.WithIsPrimary(false).Value!;
                hasChanges = true;
            }
        }

        if (!hasChanges)
        {
            return Result<EstablishmentAddressResponse>.Success(existingAddress.ToEstablishmentAddressResponse());
        }

        Result<EstablishmentAddress> updateResult = await _establishmentAddressRepository.UpdateAsync(updatedAddress);
        Result<EstablishmentAddress> commitResult = await SafeCommitAsync(() => updateResult);
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentAddressResponse>.Success(commitResult.Value!.ToEstablishmentAddressResponse());
        }
        return Result<EstablishmentAddressResponse>.Failure(commitResult.ErrorType, commitResult.Errors);

    }

    private static bool HasAddressUpdate(EstablishmentAddressForUpdateRequest request) =>
     request.AddressLine != null ||
     request.City != null ||
     request.Municipality != null ||
     request.Province != null ||
     request.Region != null ||
     request.Country != null ||
     request.Latitude != default ||
     request.Longitude != default;
}
