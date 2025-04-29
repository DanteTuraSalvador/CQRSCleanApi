using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.Establishments.Commands;

public record DeleteEstablishmentCommand(EstablishmentId EstablishmentId) : ICommand;
