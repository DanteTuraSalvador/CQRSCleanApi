using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
public record GetEstablishmentContactByIdQuery(EstablishmentContactId EstablishmentContactId)
    : IQuery<Result<EstablishmentContactResponse>>;
