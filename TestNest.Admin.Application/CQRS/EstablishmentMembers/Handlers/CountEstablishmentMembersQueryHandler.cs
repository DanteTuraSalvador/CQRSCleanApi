using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class CountEstablishmentMembersQueryHandler(
    ILogger<CountEstablishmentMembersQueryHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentMemberRepository establishmentMemberRepository
) : BaseService(null, logger, exceptionHandlerFactory),
    IQueryHandler<CountEstablishmentMembersQuery, Result<int>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;

    public async Task<Result<int>> HandleAsync(CountEstablishmentMembersQuery query)
        => await _establishmentMemberRepository.CountAsync(query.Spec);
}
