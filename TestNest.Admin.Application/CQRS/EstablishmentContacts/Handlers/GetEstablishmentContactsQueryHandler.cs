using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class GetEstablishmentContactsQueryHandler(IEstablishmentContactRepository establishmentContactRepository)
    : IQueryHandler<GetEstablishmentContactsQuery, Result<IEnumerable<EstablishmentContactResponse>>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    public async Task<Result<IEnumerable<EstablishmentContactResponse>>> HandleAsync(GetEstablishmentContactsQuery query)
    {
        Result<IEnumerable<EstablishmentContact>> contactsResult = await _establishmentContactRepository.ListAsync(query.Spec);
        return contactsResult.IsSuccess
            ? Result<IEnumerable<EstablishmentContactResponse>>.Success(
                contactsResult.Value!.Select(c => c.ToEstablishmentContactResponse()))
            : Result<IEnumerable<EstablishmentContactResponse>>.Failure(contactsResult.ErrorType, contactsResult.Errors);
    }
}
