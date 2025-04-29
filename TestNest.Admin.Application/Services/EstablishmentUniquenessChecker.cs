using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Helpers;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Services;
public class EstablishmentUniquenessChecker(IEstablishmentRepository establishmentRepository) : IEstablishmentUniquenessChecker
{
    private readonly IEstablishmentRepository _establishmentRepository = establishmentRepository;

    public async Task<Result<bool>> CheckNameAndEmailUniquenessAsync(
        EstablishmentName establishmentName,
        EmailAddress emailAddress,
        EstablishmentId? establishmentId = null)
    {
        Result<EstablishmentId> idResult = establishmentId == null
            ? IdHelper.ValidateAndCreateId<EstablishmentId>(Guid.NewGuid().ToString())
            : IdHelper.ValidateAndCreateId<EstablishmentId>(establishmentId.Value.ToString());

        if (!idResult.IsSuccess)
        {
            return Result<bool>.Failure(
                idResult.ErrorType, idResult.Errors);
        }

        EstablishmentId idToCheck = idResult.Value!;

        bool exists = await _establishmentRepository
            .ExistsWithNameAndEmailAsync(establishmentName, emailAddress, idToCheck);

        if (exists)
        {
            return Result<bool>.Failure(ErrorType.Conflict,
                new Error(EstablishmentException.DuplicateResource().Code.ToString(),
                    EstablishmentException.DuplicateResource().Message.ToString()));
        }

        return Result<bool>.Success(false);
    }
}
