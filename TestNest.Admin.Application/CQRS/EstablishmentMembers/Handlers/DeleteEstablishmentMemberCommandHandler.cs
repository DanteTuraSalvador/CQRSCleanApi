using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class DeleteEstablishmentMemberCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteEstablishmentMemberCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentMemberRepository establishmentMemberRepository
) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<DeleteEstablishmentMemberCommand, Result>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;

    public async Task<Result> HandleAsync(DeleteEstablishmentMemberCommand command)
        => await SafeTransactionAsync(async () =>
        {
            EstablishmentMemberId establishmentMemberId = command.EstablishmentMemberId;

            Result<EstablishmentMember> existingMemberResult = await _establishmentMemberRepository
                .GetByIdAsync(establishmentMemberId);

            if (!existingMemberResult.IsSuccess)
            {
                return Result.Failure(ErrorType.NotFound,
                    new Error("NotFound", $"EstablishmentMember with ID '{establishmentMemberId}' not found."));
            }

            Result deleteResult = await _establishmentMemberRepository.DeleteAsync(establishmentMemberId);

            return deleteResult.IsSuccess
                ? Result.Success()
                : Result.Failure(deleteResult.ErrorType, deleteResult.Errors);
        });
}
