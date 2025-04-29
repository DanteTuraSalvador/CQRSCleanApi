using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.Application.Contracts;

namespace TestNest.Admin.Application.CQRS.Employees.Commands;
public record CreateEmployeeCommand(EmployeeForCreationRequest EmployeeForCreationRequest) : ICommand;
