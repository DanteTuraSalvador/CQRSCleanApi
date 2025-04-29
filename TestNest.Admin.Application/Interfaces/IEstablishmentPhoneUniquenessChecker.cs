using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Interfaces;
public interface IEstablishmentPhoneUniquenessChecker
{
    Task<Result<bool>> CheckPhoneUniquenessAsync(
        PhoneNumber phoneNumber,
        EstablishmentId establishmentId,
        EstablishmentPhoneId? excludedPhoneId = null);
}
