using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;

public record UpdateEmployeeRoleCommand(EmployeeRoleId Id, EmployeeRoleForUpdateRequest Request) : ICommand;
