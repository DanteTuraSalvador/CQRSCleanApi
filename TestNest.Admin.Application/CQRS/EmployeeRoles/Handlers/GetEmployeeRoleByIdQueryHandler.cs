using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
public class GetEmployeeRoleByIdQueryHandler(
    IEmployeeRoleRepository employeeRoleRepository,
    IUnitOfWork unitOfWork,
    ILogger<GetEmployeeRoleByIdQueryHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory) : BaseService(unitOfWork, logger, exceptionHandlerFactory), IQueryHandler<GetEmployeeRoleByIdQuery, Result<EmployeeRoleResponse>>
{
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;

    public async Task<Result<EmployeeRoleResponse>> HandleAsync(GetEmployeeRoleByIdQuery query)
    {
        EmployeeRoleId employeeRoleId = query.Id;
        Result<EmployeeRole> employeeRoleResult = await _employeeRoleRepository.GetByIdAsync(employeeRoleId);
        return employeeRoleResult.IsSuccess
            ? Result<EmployeeRoleResponse>.Success(employeeRoleResult.Value!.ToEmployeeRoleResponse())
            : Result<EmployeeRoleResponse>.Failure(employeeRoleResult.ErrorType, employeeRoleResult.Errors);
    }
}
