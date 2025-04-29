using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;

public record DeleteEstablishmentPhoneCommand(
    EstablishmentPhoneId EstablishmentPhoneId)
    : ICommand;
