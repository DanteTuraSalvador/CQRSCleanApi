using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class GetAllEstablishmentAddressQueryHandler(IEstablishmentAddressRepository establishmentAddressRepository)
    : IQueryHandler<GetAllEstablishmentAddressQuery, Result<IEnumerable<EstablishmentAddressResponse>>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    public async Task<Result<IEnumerable<EstablishmentAddressResponse>>> HandleAsync(GetAllEstablishmentAddressQuery query)
    {
        Result<IEnumerable<EstablishmentAddress>> establishmentAddressesResult = await _establishmentAddressRepository.ListAsync(query.Spec);
        return establishmentAddressesResult.IsSuccess
            ? Result<IEnumerable<EstablishmentAddressResponse>>.Success(establishmentAddressesResult.Value!.Select(e => e.ToEstablishmentAddressResponse()))
            : Result<IEnumerable<EstablishmentAddressResponse>>.Failure(establishmentAddressesResult.ErrorType, establishmentAddressesResult.Errors);
    }
}
