using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.CQRS.Employees.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.Employees.Handlers;
public class CountEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    : IQueryHandler<CountEmployeesQuery, Result<int>>
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;

    public async Task<Result<int>> HandleAsync(CountEmployeesQuery query) => await _employeeRepository.CountAsync(query.Specification);
}
