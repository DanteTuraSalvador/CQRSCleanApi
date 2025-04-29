using Microsoft.Extensions.Logging;
using System.Transactions;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.Application.Mappings;
//using TestNest.Admin.Application.CQRS.Establishments.Queries;

namespace TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
public class CreateEstablishmentContactCommandHandler(
    IEstablishmentContactRepository establishmentContactRepository,
    IUnitOfWork unitOfWork,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    ILogger<CreateEstablishmentContactCommandHandler> logger,
    IEstablishmentContactUniquenessChecker establishmentContactUniquenessChecker
    , IEstablishmentRepository establishmentRepository
    /*, IDispatcher dispatcher*/)
    : BaseService(unitOfWork, logger, exceptionHandlerFactory),
      ICommandHandler<CreateEstablishmentContactCommand, Result<EstablishmentContactResponse>>
{
    private readonly IEstablishmentContactRepository _establishmentContactRepository = establishmentContactRepository;
    private readonly IEstablishmentContactUniquenessChecker _establishmentContactUniquenessChecker = establishmentContactUniquenessChecker;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    //private readonly IDispatcher _dispatcher = dispatcher;

    public async Task<Result<EstablishmentContactResponse>> HandleAsync(CreateEstablishmentContactCommand command)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required,
                                              new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                                              TransactionScopeAsyncFlowOption.Enabled);

        Result<EstablishmentId> establishmentIdResult = IdHelper
            .ValidateAndCreateId<EstablishmentId>(command.CreationRequest.EstablishmentId);

        Result<PersonName> personNameResult = PersonName.Create(
            command.CreationRequest.ContactPersonFirstName,
            command.CreationRequest.ContactPersonMiddleName,
            command.CreationRequest.ContactPersonLastName);

        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create(
            command.CreationRequest.ContactPhoneNumber);

        var combinedValidationResult = Result.Combine(
            establishmentIdResult.ToResult(),
            personNameResult.ToResult(),
            phoneNumberResult.ToResult());

        if (!combinedValidationResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Validation,
                [.. combinedValidationResult.Errors]);
        }

        //var getEstablishmentQuery = new GetEstablishmentByIdQuery(establishmentIdResult.Value!);
        //Result<EstablishmentResponse> establishmentResult = await _dispatcher.SendQueryAsync<GetEstablishmentByIdQuery, Result<EstablishmentResponse>>(getEstablishmentQuery);

        Result<Establishment> establishmentResult = await _establishmentRepository
            .GetByIdAsync(establishmentIdResult.Value!);

        if (!establishmentResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(
                establishmentResult.ErrorType,
                [.. establishmentResult.Errors]);
        }

        Result<bool> uniquenessCheckResult = await _establishmentContactUniquenessChecker.CheckEstablishmentContactUniquenessAsync(
            personNameResult.Value!,
            phoneNumberResult.Value!,
            establishmentIdResult.Value!);

        if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
        {
            return Result<EstablishmentContactResponse>.Failure(
                ErrorType.Conflict,
                new Error("Validation", $"A contact with the same name and phone number already exists for this establishment."));
        }

        if (command.CreationRequest.IsPrimary)
        {
            Result setNonPrimaryResult = await _establishmentContactRepository
                .SetNonPrimaryForEstablishmentContanctAsync(establishmentIdResult.Value!, EstablishmentContactId.Empty());

            if (!setNonPrimaryResult.IsSuccess)
            {
                return Result<EstablishmentContactResponse>.Failure(setNonPrimaryResult.ErrorType, [.. setNonPrimaryResult.Errors]);
            }
        }

        Result<EstablishmentContact> establishmentContactResult = EstablishmentContact.Create(
            establishmentIdResult.Value!,
            personNameResult.Value!,
            phoneNumberResult.Value!,
            command.CreationRequest.IsPrimary);

        if (!establishmentContactResult.IsSuccess)
        {
            return Result<EstablishmentContactResponse>.Failure(
                establishmentContactResult.ErrorType,
                [.. establishmentContactResult.Errors]);
        }

        EstablishmentContact establishmentContact = establishmentContactResult.Value!;
        _ = await _establishmentContactRepository.AddAsync(establishmentContact);
        Result<EstablishmentContact> commitResult = await SafeCommitAsync(() => Result<EstablishmentContact>.Success(establishmentContact));

        if (commitResult.IsSuccess)
        {
            scope.Complete();
            return Result<EstablishmentContactResponse>.Success(commitResult.Value!.ToEstablishmentContactResponse());
        }

        return Result<EstablishmentContactResponse>.Failure(commitResult.ErrorType, commitResult.Errors);
    }
}
