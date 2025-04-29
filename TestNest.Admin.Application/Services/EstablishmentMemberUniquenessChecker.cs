using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.Services;
public class EstablishmentMemberUniquenessChecker(IEstablishmentMemberRepository establishmentMemberRepository) : IEstablishmentMemberUniquenessChecker
{
    private readonly IEstablishmentMemberRepository _establishmentMemberRepository = establishmentMemberRepository;

    public async Task<Result<bool>> CheckMemberUniquenessAsync(
        EmployeeId employeeId,
        EstablishmentId establishmentId,
        EstablishmentMemberId? excludedMemberId = null)
    {
        Result<EstablishmentMemberId> idResult = excludedMemberId == null
            ? IdHelper.ValidateAndCreateId<EstablishmentMemberId>(Guid.NewGuid().ToString())
            : IdHelper.ValidateAndCreateId<EstablishmentMemberId>(excludedMemberId.Value.ToString());

        if (!idResult.IsSuccess)
        {
            return Result<bool>.Failure(
                idResult.ErrorType,
                idResult.Errors);
        }

        EstablishmentMemberId idToCheck = idResult.Value!;

        bool exists = await _establishmentMemberRepository.MemberExistsForEmployeeInEstablishment(
            idToCheck,
            employeeId,
            establishmentId);

        return Result<bool>.Success(exists);
    }
}
