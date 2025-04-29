using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
public record CreateEstablishmentContactCommand(EstablishmentContactForCreationRequest CreationRequest)
    : ICommand;
