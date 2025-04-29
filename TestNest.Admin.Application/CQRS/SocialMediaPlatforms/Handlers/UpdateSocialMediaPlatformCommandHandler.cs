using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.SocialMediaPlatform;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class UpdateSocialMediaPlatformCommandHandler(
    ISocialMediaPlatformRepository socialMediaRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateSocialMediaPlatformCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<UpdateSocialMediaPlatformCommand, Result<SocialMediaPlatformResponse>>
{
    private readonly ISocialMediaPlatformRepository _socialMediaRepository = socialMediaRepository;

    public async Task<Result<SocialMediaPlatformResponse>> HandleAsync(UpdateSocialMediaPlatformCommand command)
    {
        SocialMediaId socialMediaId = command.Id;
        SocialMediaPlatformForUpdateRequest updateRequest = command.Request;

        Result<SocialMediaPlatform> existingPlatformResult = await _socialMediaRepository.GetByIdAsync(socialMediaId);
        if (!existingPlatformResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(existingPlatformResult.ErrorType, existingPlatformResult.Errors);
        }

        SocialMediaPlatform existingPlatform = existingPlatformResult.Value!;
        await _socialMediaRepository.DetachAsync(existingPlatform); 

        Result<SocialMediaName> socialMediaNameResult = SocialMediaName.Create(updateRequest.Name, updateRequest.PlatformURL);
        if (!socialMediaNameResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(ErrorType.Validation, socialMediaNameResult.Errors);
        }

        Result<SocialMediaPlatform> updatedPlatformResult = existingPlatform.WithSocialMediaName(socialMediaNameResult.Value!);
        if (!updatedPlatformResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(updatedPlatformResult.ErrorType, updatedPlatformResult.Errors);
        }

        Result<SocialMediaPlatform> updateDbResult = await _socialMediaRepository.UpdateAsync(updatedPlatformResult.Value!);
        if (!updateDbResult.IsSuccess)
        {
            return Result<SocialMediaPlatformResponse>.Failure(updateDbResult.ErrorType, updateDbResult.Errors);
        }

        return await SafeCommitAsync(() => Result<SocialMediaPlatformResponse>.Success(updateDbResult.Value!.ToSocialMediaPlatformResponse()));
    }
}
