using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
public record CountEstablishmentContactsQuery(ISpecification<EstablishmentContact> Spec)
    : IQuery<Result<int>>;
