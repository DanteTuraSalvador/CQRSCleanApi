using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;

public record GetEmployeeRoleByIdQuery(EmployeeRoleId Id) : IQuery<Result<EmployeeRoleResponse>>;
