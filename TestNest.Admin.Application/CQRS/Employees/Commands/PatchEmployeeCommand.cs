using Microsoft.AspNetCore.JsonPatch;
using TestNest.Admin.Application.Contracts;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.CQRS.Employees.Commands;

public record PatchEmployeeCommand(EmployeeId EmployeeId, JsonPatchDocument<EmployeePatchRequest> PatchDocument) : ICommand;
