using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.SocialMediaPlatform;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;
public record CreateSocialMediaPlatformCommand(SocialMediaPlatformForCreationRequest Request) : ICommand;
