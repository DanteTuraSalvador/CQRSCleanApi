using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.Employees.Commands;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using System.Transactions;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Interfaces; // For IDatabaseExceptionHandlerFactory
using TestNest.Admin.SharedLibrary.Exceptions.Common; // For ErrorType and Error
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Application.Contracts.Common; // For BaseService

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;

public class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    ILogger<CreateEmployeeCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<CreateEmployeeCommand, Result<EmployeeResponse>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<EmployeeResponse>> HandleAsync(CreateEmployeeCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result<EmployeeNumber> employeeNumberResult = EmployeeNumber.Create(command.EmployeeForCreationRequest.EmployeeNumber);
        Result<PersonName> employeeNameResult = PersonName.Create(command.EmployeeForCreationRequest.FirstName,
            command.EmployeeForCreationRequest.MiddleName, command.EmployeeForCreationRequest.LastName);
        Result<EmailAddress> employeeEmailResult = EmailAddress.Create(command.EmployeeForCreationRequest.EmailAddress);
        Result<EmployeeRoleId> employeeRoleIdResult = EmployeeRoleId.Create(command.EmployeeForCreationRequest.EmployeeRoleId);
        Result<EstablishmentId> establishmentIdResult = EstablishmentId.Create(command.EmployeeForCreationRequest.EstablishmentId);

        var combinedValidationResult = Result.Combine(
            employeeNumberResult.ToResult(),
            employeeNameResult.ToResult(),
            employeeEmailResult.ToResult(),
            employeeRoleIdResult.ToResult(),
            establishmentIdResult.ToResult());

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(
                ErrorType.Validation,
                [.. combinedValidationResult.Errors]);
        }

        var employeeExists = await _employeeRepository.EmployeeExistsWithSameCombination(
            EmployeeId.New(),
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

        Result<Employee> employeeResult = Employee
            .Create(employeeNumberResult.Value!,
                employeeNameResult.Value!,
                employeeEmailResult.Value!,
                employeeRoleIdResult.Value!,
                establishmentIdResult.Value!);

        if (!employeeResult.IsSuccess)
        {
            return Result<EmployeeResponse>.Failure(
                ErrorType.Validation,
                [.. employeeResult.Errors]);
        }

        Employee employee = employeeResult.Value!;
        _ = await _employeeRepository.AddAsync(employee);

        Result<Employee> commitResult = await SafeCommitAsync(() => Result<Employee>.Success(employee));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EmployeeResponse>.Success(employee.ToEmployeeResponse());
        }
        return Result<EmployeeResponse>.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
//using TestNest.Admin.Application.Contracts;
//using TestNest.Admin.Application.CQRS.Employees.Commands;
//using TestNest.Admin.SharedLibrary.Common.Results;
//using TestNest.Admin.SharedLibrary.Dtos.Responses;
//using System.Transactions;
//using TestNest.Admin.Application.Contracts.Common;
//using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
//using TestNest.Admin.Domain.Employees;
//using TestNest.Admin.SharedLibrary.Exceptions.Common;
//using TestNest.Admin.SharedLibrary.Exceptions;
//using TestNest.Admin.SharedLibrary.StronglyTypeIds;
//using TestNest.Admin.SharedLibrary.ValueObjects;
//using TestNest.Admin.Application.Mappings;
//using TestNest.Admin.SharedLibrary.Helpers;
//using TestNest.Admin.Application.Services.Base;
//using Microsoft.Extensions.Logging;
//using TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
//using TestNest.Admin.Application.Interfaces;

//namespace TestNest.Admin.Application.CQRS.Employees.Handlers;
//public class CreateEmployeeCommandHandler(
//    IEmployeeRepository employeeRepository,
//    IEmployeeRoleRepository employeeRoleRepository,
//    IEstablishmentRepository establishmentRepository,
//    ILogger<CreateEmployeeRoleCommandHandler> logger,
//    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
//    IUnitOfWork unitOfWork)
//    : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<CreateEmployeeCommand, Result<EmployeeResponse>>
//{
//    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

//    public async Task<Result<EmployeeResponse>> HandleAsync(CreateEmployeeCommand command)
//    {
//        using var scope = new TransactionScope(TransactionScopeOption.Required,
//           new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
//        TransactionScopeAsyncFlowOption.Enabled);

