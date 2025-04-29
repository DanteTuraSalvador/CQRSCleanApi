using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
public record DeleteEstablishmentContactCommand(EstablishmentContactId EstablishmentContactId)
    : ICommand;
