using System.Transactions;
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.Establishments.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class UpdateEstablishmentCommandHandler(
    IEstablishmentRepository establishmentRepository,
    ILogger<UpdateEstablishmentCommandHandler> logger,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory databaseExceptionHandlerFactory,
    IEstablishmentUniquenessChecker uniquenessChecker)
        : BaseService(unitOfWork, logger, databaseExceptionHandlerFactory),
          ICommandHandler<UpdateEstablishmentCommand, Result<EstablishmentResponse>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<EstablishmentResponse>> HandleAsync(UpdateEstablishmentCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

        Result<Establishment> validatedEstablishment = await _establishmentRepository
            .GetByIdAsync(command.EstablishmentId);
        if (!validatedEstablishment.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                 validatedEstablishment.ErrorType,
                 validatedEstablishment.Errors);
        }

        Establishment establishment = validatedEstablishment.Value!;
        await _establishmentRepository.DetachAsync(establishment);

        Result<EstablishmentName> establishmentName = EstablishmentName
            .Create(command.EstablishmentForUpdateRequest.EstablishmentName);
        Result<EmailAddress> establishmentEmail = EmailAddress
            .Create(command.EstablishmentForUpdateRequest.EstablishmentEmail);
        Result<EstablishmentStatus> establishmentStatusResult = EstablishmentStatus
            .FromId(command.EstablishmentForUpdateRequest.EstablishmentStatusId);

        if (!establishmentStatusResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                establishmentStatusResult.ErrorType,
                establishmentStatusResult.Errors);
        }

        var combinedValidationResult = Result.Combine(
            establishmentName.ToResult(),
            establishmentEmail.ToResult(),
            establishmentStatusResult.ToResult());

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                ErrorType.Validation,
                [.. combinedValidationResult.Errors]);
        }

        Result<Establishment> updatedEstablishmentResult = establishment
            .WithName(establishmentName.Value!)
            .Bind(e => e.WithEmail(establishmentEmail.Value!))
            .Bind(e => e.WithStatus(establishmentStatusResult.Value!));

        if (!updatedEstablishmentResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                ErrorType.Validation,
                [.. updatedEstablishmentResult.Errors]);
        }

        Result<bool> uniquenessCheckResult = await uniquenessChecker.CheckNameAndEmailUniquenessAsync(
            establishmentName.Value!,
            establishmentEmail.Value!,
            command.EstablishmentId);

        if (!uniquenessCheckResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                ErrorType.Conflict,
                [.. uniquenessCheckResult.Errors]);
        }

        Result<Establishment> updateResult = await _establishmentRepository
            .UpdateAsync(updatedEstablishmentResult.Value!);
        if (!updateResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                updateResult.ErrorType,
                updateResult.Errors);
        }

        Result<Establishment> commitResult = await SafeCommitAsync(
            () => Result<Establishment>.Success(updatedEstablishmentResult.Value!));
        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentResponse>.Success(
                commitResult.Value!.ToEstablishmentResponse());
        }
        return Result<EstablishmentResponse>.Failure(
            commitResult.ErrorType,
            commitResult.Errors);
    }
}
