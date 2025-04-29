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
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.Establishments.Handlers;
public class PatchEstablishmentCommandHandler(IEstablishmentRepository establishmentRepository,
    IUnitOfWork unitOfWork,
    ILogger<PatchEstablishmentCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentUniquenessChecker uniquenessChecker)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory), ICommandHandler<PatchEstablishmentCommand, Result<EstablishmentResponse>>
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEstablishmentUniquenessChecker _uniquenessChecker = uniquenessChecker; 

    public async Task<Result<EstablishmentResponse>> HandleAsync(PatchEstablishmentCommand command)
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

        ////var establishmentPatchRequest = new EstablishmentPatchRequest();
        ////command.PatchDocument.ApplyTo(establishmentPatchRequest);


        if (command.EstablishmentPatchRequest.EstablishmentName != null)
        {
            Result<EstablishmentName> establishmentNameResult = EstablishmentName
                .Create(command.EstablishmentPatchRequest.EstablishmentName);
            if (!establishmentNameResult.IsSuccess)
            {
                return Result<EstablishmentResponse>.Failure(
                    establishmentNameResult.ErrorType,
                    establishmentNameResult.Errors);
            }

            establishment = establishment
                .WithName(establishmentNameResult.Value!).Value!;
        }

        if (command.EstablishmentPatchRequest.EmailAddress != null)
        {
            Result<EmailAddress> emailResult = EmailAddress
                .Create(command.EstablishmentPatchRequest.EmailAddress);
            if (!emailResult.IsSuccess)
            {
                return Result<EstablishmentResponse>.Failure(
                    emailResult.ErrorType,
                    emailResult.Errors);
            }

            establishment = establishment
                .WithEmail(emailResult.Value!).Value!;
        }

        if (command.EstablishmentPatchRequest.EstablishmentStatus != null)
        {
            EstablishmentStatus currentStatus = establishment.EstablishmentStatus;
            Result<EstablishmentStatus> newStatusResult = EstablishmentStatus
                .FromId(command.EstablishmentPatchRequest.EstablishmentStatus.Value);
            if (!newStatusResult.IsSuccess)
            {
                return Result<EstablishmentResponse>.Failure(
                    newStatusResult.ErrorType,
                    newStatusResult.Errors);
            }

            EstablishmentStatus? newStatus = newStatusResult.Value;
            Result transitionResult = EstablishmentStatus
                .ValidateTransition(currentStatus, newStatus!);
            if (!transitionResult.IsSuccess)
            {
                return Result<EstablishmentResponse>.Failure(
                    transitionResult.ErrorType,
                    transitionResult.Errors);
            }

            establishment = establishment.WithStatus(newStatus!).Value!;
        }

        if (command.EstablishmentPatchRequest.EstablishmentName != null || command.EstablishmentPatchRequest.EmailAddress != null)
        {
            Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckNameAndEmailUniquenessAsync(
                establishment.EstablishmentName,
                establishment.EstablishmentEmail,
                command.EstablishmentId);

            if (!uniquenessCheckResult.IsSuccess)
            {
                var exception = EstablishmentException.DuplicateResource();
                return Result<EstablishmentResponse>.Failure(ErrorType.Conflict,
                    new Error(exception.Code.ToString(), exception.Message.ToString()));
            }
        }

        Result<Establishment> updateResult = await _establishmentRepository
            .UpdateAsync(establishment);
        if (!updateResult.IsSuccess)
        {
            return Result<EstablishmentResponse>.Failure(
                updateResult.ErrorType,
                updateResult.Errors);
        }

        Result<Establishment> commitResult = await SafeCommitAsync(
            () => Result<Establishment>.Success(establishment));
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
