using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;

public record GetAllEstablishmentAddressQuery(ISpecification<EstablishmentAddress> Spec) : IQuery<Result<IEnumerable<EstablishmentAddressResponse>>>;

