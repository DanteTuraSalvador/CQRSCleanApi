using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;
public record CreateEmployeeRoleCommand(EmployeeRoleForCreationRequest Request) : ICommand;
