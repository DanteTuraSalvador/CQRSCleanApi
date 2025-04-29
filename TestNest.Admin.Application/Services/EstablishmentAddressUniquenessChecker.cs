using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.Services;

public class EstablishmentAddressUniquenessChecker(IEstablishmentAddressRepository establishmentAddressRepository) : IEstablishmentAddressUniquenessChecker
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;

    public async Task<Result<bool>> CheckAddressUniquenessAsync(
        decimal latitude,
        decimal longitude,
        EstablishmentId establishmentId,
        EstablishmentAddressId? excludedAddressId = null)
    {
        Result<EstablishmentAddressId> idResult = excludedAddressId == null
           ? IdHelper.ValidateAndCreateId<EstablishmentAddressId>(Guid.NewGuid().ToString())
           : IdHelper.ValidateAndCreateId<EstablishmentAddressId>(excludedAddressId.Value.ToString());

        if (!idResult.IsSuccess)
        {
            return Result<bool>.Failure(
                idResult.ErrorType,
                idResult.Errors);
        }

        EstablishmentAddressId idToCheck = idResult.Value!;

        bool exists = await _establishmentAddressRepository.AddressExistsWithSameCoordinatesInEstablishment(
            idToCheck,
            latitude,
            longitude,
            establishmentId);

        if (exists)
        {
            return Result<bool>.Success(true);
        }

        return Result<bool>.Success(false);
    }
}
