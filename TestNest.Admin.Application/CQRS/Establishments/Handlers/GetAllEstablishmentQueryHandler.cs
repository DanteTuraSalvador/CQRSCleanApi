using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Establishments.Queries;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class GetAllEstablishmentQueryHandler(IEstablishmentRepository establishmentRepository)
    : IQueryHandler<GetAllEstablishmentQuery, Result<IEnumerable<EstablishmentResponse>>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    public async Task<Result<IEnumerable<EstablishmentResponse>>> HandleAsync(GetAllEstablishmentQuery query)
    {
        Result<IEnumerable<Establishment>> establishmentsResult = await _establishmentRepository.ListAsync(query.Spec);
        return establishmentsResult.IsSuccess
            ? Result<IEnumerable<EstablishmentResponse>>.Success(establishmentsResult.Value!.Select(e => e.ToEstablishmentResponse()))
            : Result<IEnumerable<EstablishmentResponse>>.Failure(establishmentsResult.ErrorType, establishmentsResult.Errors);
    }
}
