using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;

public record UpdateEstablishmentMemberCommand(
    EstablishmentMemberId EstablishmentMemberId,
    EstablishmentMemberForUpdateRequest UpdateRequest
) : ICommand;
