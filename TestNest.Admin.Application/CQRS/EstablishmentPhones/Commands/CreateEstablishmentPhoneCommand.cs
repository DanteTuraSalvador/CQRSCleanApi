using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;
public record CreateEstablishmentPhoneCommand(EstablishmentPhoneForCreationRequest CreationRequest)
    : ICommand;
