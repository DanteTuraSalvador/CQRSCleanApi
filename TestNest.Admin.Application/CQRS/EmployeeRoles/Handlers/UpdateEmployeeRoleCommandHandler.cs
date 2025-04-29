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
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.Application.Mappings;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
public class UpdateEmployeeRoleCommandHandler(
    IEmployeeRoleRepository employeeRoleRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateEmployeeRoleCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory) : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<UpdateEmployeeRoleCommand, Result<EmployeeRoleResponse>>
{
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;

    public async Task<Result<EmployeeRoleResponse>> HandleAsync(UpdateEmployeeRoleCommand command)
    {
        EmployeeRoleId employeeRoleId = command.Id;
        EmployeeRoleForUpdateRequest updateRequest = command.Request;

        Result<EmployeeRole> validatedEmployeeRole = await _employeeRoleRepository.GetByIdAsync(employeeRoleId);
        if (!validatedEmployeeRole.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(validatedEmployeeRole.ErrorType, validatedEmployeeRole.Errors);
        }

        EmployeeRole employeeRole = validatedEmployeeRole.Value!;
        await _employeeRoleRepository.DetachAsync(employeeRole);

        Result<RoleName> roleNameResult = RoleName.Create(updateRequest.RoleName);
        if (!roleNameResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(ErrorType.Validation, roleNameResult.Errors);
        }

        Result<EmployeeRole> existingRoleResult = await _employeeRoleRepository.GetEmployeeRoleByNameAsync(roleNameResult.Value!.Name);
        if (existingRoleResult.IsSuccess && existingRoleResult.Value!.Id != employeeRoleId)
        {
            return Result<EmployeeRoleResponse>.Failure(ErrorType.Conflict, [new Error(EmployeeRoleException.DuplicateResource().Code.ToString(), EmployeeRoleException.DuplicateResource().Message.ToString())]);
        }

        Result<EmployeeRole> updatedEmployeeRoleResult = employeeRole.WithRoleName(roleNameResult.Value!);
        if (!updatedEmployeeRoleResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(updatedEmployeeRoleResult.ErrorType, updatedEmployeeRoleResult.Errors);
        }

        Result<EmployeeRole> updateResult = await _employeeRoleRepository.UpdateAsync(updatedEmployeeRoleResult.Value!);
        if (!updateResult.IsSuccess)
        {
            return Result<EmployeeRoleResponse>.Failure(updateResult.ErrorType, updateResult.Errors);
        }

        return await SafeCommitAsync(() => Result<EmployeeRoleResponse>.Success(updateResult.Value!.ToEmployeeRoleResponse()));
    }
}