//        Result<EmployeeNumber> employeeNumberResult = EmployeeNumber.Create(command.EmployeeForCreationRequest.EmployeeNumber);
//        Result<PersonName> employeeNameResult = PersonName.Create(command.EmployeeForCreationRequest.FirstName,
//            command.EmployeeForCreationRequest.MiddleName, command.EmployeeForCreationRequest.LastName);
//        Result<EmailAddress> employeeEmailResult = EmailAddress.Create(command.EmployeeForCreationRequest.EmailAddress);
//        Result<EmployeeRoleId> employeeRoleIdResult = EmployeeRoleId.Create(command.EmployeeForCreationRequest.EmployeeRoleId);
//        Result<EstablishmentId> establishmentIdResult = EstablishmentId.Create(command.EmployeeForCreationRequest.EstablishmentId);

//        var combinedValidationResult = Result.Combine(
//            employeeNumberResult.ToResult(),
//            employeeNameResult.ToResult(),
//            employeeEmailResult.ToResult(),
//            employeeRoleIdResult.ToResult(),
//            establishmentIdResult.ToResult());

//        if (!combinedValidationResult.IsSuccess)
//        {
//            return Result<EmployeeResponse>.Failure(
//                ErrorType.Validation,
//                [.. combinedValidationResult.Errors]);
//        }

//        Result<bool> uniquenessCheckResult = await EmployeeCombinationExistsAsync(
//            employeeNumberResult.Value!,
//            employeeNameResult.Value!,
//            employeeEmailResult.Value!,
//            establishmentIdResult.Value!);

//        if (!uniquenessCheckResult.IsSuccess)
//        {
//            return Result<EmployeeResponse>.Failure(
//                uniquenessCheckResult.ErrorType,
//                uniquenessCheckResult.Errors);
//        }

//        Result<Employee> employeeResult = Employee
//            .Create(employeeNumberResult.Value!,
//                employeeNameResult.Value!,
//                employeeEmailResult.Value!,
//                employeeRoleIdResult.Value!,
//                establishmentIdResult.Value!);

//        if (!employeeResult.IsSuccess)
//        {
//            return Result<EmployeeResponse>.Failure(
//                ErrorType.Validation,
//                [.. employeeResult.Errors]);
//        }

//        Employee employee = employeeResult.Value!;
//        _ = await _employeeRepository.AddAsync(employee);

//        Result<Employee> commitResult = await SafeCommitAsync(() => Result<Employee>.Success(employee));
//        if (commitResult.IsSuccess)
//        {
//            scope.Complete();
//            return Result<EmployeeResponse>.Success(employee.ToEmployeeResponse());
//        }
//        return Result<EmployeeResponse>.Failure(
//            commitResult.ErrorType,
//            commitResult.Errors);
//    }
//    private async Task<Result<bool>> EmployeeCombinationExistsAsync(
//      EmployeeNumber employeeNumber,
//      PersonName personName,
//      EmailAddress emailAddress,
//      EstablishmentId establishmentId,
//      EmployeeId? employeeId = null)
//    {
//        Result<EmployeeId> idResult = employeeId == null
//            ? IdHelper.ValidateAndCreateId<EmployeeId>(Guid.NewGuid().ToString())
//            : IdHelper.ValidateAndCreateId<EmployeeId>(employeeId.Value.ToString());

//        if (!idResult.IsSuccess)
//        {
//            return Result<bool>.Failure(
//                idResult.ErrorType,
//                idResult.Errors);
//        }

//        EmployeeId idToCheck = idResult.Value!;

//        bool exists = await _employeeRepository.EmployeeExistsWithSameCombination(
//            idToCheck,
//            employeeNumber,
//            personName,
//            emailAddress,
//            establishmentId);

//        if (exists)
//        {
//            return Result<bool>.Failure(
//                ErrorType.Conflict,
//                new Error(EmployeeException.DuplicateResource.Code.ToString(),
//                    EmployeeException.DuplicateEmployeeErrorMessage));
//        }

//        return Result<bool>.Success(false);
//    }

//}
