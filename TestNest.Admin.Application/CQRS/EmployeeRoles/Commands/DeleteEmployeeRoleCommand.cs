using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;

public record DeleteEmployeeRoleCommand(EmployeeRoleId Id) : ICommand;
