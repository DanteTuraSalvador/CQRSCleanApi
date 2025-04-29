using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;

public record ListEstablishmentMembersQuery(
    ISpecification<EstablishmentMember> Specification
) : IQuery<Result<IEnumerable<EstablishmentMemberResponse>>>;
