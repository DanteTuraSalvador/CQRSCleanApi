using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;

public record DeleteSocialMediaPlatformCommand(SocialMediaId Id) : ICommand;
