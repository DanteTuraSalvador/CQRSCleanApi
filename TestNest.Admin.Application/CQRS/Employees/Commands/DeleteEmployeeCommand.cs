using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.Employees.Commands;

public record DeleteEmployeeCommand(EmployeeId EmployeeId) : ICommand;
