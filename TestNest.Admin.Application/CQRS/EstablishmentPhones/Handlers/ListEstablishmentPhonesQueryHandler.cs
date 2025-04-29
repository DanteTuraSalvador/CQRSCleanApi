using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class ListEstablishmentPhonesQueryHandler(IEstablishmentPhoneRepository establishmentPhoneRepository)
    : IQueryHandler<ListEstablishmentPhonesQuery, Result<IEnumerable<EstablishmentPhoneResponse>>>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;
    public async Task<Result<IEnumerable<EstablishmentPhoneResponse>>> HandleAsync(ListEstablishmentPhonesQuery query)
    {
        Result<IEnumerable<EstablishmentPhone>> establishmentPhonesResult = await _establishmentPhoneRepository.ListAsync(query.Spec);
        return establishmentPhonesResult.IsSuccess
            ? Result<IEnumerable<EstablishmentPhoneResponse>>.Success(establishmentPhonesResult.Value!.Select(p => p.ToEstablishmentPhoneResponse()))
            : Result<IEnumerable<EstablishmentPhoneResponse>>.Failure(establishmentPhonesResult.ErrorType, establishmentPhonesResult.Errors);
    }
}
