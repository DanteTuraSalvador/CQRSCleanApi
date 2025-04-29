using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.Interfaces;
public interface IEstablishmentAddressUniquenessChecker
{
    Task<Result<bool>> CheckAddressUniquenessAsync(
        decimal latitude,
        decimal longitude,
        EstablishmentId establishmentId,
        EstablishmentAddressId? excludedAddressId = null);
}
