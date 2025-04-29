
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.Application.Contracts;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
public record UpdateEstablishmentAddressCommand(EstablishmentAddressId EstablishmentAddressId,
        EstablishmentAddressForUpdateRequest EstablishmentAddressForUpdateRequest) : ICommand;
