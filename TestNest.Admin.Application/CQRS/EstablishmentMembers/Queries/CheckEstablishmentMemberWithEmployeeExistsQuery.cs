using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;

public record CheckEstablishmentMemberWithEmployeeExistsQuery(
    EmployeeId EmployeeId,
    EstablishmentId EstablishmentId,
    EstablishmentMemberId? ExcludedMemberId = null
) : IQuery<Result<bool>>;
