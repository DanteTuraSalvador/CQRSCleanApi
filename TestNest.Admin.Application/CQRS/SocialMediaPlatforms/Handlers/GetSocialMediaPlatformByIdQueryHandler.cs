using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class GetSocialMediaPlatformByIdQueryHandler(ISocialMediaPlatformRepository socialMediaRepository)
    : IQueryHandler<GetSocialMediaPlatformByIdQuery, Result<SocialMediaPlatformResponse>>
{
    private readonly ISocialMediaPlatformRepository _socialMediaRepository = socialMediaRepository;

    public async Task<Result<SocialMediaPlatformResponse>> HandleAsync(GetSocialMediaPlatformByIdQuery query)
    {
        Result<SocialMediaPlatform> platformResult = await _socialMediaRepository.GetByIdAsync(query.SocialMediaPlatformId);
        return platformResult.IsSuccess
            ? Result<SocialMediaPlatformResponse>.Success(platformResult.Value!.ToSocialMediaPlatformResponse())
            : Result<SocialMediaPlatformResponse>.Failure(platformResult.ErrorType, platformResult.Errors);
    }
}
