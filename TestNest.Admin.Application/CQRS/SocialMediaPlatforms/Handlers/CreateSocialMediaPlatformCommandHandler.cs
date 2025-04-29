using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;
using System.Transactions;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class CreateSocialMediaPlatformCommandHandler(
    ISocialMediaPlatformRepository socialMediaPlatformRepository,
    ILogger<CreateSocialMediaPlatformCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<CreateSocialMediaPlatformCommand, Result<SocialMediaPlatformResponse>>
{
    private readonly ISocialMediaPlatformRepository _socialMediaPlatformRepository = socialMediaPlatformRepository;

    public async Task<Result<SocialMediaPlatformResponse>> HandleAsync(CreateSocialMediaPlatformCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result<SocialMediaName> socialMediaNameResult = SocialMediaName
            .Create(command.Request.Name, command.Request.PlatformURL);

        if (!socialMediaNameResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(
                ErrorType.Validation,
                [.. socialMediaNameResult.Errors]);
        }

        Result<SocialMediaPlatform> existingPlatformResult = await _socialMediaPlatformRepository
            .GetSocialMediaPlatformByNameAsync(socialMediaNameResult.Value!.Name);

        if (existingPlatformResult.IsSuccess)
        {
            var exception = SocialMediaPlatformException.DuplicateResource();
            return Result<SocialMediaPlatformResponse>.Failure(
                ErrorType.Conflict,
                new Error(exception.Code.ToString(), exception.Message.ToString()));
        }

        Result<SocialMediaPlatform> socialMediaPlatformResult = SocialMediaPlatform
            .Create(socialMediaNameResult.Value!);

        if (!socialMediaPlatformResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(
                ErrorType.Validation,
                [.. socialMediaPlatformResult.Errors]);
        }

        SocialMediaPlatform socialMediaPlatform = socialMediaPlatformResult.Value!;
        _ = await _socialMediaPlatformRepository.AddAsync(socialMediaPlatform);

        Result<SocialMediaPlatform> commitResult = await SafeCommitAsync(
            () => Result<SocialMediaPlatform>.Success(socialMediaPlatform));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<SocialMediaPlatformResponse>.Success(
                commitResult.Value!.ToSocialMediaPlatformResponse());
        }
        return Result<SocialMediaPlatformResponse>.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
