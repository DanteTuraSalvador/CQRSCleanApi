using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
public record CreateEstablishmentAddressCommand (EstablishmentAddressForCreationRequest EstablishmentAddressForCreationRequest) : ICommand;

