using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;

public record UpdateEstablishmentPhoneCommand(
    EstablishmentPhoneId EstablishmentPhoneId,
    EstablishmentPhoneForUpdateRequest UpdateRequest)
    : ICommand;
