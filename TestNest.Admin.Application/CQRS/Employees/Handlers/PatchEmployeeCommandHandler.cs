using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Employees.Commands;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;

public class PatchEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IEmployeeRoleRepository employeeRoleRepository,
    IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<PatchEmployeeCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<PatchEmployeeCommand, Result<EmployeeResponse>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<EmployeeResponse>> HandleAsync(PatchEmployeeCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result<Employee> validatedEmployee = await _employeeRepository.GetByIdAsync(command.EmployeeId);
        if (!validatedEmployee.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(validatedEmployee.ErrorType, validatedEmployee.Errors);
        }

        Employee employee = validatedEmployee.Value!;
        await _employeeRepository.DetachAsync(employee);

        var employeePatchRequest = new EmployeePatchRequest();
        command.PatchDocument.ApplyTo(employeePatchRequest);

        if (employeePatchRequest.EmployeeNumber != null)
        {
            Result<EmployeeNumber> employeeNumberResult = EmployeeNumber.Create(employeePatchRequest.EmployeeNumber);
            if (!employeeNumberResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(employeeNumberResult.ErrorType, employeeNumberResult.Errors);
            }

            employee = employee.WithEmployeeNumber(employeeNumberResult.Value!).Value!;
        }

        if (employeePatchRequest.FirstName != null || employeePatchRequest.MiddleName != null || employeePatchRequest.LastName != null)
        {
            Result<PersonName> nameResult = PersonName.Create(
                employeePatchRequest.FirstName ?? employee.EmployeeName.FirstName,
                employeePatchRequest.MiddleName ?? employee.EmployeeName.MiddleName,
                employeePatchRequest.LastName ?? employee.EmployeeName.LastName);
            if (!nameResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(nameResult.ErrorType, nameResult.Errors);
            }

            employee = employee.WithPersonName(nameResult.Value!).Value!;
        }

        if (employeePatchRequest.EmailAddress != null)
        {
            Result<EmailAddress> emailResult = EmailAddress.Create(employeePatchRequest.EmailAddress);
            if (!emailResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(emailResult.ErrorType, emailResult.Errors);
            }

            employee = employee.WithEmail(emailResult.Value!).Value!;
        }

        if (employeePatchRequest.EmployeeRoleId != null)
        {
            Result<EmployeeRoleId> roleIdResult = IdHelper.ValidateAndCreateId<EmployeeRoleId>(employeePatchRequest.EmployeeRoleId);
            if (!roleIdResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(roleIdResult.ErrorType, roleIdResult.Errors);
            }

            bool roleExists = await _employeeRoleRepository.RoleIdExists(roleIdResult.Value!);
            if (!roleExists)
            {
                return Result<EmployeeResponse>.Failure(ErrorType.NotFound, new Error(EmployeeException.InvalidEmployeeRoleId.Code.ToString(), EmployeeException.InvalidEmployeeRoleId.Message));
            }

            employee = employee.WithEmployeeRole(roleIdResult.Value!).Value!;
        }

        if (employeePatchRequest.EstablishmentId != null)
        {
            Result<EstablishmentId> establishmentIdResult = IdHelper.ValidateAndCreateId<EstablishmentId>(employeePatchRequest.EstablishmentId);
            if (!establishmentIdResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(establishmentIdResult.ErrorType, establishmentIdResult.Errors);
            }

            bool establishmentExists = await _establishmentRepository.EstablishmentIdExists(establishmentIdResult.Value!);
            if (!establishmentExists)
            {
                return Result<EmployeeResponse>.Failure(ErrorType.NotFound, new Error(EmployeeException.InvalidEstablishmentId.Code.ToString(), EmployeeException.InvalidEstablishmentId.Message));
            }

            employee = employee.WithEstablishmentId(establishmentIdResult.Value!).Value!;
        }

        if (employeePatchRequest.EmployeeStatusId != null)
        {
            Result<EmployeeStatus> statusResult = EmployeeStatus.FromId(employeePatchRequest.EmployeeStatusId.Value);
            if (!statusResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(statusResult.ErrorType, statusResult.Errors);
            }

            employee = employee.WithEmployeeStatus(statusResult.Value!).Value!;
        }

        // Uniqueness check if any identifying fields are updated
        if (employeePatchRequest.EmployeeNumber != null || employeePatchRequest.FirstName != null || employeePatchRequest.MiddleName != null || employeePatchRequest.LastName != null || employeePatchRequest.EstablishmentId != null || employeePatchRequest.EmailAddress != null)
        {
            EmployeeNumber employeeNumberToCheck = employeePatchRequest.EmployeeNumber != null ? EmployeeNumber.Create(employeePatchRequest.EmployeeNumber).Value! : employee.EmployeeNumber;
            Result<PersonName> personNameToCheckResult = PersonName.Create(employeePatchRequest.FirstName ?? employee.EmployeeName.FirstName, employeePatchRequest.MiddleName ?? employee.EmployeeName.MiddleName, employeePatchRequest.LastName ?? employee.EmployeeName.LastName);
            if (!personNameToCheckResult.IsSuccess)
            {
                return Result<EmployeeResponse>.Failure(personNameToCheckResult.ErrorType, personNameToCheckResult.Errors);
            }

            PersonName personNameToCheck = personNameToCheckResult.Value!;
            EmailAddress emailToCheck = employeePatchRequest.EmailAddress != null ? EmailAddress.Create(employeePatchRequest.EmailAddress).Value! : employee.EmployeeEmail;
            EstablishmentId establishmentIdToCheck = employeePatchRequest.EstablishmentId != null ? IdHelper.ValidateAndCreateId<EstablishmentId>(employeePatchRequest.EstablishmentId).Value! : employee.EstablishmentId;

            bool exists = await _employeeRepository.EmployeeExistsWithSameCombination(command.EmployeeId, employeeNumberToCheck, personNameToCheck, emailToCheck, establishmentIdToCheck);
            if (exists)
            {
                return Result<EmployeeResponse>.Failure(ErrorType.Conflict, new Error(EmployeeException.DuplicateResource.Code.ToString(), EmployeeException.DuplicateEmployeeErrorMessage));
            }
        }

        Result<Employee> updateResult = await _employeeRepository.UpdateAsync(employee);
        if (!updateResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(updateResult.ErrorType, updateResult.Errors);
        }

        //Result<Employee> commitResult = await SafeCommitAsync(() => Result<Employee>.Success(employee));
        //if (commitResult.IsSuccess)
        //{
        //    scope.Complete();
        //    return Result<EmployeeResponse>.Success(employee.ToEmployeeResponse());
        //}
        //return Result<EmployeeResponse>.Failure(
        //    commitResult.ErrorType,
        //    commitResult.Errors);

        Result<EmployeeResponse> commitResult = await SafeCommitAsync(() =>
        {
            scope.Complete();
            return Result<EmployeeResponse>.Success(employee.ToEmployeeResponse());
        });

        if (commitResult.IsSuccess)
        {
            return commitResult;
        }
        return Result<EmployeeResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
