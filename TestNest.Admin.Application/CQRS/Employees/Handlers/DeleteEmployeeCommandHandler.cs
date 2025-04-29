using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.Employees.Commands;
using TestNest.Admin.SharedLibrary.Common.Results;
using Microsoft.Extensions.Logging;
using System.Transactions;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;
public class DeleteEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmployeeCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<DeleteEmployeeCommand, Result>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result> HandleAsync(DeleteEmployeeCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result result = await _employeeRepository.DeleteAsync(command.EmployeeId);

        if (!result.IsSuccess)
        {
            return result;
        }

        Result<bool> commitResult = await SafeCommitAsync(() => Result<bool>.Success(true));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result.Success();
        }
        return Result.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
