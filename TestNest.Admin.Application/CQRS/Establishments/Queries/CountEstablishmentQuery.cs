using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.Establishments.Queries;
public record CountEstablishmentQuery(ISpecification<Establishment> Spec) : IQuery<Result<int>>;
