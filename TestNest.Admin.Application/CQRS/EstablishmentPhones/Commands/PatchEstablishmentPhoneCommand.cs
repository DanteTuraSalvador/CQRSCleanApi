using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;

public record PatchEstablishmentPhoneCommand(
    EstablishmentPhoneId EstablishmentPhoneId,
    EstablishmentPhonePatchRequest PatchRequest)
    : ICommand;
