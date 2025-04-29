using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Employees.Commands;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
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

public class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateEmployeeCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<UpdateEmployeeCommand, Result<EmployeeResponse>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<EmployeeResponse>> HandleAsync(UpdateEmployeeCommand command)
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

        Result<EmployeeNumber> employeeNumberResult = EmployeeNumber.Create(command.EmployeeForUpdateRequest.EmployeeNumber);
        Result<PersonName> employeeNameResult = PersonName.Create(command.EmployeeForUpdateRequest.FirstName,
            command.EmployeeForUpdateRequest.MiddleName, command.EmployeeForUpdateRequest.LastName);
        Result<EmailAddress> employeeEmailResult = EmailAddress.Create(command.EmployeeForUpdateRequest.EmailAddress);
        Result<EmployeeRoleId> employeeRoleIdResult = EmployeeRoleId.Create(command.EmployeeForUpdateRequest.EmployeeRoleId);
        Result<EstablishmentId> establishmentIdResult = EstablishmentId.Create(command.EmployeeForUpdateRequest.EstablishmentId);
        Result<EmployeeStatus> employeeStatusResult = EmployeeStatus.FromId(command.EmployeeForUpdateRequest.EmployeeStatusId);

        if (!employeeStatusResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(
                employeeStatusResult.ErrorType,
                employeeStatusResult.Errors
            );
        }

        var combinedValidationResult = Result.Combine(
            employeeNumberResult.ToResult(),
            employeeNameResult.ToResult(),
            employeeEmailResult.ToResult(),
            employeeRoleIdResult.ToResult(),
            establishmentIdResult.ToResult(),
            employeeStatusResult.ToResult()
        );

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(
                ErrorType.Validation,
                [.. combinedValidationResult.Errors]);
        }

        Employee updatedEmployeeResult = employee
            .WithEmployeeNumber(employeeNumberResult.Value!)
            .Bind(e => e.WithPersonName(employeeNameResult.Value!))
            .Bind(e => e.WithEmail(employeeEmailResult.Value!))
            .Bind(e => e.WithEmployeeRole(employeeRoleIdResult.Value!))
            .Bind(e => e.WithEstablishmentId(establishmentIdResult.Value!))
            .Bind(e => e.WithEmployeeStatus(employeeStatusResult.Value!)).Value!;

        bool employeeExists = await _employeeRepository.EmployeeExistsWithSameCombination(
            command.EmployeeId, 
            employeeNumberResult.Value!,
            employeeNameResult.Value!,
            employeeEmailResult.Value!,
            establishmentIdResult.Value!
        );

        if (employeeExists)
        {
            return Result<EmployeeResponse>.Failure(
                ErrorType.Conflict,
                [new Error(EmployeeException.DuplicateResource.Code.ToString(), EmployeeException.DuplicateEmployeeErrorMessage)]);
        }

        Result<Employee> updateResult = await _employeeRepository.UpdateAsync(updatedEmployeeResult);
        if (!updateResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(
                updateResult.ErrorType,
                updateResult.Errors);
        }

        Result<EmployeeResponse> commitResult = await SafeCommitAsync(() =>
        {
            scope.Complete();
            return Result<EmployeeResponse>.Success(updatedEmployeeResult.ToEmployeeResponse());
        });

        if (commitResult.IsSuccess)
        {
            return commitResult;
        }
        return Result<EmployeeResponse>.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
