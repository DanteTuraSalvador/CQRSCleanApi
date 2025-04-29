using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class GetEstablishmentPhoneByIdQueryHandler(IEstablishmentPhoneRepository establishmentPhoneRepository)
    : IQueryHandler<GetEstablishmentPhoneByIdQuery, Result<EstablishmentPhoneResponse>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    public async Task<Result<EstablishmentPhoneResponse>> HandleAsync(GetEstablishmentPhoneByIdQuery query)
    {
        Result<EstablishmentPhone> establishmentPhoneResult = await _establishmentPhoneRepository.GetByIdAsync(query.EstablishmentPhoneId);
        return establishmentPhoneResult.IsSuccess
            ? Result<EstablishmentPhoneResponse>.Success(establishmentPhoneResult.Value!.ToEstablishmentPhoneResponse())
            : Result<EstablishmentPhoneResponse>.Failure(establishmentPhoneResult.ErrorType, establishmentPhoneResult.Errors);
    }
}
