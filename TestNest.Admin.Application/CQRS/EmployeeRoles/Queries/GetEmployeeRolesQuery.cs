using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Paginations;
using TestNest.Admin.SharedLibrary.Dtos.Responses;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;

public record GetEmployeeRolesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string SortBy = "EmployeeRoleId",
    string SortOrder = "asc",
    string? RoleName = null,
    string? EmployeeRoleId = null
) : IQuery<Result<PaginatedResponse<EmployeeRoleResponse>>>;
