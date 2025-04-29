using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions.Common;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class DeleteEstablishmentAddressCommandHandler(
    IEstablishmentAddressRepository establishmentAddressRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteEstablishmentAddressCommandHandler> logger,
    IDatabaseExceptionHandlerFactory databaseExceptionHandlerFactory)
    : BaseService(unitOfWork, logger, databaseExceptionHandlerFactory),
      ICommandHandler<DeleteEstablishmentAddressCommand, Result>
{
    public readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;

    public async Task<Result> HandleAsync(DeleteEstablishmentAddressCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                               new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                               TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentAddress> existingAddressResult = await _establishmentAddressRepository
            .GetByIdAsync(command.EstablishmentAddressId);
        if (!existingAddressResult.IsSuccess)
        {
            return Result.Failure(ErrorType.NotFound,
                                  new Error("NotFound", $"EstablishmentAddress with ID '{command.EstablishmentAddressId}' not found."));
        }

        EstablishmentAddress existingAddress = existingAddressResult.Value!;

        if (existingAddress.IsPrimary)
        {
            return Result.Failure(ErrorType.Validation,
                                  new Error("DeletionNotAllowed", $"Cannot delete the primary address for Establishment ID '{existingAddress.EstablishmentId}'. Please set another address as primary first."));
        }

        Result deleteResult = await _establishmentAddressRepository.DeleteAsync(command.EstablishmentAddressId);
        Result<bool> commitResult = await SafeCommitAsync(() => Result<bool>.Success(true));

        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result.Success();
        }
        return Result.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
