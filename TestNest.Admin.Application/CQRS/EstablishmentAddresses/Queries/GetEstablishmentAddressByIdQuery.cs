using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;

public record GetEstablishmentAddressByIdQuery(EstablishmentAddressId EstablishmentAddressId) : IQuery<Result<EstablishmentAddressResponse>>;

