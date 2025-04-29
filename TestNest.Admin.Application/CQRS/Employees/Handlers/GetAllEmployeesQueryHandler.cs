using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.Employees.Queries;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.Application.Mappings;

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;
public class GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    : IQueryHandler<GetAllEmployeesQuery, Result<IEnumerable<EmployeeResponse>>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<IEnumerable<EmployeeResponse>>> HandleAsync(GetAllEmployeesQuery query)
    {
        Result<IEnumerable<Employee>> employeesResult = await _employeeRepository.ListAsync(query.Specification);
        return employeesResult.IsSuccess
            ? Result<IEnumerable<EmployeeResponse>>.Success(employeesResult.Value!.Select(e => e.ToEmployeeResponse()))
            : Result<IEnumerable<EmployeeResponse>>.Failure(employeesResult.ErrorType, employeesResult.Errors);
    }
}
