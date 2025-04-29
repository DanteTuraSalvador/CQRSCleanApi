using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.Establishments.Commands;

public record UpdateEstablishmentCommand(EstablishmentId EstablishmentId, EstablishmentForUpdateRequest EstablishmentForUpdateRequest) : ICommand;
