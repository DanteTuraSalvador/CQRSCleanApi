using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.EmployeeSpecifications;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Responses;

namespace TestNest.Admin.Application.CQRS.Employees.Queries;
public record GetAllEmployeesQuery(EmployeeSpecification Specification) : IQuery<Result<IEnumerable<EmployeeResponse>>>;
