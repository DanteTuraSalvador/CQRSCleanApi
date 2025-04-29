using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.Establishments.Commands;
public record CreateEstablishmentCommand(EstablishmentForCreationRequest EstablishmentForCreationRequest) : ICommand;
