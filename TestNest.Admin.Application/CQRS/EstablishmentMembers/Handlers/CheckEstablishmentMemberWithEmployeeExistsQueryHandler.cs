using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class CheckEstablishmentMemberWithEmployeeExistsQueryHandler(IEstablishmentMemberUniquenessChecker uniquenessChecker)
    : IQueryHandler<CheckEstablishmentMemberWithEmployeeExistsQuery, Result<bool>>
{
    private readonly IEstablishmentMemberUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<bool>> HandleAsync(CheckEstablishmentMemberWithEmployeeExistsQuery query)
        => await _uniquenessChecker.CheckMemberUniquenessAsync(
            query.EmployeeId,
            query.EstablishmentId,
            query.ExcludedMemberId);
}
