using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class GetEstablishmentContactByIdQueryHandler(IEstablishmentContactRepository establishmentContactRepository)
    : IQueryHandler<GetEstablishmentContactByIdQuery, Result<EstablishmentContactResponse>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    public async Task<Result<EstablishmentContactResponse>> HandleAsync(GetEstablishmentContactByIdQuery query)
    {
        Result<EstablishmentContact> establishmentContactResult = await _establishmentContactRepository.GetByIdAsync(query.EstablishmentContactId);
        return establishmentContactResult.IsSuccess
            ? Result<EstablishmentContactResponse>.Success(establishmentContactResult.Value!.ToEstablishmentContactResponse())
            : Result<EstablishmentContactResponse>.Failure(establishmentContactResult.ErrorType, establishmentContactResult.Errors);
    }
}
