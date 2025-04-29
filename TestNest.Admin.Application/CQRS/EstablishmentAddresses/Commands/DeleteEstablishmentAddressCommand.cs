using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;


namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
public record DeleteEstablishmentAddressCommand (EstablishmentAddressId EstablishmentAddressId) : ICommand;
