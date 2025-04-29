using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;

public record PatchEstablishmentMemberCommand(
    EstablishmentMemberId EstablishmentMemberId,
    EstablishmentMemberPatchRequest PatchRequest
) : ICommand;
