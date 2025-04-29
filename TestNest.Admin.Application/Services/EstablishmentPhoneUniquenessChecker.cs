using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Services;
public class EstablishmentPhoneUniquenessChecker(IEstablishmentPhoneRepository establishmentPhoneRepository) : IEstablishmentPhoneUniquenessChecker
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;

    public async Task<Result<bool>> CheckPhoneUniquenessAsync(
        PhoneNumber phoneNumber,
        EstablishmentId establishmentId,
        EstablishmentPhoneId? excludedPhoneId = null)
    {
        Result<EstablishmentPhoneId> idResult = excludedPhoneId == null
            ? IdHelper.ValidateAndCreateId<EstablishmentPhoneId>(Guid.NewGuid().ToString())
            : IdHelper.ValidateAndCreateId<EstablishmentPhoneId>(excludedPhoneId.Value.ToString());

        if (!idResult.IsSuccess)
        {
            return Result<bool>.Failure(
                idResult.ErrorType,
                idResult.Errors);
        }

        EstablishmentPhoneId idToCheck = idResult.Value!;

        bool exists = await _establishmentPhoneRepository.PhoneExistsWithSameNumberInEstablishment(
            idToCheck,
            phoneNumber.PhoneNo,
            establishmentId);

        return Result<bool>.Success(exists);
    }
}
