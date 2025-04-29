using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class CheckEstablishmentPhoneExistsQueryHandler(IEstablishmentPhoneUniquenessChecker uniquenessChecker)
    : IQueryHandler<CheckEstablishmentPhoneExistsQuery, Result<bool>>
{
    private readonly IEstablishmentPhoneUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<bool>> HandleAsync(CheckEstablishmentPhoneExistsQuery query)
        => await _uniquenessChecker.CheckPhoneUniquenessAsync(
            query.PhoneNumber,
            query.EstablishmentId,
            query.ExcludedPhoneId);
}
