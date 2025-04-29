using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;

public record DeleteEstablishmentMemberCommand(
    EstablishmentMemberId EstablishmentMemberId
) : ICommand;
