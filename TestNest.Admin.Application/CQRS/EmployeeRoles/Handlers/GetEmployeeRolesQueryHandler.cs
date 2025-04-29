using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Application.Specifications.EmployeeRoleSpecifications;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Paginations;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
public class GetEmployeeRolesQueryHandler(
    IEmployeeRoleRepository employeeRoleRepository,
    IUnitOfWork unitOfWork,
    ILogger<GetEmployeeRolesQueryHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory) : BaseService(unitOfWork, logger, exceptionHandlerFactory), IQueryHandler<GetEmployeeRolesQuery, Result<PaginatedResponse<EmployeeRoleResponse>>>
{
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;

    public async Task<Result<PaginatedResponse<EmployeeRoleResponse>>> HandleAsync(GetEmployeeRolesQuery query)
    {
        var spec = new EmployeeRoleSpecification(
            roleName: query.RoleName,
            employeeRoleId: query.EmployeeRoleId?.ToString(), // Specification expects string? for EmployeeRoleId
            sortBy: query.SortBy,
            sortDirection: query.SortOrder,
            pageNumber: query.PageNumber,
            pageSize: query.PageSize
        );

        Result<int> countResult = await _employeeRoleRepository.CountAsync(spec);
        if (!countResult.IsSuccess)
        { return Result<PaginatedResponse<EmployeeRoleResponse>>.Failure(countResult.ErrorType, countResult.Errors); }
        int totalCount = countResult.Value;

        Result<IEnumerable<EmployeeRole>> employeeRolesResult = await _employeeRoleRepository.ListAsync(spec);
        if (!employeeRolesResult.IsSuccess)
        {
            return Result<PaginatedResponse<EmployeeRoleResponse>>.Failure(employeeRolesResult.ErrorType, employeeRolesResult.Errors);
        }

        IEnumerable<EmployeeRoleResponse> employeeRoleResponses = employeeRolesResult.Value!.Select(er => er.ToEmployeeRoleResponse());

        if (employeeRoleResponses == null || !employeeRoleResponses.Any())
        {
            return Result<PaginatedResponse<EmployeeRoleResponse>>.Failure(ErrorType.NotFound, [new Error(EmployeeRoleException.NotFound().Code.ToString(), EmployeeRoleException.NotFound().Message.ToString())]);
        }

        int totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var paginatedResponse = new PaginatedResponse<EmployeeRoleResponse>
        {
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = totalPages,
            Data = employeeRoleResponses,
            Links = new PaginatedLinks
            {
                First = GeneratePaginationLink(query, 1),
                Last = totalPages > 0 ? GeneratePaginationLink(query, totalPages) : null,
                Next = query.PageNumber < totalPages ? GeneratePaginationLink(query, query.PageNumber + 1) : null,
                Previous = query.PageNumber > 1 ? GeneratePaginationLink(query, query.PageNumber - 1) : null
            }
        };

        return Result<PaginatedResponse<EmployeeRoleResponse>>.Success(paginatedResponse);
    }

    private static string GeneratePaginationLink(GetEmployeeRolesQuery query, int targetPageNumber) =>
        $"/api/EmployeeRoles?pageNumber={targetPageNumber}&pageSize={query.PageSize}&sortBy={query.SortBy}&sortOrder={query.SortOrder}&roleName={query.RoleName}&employeeRoleId={query.EmployeeRoleId}";
}
