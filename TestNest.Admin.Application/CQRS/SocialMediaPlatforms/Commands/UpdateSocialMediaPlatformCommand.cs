using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.SocialMediaPlatform;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;

public record UpdateSocialMediaPlatformCommand(SocialMediaId Id, SocialMediaPlatformForUpdateRequest Request) : ICommand;
