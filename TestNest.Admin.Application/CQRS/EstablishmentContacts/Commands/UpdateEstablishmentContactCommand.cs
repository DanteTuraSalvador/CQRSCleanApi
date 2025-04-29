using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
public record UpdateEstablishmentContactCommand(
    EstablishmentContactId EstablishmentContactId,
    EstablishmentContactForUpdateRequest UpdateRequest) : ICommand;
