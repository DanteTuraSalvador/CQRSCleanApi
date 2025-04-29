using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using System.Transactions;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
public class DeleteEmployeeRoleCommandHandler(
    IEmployeeRoleRepository employeeRoleRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEmployeeRoleCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory) : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<DeleteEmployeeRoleCommand, Result>
{
    private readonly IEmployeeRoleRepository _employeeRoleRepository = employeeRoleRepository;

    public async Task<Result> HandleAsync(DeleteEmployeeRoleCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        EmployeeRoleId employeeRoleId = command.Id;
        Result deleteResult = await _employeeRoleRepository.DeleteAsync(employeeRoleId);
        if (!deleteResult.IsSuccess)
        {
            return deleteResult;
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
