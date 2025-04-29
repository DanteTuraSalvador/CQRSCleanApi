using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
public class GetEstablishmentAddressByIdQueryHandler(IEstablishmentAddressRepository establishmentAddressRepository)
    : IQueryHandler<GetEstablishmentAddressByIdQuery, Result<EstablishmentAddressResponse>>
{
    private readonly IEstablishmentAddressRepository _establishmentAddressRepository = establishmentAddressRepository;
    public async Task<Result<EstablishmentAddressResponse>> HandleAsync(GetEstablishmentAddressByIdQuery query)
    {

        Result<EstablishmentAddress> establishmentAddressResult = await _establishmentAddressRepository.GetByIdAsync(query.EstablishmentAddressId);
        return establishmentAddressResult.IsSuccess
            ? Result<EstablishmentAddressResponse>.Success(establishmentAddressResult.Value!.ToEstablishmentAddressResponse())
            : Result<EstablishmentAddressResponse>.Failure(establishmentAddressResult.ErrorType, establishmentAddressResult.Errors);
    }
}
