using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Services.Base;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Interfaces;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
public class CreateEmployeeRoleCommandHandler(
    IEmployeeRoleRepository employeeRoleRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateEmployeeRoleCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory) : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<CreateEmployeeRoleCommand, Result<EmployeeRoleResponse>>
{
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;

    public async Task<Result<EmployeeRoleResponse>> HandleAsync(CreateEmployeeRoleCommand command)
    {
        EmployeeRoleForCreationRequest creationRequest = command.Request;
        Result<RoleName> roleNameResult = RoleName.Create(creationRequest.RoleName);

        if (!roleNameResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(ErrorType.Validation, roleNameResult.Errors);
        }

        Result<EmployeeRole> existingRoleResult = await _employeeRoleRepository
            .GetEmployeeRoleByNameAsync(roleNameResult.Value!.Name);

        if (existingRoleResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(ErrorType.Conflict, [new Error(EmployeeRoleException.DuplicateResource().Code.ToString(), EmployeeRoleException.DuplicateResource().Message.ToString())]);
        }

        Result<EmployeeRole> employeeRoleResult = EmployeeRole.Create(roleNameResult.Value!);

        if (!employeeRoleResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(ErrorType.Validation, employeeRoleResult.Errors);
        }

        EmployeeRole employeeRole = employeeRoleResult.Value!;
        _ = await _employeeRoleRepository.AddAsync(employeeRole);
        return await SafeCommitAsync(() => Result<EmployeeRoleResponse>.Success(employeeRole.ToEmployeeRoleResponse()));
    }
}
