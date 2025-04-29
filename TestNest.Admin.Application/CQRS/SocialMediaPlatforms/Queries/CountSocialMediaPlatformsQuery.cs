using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.SoicalMediaPlatfomrSpecifications;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
public record CountSocialMediaPlatformsQuery(SocialMediaPlatformSpecification Specification) : IQuery<Result<int>>;
