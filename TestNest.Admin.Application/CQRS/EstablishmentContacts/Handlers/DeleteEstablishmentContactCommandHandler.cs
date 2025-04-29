using Microsoft.Extensions.Logging;
using System.Transactions;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions.Common;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class DeleteEstablishmentContactCommandHandler(
    IEstablishmentContactRepository establishmentContactRepository,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    ILogger<DeleteEstablishmentContactCommandHandler> logger)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<DeleteEstablishmentContactCommand, Result>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;

    public async Task<Result> HandleAsync(DeleteEstablishmentContactCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                             new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                             TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentContact> existingContactResult = await _establishmentContactRepository
            .GetByIdAsync(command.EstablishmentContactId);
        if (!existingContactResult.IsSuccess)
        {
            return Result.Failure(ErrorType.NotFound, new Error("NotFound", $"EstablishmentContact with ID '{command.EstablishmentContactId}' not found."));
        }

        EstablishmentContact existingContact = existingContactResult.Value!;

        if (existingContact.IsPrimary)
        {
            return Result.Failure(ErrorType.Validation,
                new Error("DeletionNotAllowed", $"Cannot delete the primary contact for Establishment ID '{existingContact.EstablishmentId}'. Please set another contact as primary first."));
        }

        Result deleteResult = await _establishmentContactRepository.DeleteAsync(command.EstablishmentContactId);
        Result<bool> commitResult = await SafeCommitAsync(() => Result<bool>.Success(true));

        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result.Success();
        }
        return Result.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
