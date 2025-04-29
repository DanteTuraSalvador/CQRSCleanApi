
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
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;

namespace TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
public class PatchEstablishmentMemberCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<PatchEstablishmentMemberCommandHandler> logger,
    IDatabaseExceptionHandlerFactory exceptionHandlerFactory,
    IEstablishmentMemberRepository establishmentMemberRepository,
    IEmployeeRepository employeeRepository
) : BaseService(unitOfWork, logger, exceptionHandlerFactory),
    ICommandHandler<PatchEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<EstablishmentMemberResponse>> HandleAsync(PatchEstablishmentMemberCommand command)
        => await SafeTransactionAsync(async () =>
        {
            EstablishmentMemberId establishmentMemberId = command.EstablishmentMemberId;
            EstablishmentMemberPatchRequest patchRequest = command.PatchRequest;

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

            EstablishmentId updatedEstablishmentId = existingMember.EstablishmentId;
            EmployeeId updatedEmployeeId = existingMember.EmployeeId;
            MemberTitle updatedTitle = existingMember.MemberTitle;
            MemberDescription updatedDescription = existingMember.MemberDescription;
            MemberTag updatedTag = existingMember.MemberTag;

            if (patchRequest.EstablishmentId is not null) 
            {
                Result<EstablishmentId> establishmentIdResult = IdHelper.ValidateAndCreateId<EstablishmentId>(patchRequest.EstablishmentId);
                if (!establishmentIdResult.IsSuccess)
                {
                    return Result<EstablishmentMemberResponse>.Failure(ErrorType.Validation, establishmentIdResult.Errors);
                }
                updatedEstablishmentId = establishmentIdResult.Value!;
            }

            if (patchRequest.EmployeeId is not null) 
            {
                Result<EmployeeId> employeeIdResult = IdHelper.ValidateAndCreateId<EmployeeId>(patchRequest.EmployeeId);
                if (!employeeIdResult.IsSuccess)
                {
                    return Result<EstablishmentMemberResponse>.Failure(ErrorType.Validation, employeeIdResult.Errors);
                }
                updatedEmployeeId = employeeIdResult.Value!;
            }

            if (patchRequest.MemberTitle is not null)
            {
                Result<MemberTitle> titleResult = MemberTitle.Create(patchRequest.MemberTitle);
                if (!titleResult.IsSuccess)
                {
                    return Result<EstablishmentMemberResponse>.Failure(ErrorType.Validation, titleResult.Errors);
                }
                updatedTitle = titleResult.Value!;
            }

            if (patchRequest.MemberDescription is not null)
            {
                Result<MemberDescription> descriptionResult = MemberDescription.Create(patchRequest.MemberDescription);
                if (!descriptionResult.IsSuccess)
                {
                    return Result<EstablishmentMemberResponse>.Failure(ErrorType.Validation, descriptionResult.Errors);
                }
                updatedDescription = descriptionResult.Value!;
            }

            if (patchRequest.MemberTag is not null) 
            {
                Result<MemberTag> tagResult = MemberTag.Create(patchRequest.MemberTag);
                if (!tagResult.IsSuccess)
                {
                    return Result<EstablishmentMemberResponse>.Failure(ErrorType.Validation, tagResult.Errors);
                }
                updatedTag = tagResult.Value!;
            }

            Result<Employee> employeeResult = await _employeeRepository.GetByIdAsync(updatedEmployeeId);
            if (!employeeResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.NotFound,
                    new Error("NotFound", $"Employee with ID '{updatedEmployeeId}' not found."));
            }

            if (employeeResult.Value!.EstablishmentId != updatedEstablishmentId)
            {
                return Result<EstablishmentMemberResponse>.Failure(
                    ErrorType.Validation,
                    new Error("Validation", $"Employee with ID '{updatedEmployeeId}' does not belong to Establishment with ID '{updatedEstablishmentId}'."));
            }

            Result<EstablishmentMember> updatedMemberResult = EstablishmentMember.Create(
                establishmentId: updatedEstablishmentId,
                employeeId: updatedEmployeeId,
                title: updatedTitle,
                description: updatedDescription,
                tag: updatedTag
            );

            if (!updatedMemberResult.IsSuccess)
            {
                return Result<EstablishmentMemberResponse>.Failure(updatedMemberResult.ErrorType, [.. updatedMemberResult.Errors]);
            }

            EstablishmentMember updatedMember = updatedMemberResult.Value!;

            Result<EstablishmentMember> updateResult = await _establishmentMemberRepository.UpdateAsync(updatedMember);
            return updateResult.IsSuccess
                ? Result<EstablishmentMemberResponse>.Success(updateResult.Value!.ToEstablishmentMemberResponse())
                : Result<EstablishmentMemberResponse>.Failure(updateResult.ErrorType, [.. updateResult.Errors]);
        });
}
