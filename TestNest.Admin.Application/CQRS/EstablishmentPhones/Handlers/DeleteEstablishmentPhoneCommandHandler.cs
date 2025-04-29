using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;

public class DeleteEstablishmentPhoneCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteEstablishmentPhoneCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentPhoneRepository establishmentPhoneRepository) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<DeleteEstablishmentPhoneCommand, Result>
{
    private readonly IEstablishmentPhoneRepository _establishmentPhoneRepository = establishmentPhoneRepository;

    public async Task<Result> HandleAsync(DeleteEstablishmentPhoneCommand command)
        => await SafeTransactionAsync(async () =>
        {
            Result<EstablishmentPhone> existingPhoneResult = await _establishmentPhoneRepository
                .GetByIdAsync(command.EstablishmentPhoneId);
            if (!existingPhoneResult.IsSuccess)
            {
                return Result.Failure(existingPhoneResult.ErrorType, existingPhoneResult.Errors);
            }

            Result deleteResult = await _establishmentPhoneRepository.DeleteAsync(command.EstablishmentPhoneId);
            return deleteResult.IsSuccess ? Result.Success() : deleteResult;
        });
}
