using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
public class CountSocialMediaPlatformsQueryHandler(ISocialMediaPlatformRepository socialMediaRepository)
    : IQueryHandler<CountSocialMediaPlatformsQuery, Result<int>>
{
    private readonly ISocialMediaPlatformRepository _socialMediaRepository = socialMediaRepository;

    public async Task<Result<int>> HandleAsync(CountSocialMediaPlatformsQuery query) => await _socialMediaRepository.CountAsync(query.Specification);
}
