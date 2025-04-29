using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class ListEstablishmentMembersQueryHandler(IEstablishmentMemberRepository establishmentMemberRepository)
    : IQueryHandler<ListEstablishmentMembersQuery, Result<IEnumerable<EstablishmentMemberResponse>>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;

    public async Task<Result<IEnumerable<EstablishmentMemberResponse>>> HandleAsync(ListEstablishmentMembersQuery query)
    {
        Result<IEnumerable<EstablishmentMember>> establishmentMembersResult = await _establishmentMemberRepository.ListAsync(query.Specification);
        if (!establishmentMembersResult.IsSuccess)
        {
            return Result<IEnumerable<EstablishmentMemberResponse>>.Failure(
                establishmentMembersResult.ErrorType,
                [.. establishmentMembersResult.Errors]);
        }

        IEnumerable<EstablishmentMemberResponse> establishmentMembersResponse = establishmentMembersResult.Value!
            .Select(member => member.ToEstablishmentMemberResponse());

        return Result<IEnumerable<EstablishmentMemberResponse>>.Success(establishmentMembersResponse);
    }
}
