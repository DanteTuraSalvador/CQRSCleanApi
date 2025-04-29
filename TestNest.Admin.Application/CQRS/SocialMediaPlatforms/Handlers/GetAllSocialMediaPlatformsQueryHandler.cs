using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class GetAllSocialMediaPlatformsQueryHandler(ISocialMediaPlatformRepository socialMediaRepository)
    : IQueryHandler<GetAllSocialMediaPlatformsQuery, Result<IEnumerable<SocialMediaPlatformResponse>>>
{
    private readonly ISocialMediaPlatformRepository _socialMediaRepository = socialMediaRepository;

    public async Task<Result<IEnumerable<SocialMediaPlatformResponse>>> HandleAsync(GetAllSocialMediaPlatformsQuery query)
    {
        Result<IEnumerable<SocialMediaPlatform>> platformsResult = await _socialMediaRepository.ListAsync(query.Specification);
        return platformsResult.IsSuccess
            ? Result<IEnumerable<SocialMediaPlatformResponse>>.Success(platformsResult.Value!.Select(p => p.ToSocialMediaPlatformResponse()))
            : Result<IEnumerable<SocialMediaPlatformResponse>>.Failure(platformsResult.ErrorType, platformsResult.Errors);
    }
}
