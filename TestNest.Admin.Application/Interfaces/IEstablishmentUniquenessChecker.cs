using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Interfaces;
public interface IEstablishmentUniquenessChecker
{
    Task<Result<bool>> CheckNameAndEmailUniquenessAsync(
        EstablishmentName establishmentName,
        EmailAddress emailAddress,
        EstablishmentId? establishmentId = null);
}
