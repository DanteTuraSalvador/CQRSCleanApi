using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.Employees.Commands;

public record UpdateEmployeeCommand(EmployeeId EmployeeId, EmployeeForUpdateRequest EmployeeForUpdateRequest) : ICommand;
