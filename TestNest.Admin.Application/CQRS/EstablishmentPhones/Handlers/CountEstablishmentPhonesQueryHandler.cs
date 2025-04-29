using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class CountEstablishmentPhonesQueryHandler(IEstablishmentPhoneRepository establishmentPhoneRepository)
    : IQueryHandler<CountEstablishmentPhonesQuery, Result<int>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    public async Task<Result<int>> HandleAsync(CountEstablishmentPhonesQuery query)
        => await _establishmentPhoneRepository.CountAsync(query.Spec);
}
