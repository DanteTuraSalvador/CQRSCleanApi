using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class CountEstablishmentContactsQueryHandler(IEstablishmentContactRepository establishmentContactRepository)
    : IQueryHandler<CountEstablishmentContactsQuery, Result<int>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    public async Task<Result<int>> HandleAsync(CountEstablishmentContactsQuery query)
        => await _establishmentContactRepository.CountAsync(query.Spec);
}
