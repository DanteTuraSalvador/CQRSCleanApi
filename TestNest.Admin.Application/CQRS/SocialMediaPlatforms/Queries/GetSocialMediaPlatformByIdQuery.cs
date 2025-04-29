using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
public record GetSocialMediaPlatformByIdQuery(SocialMediaId SocialMediaPlatformId) : IQuery<Result<SocialMediaPlatformResponse>>;
