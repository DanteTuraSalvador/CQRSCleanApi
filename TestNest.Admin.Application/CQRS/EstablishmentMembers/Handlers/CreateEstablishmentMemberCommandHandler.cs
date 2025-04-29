// Application/CQRS/EstablishmentMembers/Commands/Handlers/CreateEstablishmentMemberCommandHandler.cs
using Microsoft.Extensions.Logging;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Mappings;
using TestNest.Admin.Application.Services.Base;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;

public class CreateEstablishmentMemberCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<CreateEstablishmentMemberCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentMemberRepository establishmentMemberRepository,
    IEstablishmentRepository establishmentRepository,
    IEmployeeRepository employeeRepository,
    IEstablishmentMemberUniquenessChecker uniquenessChecker
) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<CreateEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEstablishmentMemberUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentMemberResponse>> HandleAsync(CreateEstablishmentMemberCommand command)
        => await SafeTransactionAsync(async () =>
        {
            SharedLibrary.Dtos.Requests.Establishment.EstablishmentMemberForCreationRequest creationRequest = command.CreationRequest;

            Result<EstablishmentId> establishmentIdResult = IdHelper
                .ValidateAndCreateId<EstablishmentId>(creationRequest.EstablishmentId);
            Result<EmployeeId> employeeIdResult = IdHelper
                .ValidateAndCreateId<EmployeeId>(creationRequest.EmployeeId);
            Result<MemberTitle> memberTitleResult = MemberTitle.Create(creationRequest.MemberTitle);
            Result<MemberDescription> memberDescriptionResult = MemberDescription.Create(creationRequest.MemberDescription);
            Result<MemberTag> memberTagResult = MemberTag.Create(creationRequest.MemberTag);

            var combinedValidationResult = Result.Combine(
                establishmentIdResult.ToResult(),
                employeeIdResult.ToResult(),
                memberTitleResult.ToResult(),
                memberDescriptionResult.ToResult(),
                memberTagResult.ToResult());

            if (!combinedValidationResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Validation,
                    [.. combinedValidationResult.Errors]);
            }

            Result<Establishment> establishmentResult = await _establishmentRepository
                .GetByIdAsync(establishmentIdResult.Value!);

            if (!establishmentResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    establishmentResult.ErrorType,
                    [.. establishmentResult.Errors]);
            }

            Result<Employee> employeeResult = await _employeeRepository.GetByIdAsync(employeeIdResult.Value!);
            if (!employeeResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.NotFound,
                    new Error("NotFound", $"Employee with ID '{employeeIdResult.Value}' not found."));
            }

            if (employeeResult.Value!.EstablishmentId != establishmentIdResult.Value!)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Validation,
                    new Error("Validation", $"Employee with ID '{employeeIdResult.Value}' does not belong to Establishment with ID '{establishmentIdResult.Value}'."));
            }

            Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckMemberUniquenessAsync(
                employeeIdResult.Value!,
                establishmentIdResult.Value!
            );

            if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Conflict,
                    new Error("Validation", $"Employee with ID '{employeeIdResult.Value}' already exists as a member in this establishment."));
            }

            Result<EstablishmentMember> establishmentMemberResult = EstablishmentMember.Create(
                establishmentId: establishmentIdResult.Value!,
                employeeId: employeeIdResult.Value!,
                title: memberTitleResult.Value!,
                description: memberDescriptionResult.Value!,
                tag: memberTagResult.Value!
            );

            if (!establishmentMemberResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    establishmentMemberResult.ErrorType,
                    [.. establishmentMemberResult.Errors]);
            }

            EstablishmentMember newMember = establishmentMemberResult.Value!;
            _ = await _establishmentMemberRepository.AddAsync(newMember);
            return Result<EstablishmentMemberResponse>.Success(newMember.ToEstablishmentMemberResponse());
        });
}
