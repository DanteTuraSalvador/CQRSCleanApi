using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;
public record CreateEstablishmentMemberCommand(
    EstablishmentMemberForCreationRequest CreationRequest
) : ICommand;
