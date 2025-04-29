using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
public record PatchEstablishmentAddressCommand(EstablishmentAddressId EstablishmentAddressId,
       EstablishmentAddressPatchRequest EstablishmentAddressPatchRequest) : ICommand;

