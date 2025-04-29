using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class GetEstablishmentMemberByIdQueryHandler(IEstablishmentMemberRepository establishmentMemberRepository)
    : IQueryHandler<GetEstablishmentMemberByIdQuery, Result<EstablishmentMemberResponse>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;

    public async Task<Result<EstablishmentMemberResponse>> HandleAsync(GetEstablishmentMemberByIdQuery query)
    {
        Result<EstablishmentMember> establishmentMemberResult = await _establishmentMemberRepository.GetByIdAsync(query.EstablishmentMemberId);
        return establishmentMemberResult.IsSuccess
            ? Result<EstablishmentMemberResponse>.Success(establishmentMemberResult.Value!.ToEstablishmentMemberResponse())
            : Result<EstablishmentMemberResponse>.Failure(establishmentMemberResult.ErrorType, establishmentMemberResult.Errors);
    }
}
