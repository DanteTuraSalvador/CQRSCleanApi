using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;

public record CheckEstablishmentPhoneExistsQuery(
    PhoneNumber PhoneNumber,
    EstablishmentId EstablishmentId,
    EstablishmentPhoneId? ExcludedPhoneId = null
) : IQuery<Result<bool>>;
