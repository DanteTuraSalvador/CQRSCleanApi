using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.SharedLibrary.Common.Results;
using System.Transactions;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class DeleteSocialMediaPlatformCommandHandler(
    ISocialMediaPlatformRepository socialMediaRepository,
    IUnitOfWork unitOfWork,
    ILogger<DeleteSocialMediaPlatformCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<DeleteSocialMediaPlatformCommand, Result>
{
    private readonly ISocialMediaPlatformRepository _socialMediaRepository = socialMediaRepository;

    public async Task<Result> HandleAsync(DeleteSocialMediaPlatformCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result deleteResult = await _socialMediaRepository.DeleteAsync(command.Id);

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
