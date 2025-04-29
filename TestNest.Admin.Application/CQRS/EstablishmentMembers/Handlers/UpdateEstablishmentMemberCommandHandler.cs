// Application/CQRS/EstablishmentMembers/Commands/Handlers/UpdateEstablishmentMemberCommandHandler.cs
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
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;

public class UpdateEstablishmentMemberCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<UpdateEstablishmentMemberCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentMemberRepository establishmentMemberRepository,
    IEstablishmentRepository establishmentRepository,
    IEmployeeRepository employeeRepository,
    IEstablishmentMemberUniquenessChecker uniquenessChecker
) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<UpdateEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEstablishmentMemberUniquenessChecker _uniquenessChecker = uniquenessChecker;

    public async Task<Result<EstablishmentMemberResponse>> HandleAsync(UpdateEstablishmentMemberCommand command)
        => await SafeTransactionAsync(async () =>
        {
            EstablishmentMemberForUpdateRequest updateRequest = command.UpdateRequest;
            EstablishmentMemberId establishmentMemberId = command.EstablishmentMemberId;

            Result<EstablishmentId> establishmentIdResult = IdHelper
                .ValidateAndCreateId<EstablishmentId>(updateRequest.EstablishmentId);
            Result<MemberTitle> memberTitleResult = MemberTitle.Create(updateRequest.MemberTitle!);
            Result<MemberDescription> memberDescriptionResult = MemberDescription.Create(updateRequest.MemberDescription!);
            Result<MemberTag> memberTagResult = MemberTag.Create(updateRequest.MemberTag!);

            var combinedValidationResult = Result.Combine(
                establishmentIdResult.ToResult(),
                memberTitleResult.ToResult(),
                memberDescriptionResult.ToResult(),
                memberTagResult.ToResult());

            if (!combinedValidationResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Validation,
                    [.. combinedValidationResult.Errors]);
            }

            EstablishmentId updatedEstablishmentId = establishmentIdResult.Value!;
            bool establishmentExists = await _establishmentRepository.ExistsAsync(updatedEstablishmentId);
            if (!establishmentExists)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.NotFound,
                    new Error("NotFound", $"Establishment with ID '{updatedEstablishmentId}' not found."));
            }

            Result<EstablishmentMember> existingMemberResult = await _establishmentMemberRepository
                .GetByIdAsync(establishmentMemberId);
            if (!existingMemberResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    existingMemberResult.ErrorType,
                    [.. existingMemberResult.Errors]);
            }
            EstablishmentMember existingMember = existingMemberResult.Value!;
            await _establishmentMemberRepository.DetachAsync(existingMember);

            if (existingMember.EstablishmentId != updatedEstablishmentId)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Unauthorized,
                    new Error("Unauthorized", $"Cannot update member. The provided EstablishmentId '{updatedEstablishmentId}' does not match the existing member's EstablishmentId '{existingMember.EstablishmentId}'."));
            }

            Result<Employee> employeeResult = await _employeeRepository.GetByIdAsync(existingMember.EmployeeId);
            if (!employeeResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.NotFound,
                    new Error("NotFound", $"Employee with ID '{existingMember.EmployeeId}' not found."));
            }

            if (employeeResult.Value!.EstablishmentId != updatedEstablishmentId)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Validation,
                    new Error("Validation", $"Employee with ID '{existingMember.EmployeeId}' does not belong to Establishment with ID '{updatedEstablishmentId}'."));
            }

            Result<EstablishmentMember> updatedMemberResult = existingMember
                .WithTitle(memberTitleResult.Value!)
                .Bind(member => member.WithDescription(memberDescriptionResult.Value!))
                .Bind(member => member.WithTag(memberTagResult.Value!));

            if (!updatedMemberResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    updatedMemberResult.ErrorType,
                    [.. updatedMemberResult.Errors]);
            }

            EstablishmentMember updatedMember = updatedMemberResult.Value!;

            Result<bool> uniquenessCheckResult = await _uniquenessChecker.CheckMemberUniquenessAsync(
                updatedMember.EmployeeId,
                updatedEstablishmentId,
                establishmentMemberId);

            if (!uniquenessCheckResult.IsSuccess || uniquenessCheckResult.Value)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Conflict,
                    new Error("Validation", $"Employee with ID '{updatedMember.EmployeeId}' already exists as a member in this establishment."));
            }

            Result<EstablishmentMember> updateResult = await _establishmentMemberRepository.UpdateAsync(updatedMember);
            return updateResult.IsSuccess
                ? Result<EstablishmentMemberResponse>.Success(updateResult.Value!.ToEstablishmentMemberResponse())
                : Result<EstablishmentMemberResponse>.Failure(updateResult.ErrorType, [.. updateResult.Errors]);
        });
}
