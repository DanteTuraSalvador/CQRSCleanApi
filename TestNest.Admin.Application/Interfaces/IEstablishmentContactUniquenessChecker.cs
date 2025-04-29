using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Interfaces;

public interface IEstablishmentContactUniquenessChecker
{
    Task<Result<bool>> CheckEstablishmentContactUniquenessAsync(
        PersonName contactPerson,
        PhoneNumber contactPhoneNumber,
        EstablishmentId establishmentId,
        EstablishmentContactId? excludedContactId = null);
}
