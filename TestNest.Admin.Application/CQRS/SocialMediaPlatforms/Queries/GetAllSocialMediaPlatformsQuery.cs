using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.SoicalMediaPlatfomrSpecifications;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
public record GetAllSocialMediaPlatformsQuery(SocialMediaPlatformSpecification Specification) : IQuery<Result<IEnumerable<SocialMediaPlatformResponse>>>;
