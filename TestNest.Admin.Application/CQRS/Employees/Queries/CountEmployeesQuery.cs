using TestNest.Admin.Application.Contracts;
using TestNest.Admin.Application.Specifications.EmployeeSpecifications;
using TestNest.Admin.SharedLibrary.Common.Results;

namespace TestNest.Admin.Application.CQRS.Employees.Queries;
public record CountEmployeesQuery(EmployeeSpecification Specification) : IQuery<Result<int>>;
