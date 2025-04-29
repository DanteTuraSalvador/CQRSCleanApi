using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.Employees.Queries;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;
public class GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository)
    : IQueryHandler<GetEmployeeByIdQuery, Result<EmployeeResponse>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<EmployeeResponse>> HandleAsync(GetEmployeeByIdQuery query)
    {
        Result<Employee> employeeResult = await _employeeRepository.GetByIdAsync(query.EmployeeId);
        return employeeResult.IsSuccess
            ? Result<EmployeeResponse>.Success(employeeResult.Value!.ToEmployeeResponse())
            : Result<EmployeeResponse>.Failure(employeeResult.ErrorType, employeeResult.Errors);
    }
}
