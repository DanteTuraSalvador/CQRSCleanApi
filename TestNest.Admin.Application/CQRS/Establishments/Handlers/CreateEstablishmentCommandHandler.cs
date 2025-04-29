using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Establishments.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class CreateEstablishmentCommandHandler(
    IEstablishmentRepository establishmentRepository,
    ILogger<CreateEstablishmentCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentUniquenessChecker uniquenessChecker)
        : BaseService(unitOfWork, logger, exceptionHandlerFactory),
        ICommandHandler<CreateEstablishmentCommand, Result<EstablishmentResponse>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEstablishmentUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentResponse>> HandleAsync(CreateEstablishmentCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentName> establishmentNameResult = EstablishmentName
            .Create(command.EstablishmentForCreationRequest.EstablishmentName);
        Result<EmailAddress> establishmentEmailResult = EmailAddress
            .Create(command.EstablishmentForCreationRequest.EstablishmentEmail);

        var combinedValidationResult = Result.Combine(
            establishmentNameResult.ToResult(),
            establishmentEmailResult.ToResult());

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                 ErrorType.Validation,
                 [.. combinedValidationResult.Errors]);
        }
        Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckNameAndEmailUniquenessAsync(
            establishmentNameResult.Value!, establishmentEmailResult.Value!);

        if (!uniquenessCheckResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                ErrorType.Conflict,
                [.. uniquenessCheckResult.Errors]);
        }

        Result<Establishment> establishmentResult = Establishment
            .Create(establishmentNameResult.Value!,
                establishmentEmailResult.Value!);

        if (!establishmentResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                ErrorType.Validation,
                [.. establishmentResult.Errors]);
        }

        Establishment establishment = establishmentResult.Value!;
        _ = await _establishmentRepository.AddAsync(establishment);

        Result<Establishment> commitResult = await SafeCommitAsync(
            () => Result<Establishment>.Success(establishment));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentResponse>.Success(
                establishment.ToEstablishmentResponse());
        }
        return Result<EstablishmentResponse>.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
