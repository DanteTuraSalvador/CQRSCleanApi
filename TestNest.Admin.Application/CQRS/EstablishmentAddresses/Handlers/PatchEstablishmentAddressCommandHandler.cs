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
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class PatchEstablishmentAddressCommandHandler(IEstablishmentAddressRepository establishmentAddressRepository,
    ILogger<PatchEstablishmentAddressCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory databaseExceptionHandlerFactory,
    IEstablishmentAddressUniquenessChecker uniquenessChecker)
    : BaseService(unitOfWork, logger, databaseExceptionHandlerFactory),
      ICommandHandler<PatchEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    private readonly IEstablishmentAddressUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentAddressResponse>> HandleAsync(PatchEstablishmentAddressCommand command)
    {

        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentAddress> existingAddressResult = await _establishmentAddressRepository
            .GetByIdAsync(command.EstablishmentAddressId);
        if (!existingAddressResult.IsSuccess)
        {
            return Result<EstablishmentAddressResponse>.Failure(existingAddressResult.ErrorType, existingAddressResult.Errors);
        }

        EstablishmentAddress existingAddress = existingAddressResult.Value!;
        await _establishmentAddressRepository.DetachAsync(existingAddress);

        EstablishmentAddress updatedAddress = existingAddress;
        bool hasChanges = false;

        decimal updatedLatitude = existingAddress.Address.Latitude;
        decimal updatedLongitude = existingAddress.Address.Longitude;

        if (HasAddressPatchUpdate(command.EstablishmentAddressPatchRequest, existingAddress.Address))
        {
            Result<Address> addressResult = Address.Create(
                command.EstablishmentAddressPatchRequest.AddressLine ?? existingAddress.Address.AddressLine,
                command.EstablishmentAddressPatchRequest.City ?? existingAddress.Address.City,
                command.EstablishmentAddressPatchRequest.Municipality ?? existingAddress.Address.Municipality,
            command.EstablishmentAddressPatchRequest.Province ?? existingAddress.Address.Province,
            command.EstablishmentAddressPatchRequest.Region ?? existingAddress.Address.Region,
            command.EstablishmentAddressPatchRequest.Country ?? existingAddress.Address.Country,
            command.EstablishmentAddressPatchRequest.Latitude.HasValue ? (decimal)command.EstablishmentAddressPatchRequest.Latitude.Value : existingAddress.Address.Latitude,
                command.EstablishmentAddressPatchRequest.Longitude.HasValue ? (decimal)command.EstablishmentAddressPatchRequest.Longitude.Value : existingAddress.Address.Longitude
            );

            if (!addressResult.IsSuccess)
            {
                return Result<EstablishmentAddressResponse>.Failure(
                    addressResult.ErrorType, [.. addressResult.Errors]);
            }

            updatedAddress = updatedAddress.WithAddress(addressResult.Value!).Value!;
            updatedLatitude = addressResult.Value!.Latitude;
            updatedLongitude = addressResult.Value!.Longitude;
            hasChanges = true;
        }

        Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckAddressUniquenessAsync(
            updatedLatitude,
            updatedLongitude,
            existingAddress.EstablishmentId,
            command.EstablishmentAddressId);

        if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
        {
            return Result<EstablishmentAddressResponse>.Failure(
            ErrorType.Conflict,
            new Error("Validation", $"An address with the same latitude ({updatedLatitude}) and longitude ({updatedLongitude}) already exists for this establishment."));
        }

        if (command.EstablishmentAddressPatchRequest.IsPrimary.HasValue && command.EstablishmentAddressPatchRequest.IsPrimary != existingAddress.IsPrimary)
        {
            if (command.EstablishmentAddressPatchRequest.IsPrimary.Value)
            {
                Result setNonPrimaryResult = await _establishmentAddressRepository
                    .SetNonPrimaryForEstablishmentAsync(existingAddress.EstablishmentId, command.EstablishmentAddressId);

                if (!setNonPrimaryResult.IsSuccess)
                {
                    return Result<EstablishmentAddressResponse>.Failure(
                        setNonPrimaryResult.ErrorType, [.. setNonPrimaryResult.Errors]);
                }
            }
            updatedAddress = updatedAddress.WithIsPrimary(command.EstablishmentAddressPatchRequest.IsPrimary.Value).Value!;
            hasChanges = true;
        }

        if (hasChanges)
        {
            Result<EstablishmentAddress> updateResult = await _establishmentAddressRepository.UpdateAsync(updatedAddress);
            Result<EstablishmentAddress> commitResult = await SafeCommitAsync(() => updateResult);
            if (commitResult.IsSuccess)
            {
                scope.Complete();
                return Result<EstablishmentAddressResponse>.Success(commitResult.Value!.ToEstablishmentAddressResponse());
            }
            return Result<EstablishmentAddressResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
        }

        return Result<EstablishmentAddressResponse>.Success(existingAddress.ToEstablishmentAddressResponse());
    }

    private static bool HasAddressPatchUpdate(EstablishmentAddressPatchRequest
        request, Address existingAddress) =>
        request.AddressLine != existingAddress.AddressLine ||
        request.City != existingAddress.City ||
        request.Municipality != existingAddress.Municipality ||
        request.Province != existingAddress.Province ||
        request.Region != existingAddress.Region ||
        request.Country != existingAddress.Country ||
        request.Latitude.HasValue && (decimal)request.Latitude.Value != existingAddress.Latitude ||
        request.Longitude.HasValue && (decimal)request.Longitude.Value != existingAddress.Longitude;
}
