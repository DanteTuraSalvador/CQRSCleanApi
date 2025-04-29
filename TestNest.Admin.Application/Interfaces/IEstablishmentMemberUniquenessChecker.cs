using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.Interfaces;
public interface IEstablishmentMemberUniquenessChecker
{
    Task<Result<bool>> CheckMemberUniquenessAsync(
        EmployeeId employeeId,
        EstablishmentId establishmentId,
        EstablishmentMemberId? excludedMemberId = null);
}
