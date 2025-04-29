using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Establishments.Queries;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class GetEstablishmentByIdQueryHandler(IEstablishmentRepository establishmentRepository) : IQueryHandler<GetEstablishmentByIdQuery, Result<EstablishmentResponse>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    public async Task<Result<EstablishmentResponse>> HandleAsync(GetEstablishmentByIdQuery query)
    {
        Result<Establishment> establishmentResult = await _establishmentRepository.GetByIdAsync(query.EstablishmentId);
        return establishmentResult.IsSuccess
            ? Result<EstablishmentResponse>.Success(establishmentResult.Value!.ToEstablishmentResponse())
            : Result<EstablishmentResponse>.Failure(establishmentResult.ErrorType, establishmentResult.Errors);
    }
}
