using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Services;

public class EstablishmentContactUniquenessChecker(IEstablishmentContactRepository establishmentContactRepository) 
    : IEstablishmentContactUniquenessChecker
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;

    public async Task<Result<bool>> CheckEstablishmentContactUniquenessAsync(
        PersonName contactPerson,
        PhoneNumber contactPhoneNumber,
        EstablishmentId establishmentId,
        EstablishmentContactId? excludedContactId = null)
    {
        Result<EstablishmentContactId> idResult = excludedContactId == null
               ? IdHelper.ValidateAndCreateId<EstablishmentContactId>(Guid.NewGuid().ToString())
               : IdHelper.ValidateAndCreateId<EstablishmentContactId>(excludedContactId.Value.ToString());

        if (!idResult.IsSuccess)
        {
            return Result<bool>.Failure(
                idResult.ErrorType,
                idResult.Errors);
        }

        EstablishmentContactId idToCheck = idResult.Value!;

        bool exists = await _establishmentContactRepository.ContactExistsWithSameDetailsInEstablishment(
            idToCheck,
            contactPerson,
            contactPhoneNumber,
            establishmentId);

        return Result<bool>.Success(exists);

    }
}
