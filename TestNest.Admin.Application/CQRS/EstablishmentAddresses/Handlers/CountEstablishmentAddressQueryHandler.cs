using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class CountEstablishmentAddressQueryHandler(IEstablishmentAddressRepository establishmentAddressRepository)
    : IQueryHandler<CountEstablishmentAddressQuery, Result<int>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    public async Task<Result<int>> HandleAsync(CountEstablishmentAddressQuery query)
        => await _establishmentAddressRepository.CountAsync(query.Spec);
}
