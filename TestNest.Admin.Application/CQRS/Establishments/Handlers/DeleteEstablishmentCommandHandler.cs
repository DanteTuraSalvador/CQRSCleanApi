using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.SharedLibrary.Common.Results;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.CQRS.Establishments.Commands;
using System.Transactions;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class DeleteEstablishmentCommandHandler(
    IEstablishmentRepository establishmentRepository,
    ILogger<DeleteEstablishmentCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
        : BaseService(unitOfWork, logger, exceptionHandlerFactory),
        ICommandHandler<DeleteEstablishmentCommand, Result>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result> HandleAsync(DeleteEstablishmentCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
           new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
           TransactionScopeAsyncFlowOption.Enabled);

        Result result = await _establishmentRepository.DeleteAsync(command.EstablishmentId);
        if (!result.IsSuccess)
        {
            return result;
        }

        Result<bool> commitResult = await SafeCommitAsync(
            () => Result<bool>.Success(true));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result.Success();
        }
        return Result.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
