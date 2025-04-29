using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Establishments.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class CountEstablishmentQueryHandler(IEstablishmentRepository establishmentRepository) : IQueryHandler<CountEstablishmentQuery, Result<int>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    public async Task<Result<int>> HandleAsync(CountEstablishmentQuery query) => await _establishmentRepository.CountAsync(query.Spec);
}
