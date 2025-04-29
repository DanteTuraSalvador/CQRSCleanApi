using Microsoft.AspNetCore.Mvc;
using TestNest.Admin.API.Helpers;
using TestNest.Admin.Application.Contracts; // For IDispatcher
using TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Paginations;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeRolesController(IDispatcher dispatcher, IErrorResponseService errorResponseService) : ControllerBase
{
    private readonly IDispatcher _dispatcher = dispatcher;
    private readonly IErrorResponseService _errorResponseService = errorResponseService;

    /// <summary>
    /// Creates a new employee role.
    /// </summary>
    /// <param name="employeeRoleForCreationRequest">The data for the new employee role.</param>
    /// <returns>The newly created employee role.</returns>
    /// <response code="201">Returns the newly created employee role.</response>
    /// <response code="400">If the request is invalid or validation fails.</response>
    /// <response code="409">If an employee role with the same name already exists.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(EmployeeRoleResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEmployeeRole(
        [FromBody] EmployeeRoleForCreationRequest employeeRoleForCreationRequest)
    {
        var command = new CreateEmployeeRoleCommand(employeeRoleForCreationRequest);
        Result<EmployeeRoleResponse> result = await _dispatcher.SendCommandAsync<CreateEmployeeRoleCommand, Result<EmployeeRoleResponse>>(command);
        return result.IsSuccess ? CreatedAtAction(nameof(GetAllEmployeeRoles), new { employeeRoleId = result.Value!.Id }, result.Value) : HandleErrorResponse(result.ErrorType, result.Errors);
    }

    /// <summary>
    /// Updates an existing employee role.
    /// </summary>
    /// <param name="employeeRoleId">The ID of the employee role to update.</param>
    /// <param name="employeeRoleForUpdateRequest">The updated data for the employee role.</param>
    /// <returns>The updated employee role.</returns>
    /// <response code="200">Returns the updated employee role.</response>
    /// <response code="400">If the request is invalid or validation fails.</response>
    /// <response code="404">If the employee role with the given ID is not found.</response>
    /// <response code="409">If an employee role with the same name already exists (excluding the one being updated).</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("{employeeRoleId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeRoleResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEmployeeRole(
        string employeeRoleId,
        [FromBody] EmployeeRoleForUpdateRequest employeeRoleForUpdateRequest)
    {
        Result<EmployeeRoleId> employeeRoleIdValidatedResult = IdHelper
            .ValidateAndCreateId<EmployeeRoleId>(employeeRoleId);

        if (!employeeRoleIdValidatedResult.IsSuccess)
        {
            return HandleErrorResponse(
                employeeRoleIdValidatedResult.ErrorType,
                employeeRoleIdValidatedResult.Errors);
        }

        var command = new UpdateEmployeeRoleCommand(employeeRoleIdValidatedResult.Value!, employeeRoleForUpdateRequest);
        Result<EmployeeRoleResponse> result = await _dispatcher.SendCommandAsync<UpdateEmployeeRoleCommand, Result<EmployeeRoleResponse>>(command);
        return result.IsSuccess ? Ok(result.Value!) : HandleErrorResponse(result.ErrorType, result.Errors);
    }

    /// <summary>
    /// Deletes an employee role.
    /// </summary>
    /// <param name="employeeRoleId">The ID of the employee role to delete.</param>
    /// <returns>No content if the deletion was successful.</returns>
    /// <response code="204">If the employee role was successfully deleted.</response>
    /// <response code="400">If the provided employeeRoleId is invalid.</response>
    /// <response code="404">If the employee role with the given ID is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("{employeeRoleId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEmployeeRole(string employeeRoleId)
    {
        Result<EmployeeRoleId> employeeRoleIdValidatedResult = IdHelper.ValidateAndCreateId<EmployeeRoleId>(employeeRoleId);

        if (!employeeRoleIdValidatedResult.IsSuccess)
        {
            return HandleErrorResponse(
                ErrorType.Validation,
                employeeRoleIdValidatedResult.Errors);
        }

        var command = new DeleteEmployeeRoleCommand(employeeRoleIdValidatedResult.Value!);
        Result result = await _dispatcher.SendCommandAsync<DeleteEmployeeRoleCommand, Result>(command);
        return result.IsSuccess ? NoContent() : HandleErrorResponse(result.ErrorType, result.Errors);
    }

    /// <summary>
    /// Gets a list of employee roles with optional filtering, sorting, and pagination, or a single employee role by ID.
    /// </summary>
    /// <param name="pageNumber">The page number for pagination (default: 1).</param>
    /// <param name="pageSize">The page size for pagination (default: 10).</param>
    /// <param name="sortBy">The field to sort by (default: EmployeeRoleId).</param>
    /// <param name="sortOrder">The sort order ("asc" or "desc", default: "asc").</param>
    /// <param name="roleName">Optional filter by role name.</param>
    /// <param name="employeeRoleId">Optional filter by employee role ID to get a single role.</param>
    /// <returns>A list of employee roles or a single employee role.</returns>
    /// <response code="200">Returns a list of employee roles or a single employee role.</response>
    /// <response code="400">If the provided employeeRoleId is invalid.</response>
    /// <response code="404">If no employee roles are found matching the criteria or the requested ID.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet(Name = "GetEmployeeRoles")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<EmployeeRoleResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeRoleResponse))] // For single role by ID
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllEmployeeRoles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "EmployeeRoleId",
        [FromQuery] string sortOrder = "asc",
        [FromQuery] string roleName = null,
        [FromQuery] string employeeRoleId = null)
    {
        if (!string.IsNullOrEmpty(employeeRoleId))
        {
            Result<EmployeeRoleId> employeeRoleIdValidatedResult = IdHelper
                .ValidateAndCreateId<EmployeeRoleId>(employeeRoleId);

            if (!employeeRoleIdValidatedResult.IsSuccess)
            {
                return HandleErrorResponse(employeeRoleIdValidatedResult.ErrorType, employeeRoleIdValidatedResult.Errors);
            }

            var query = new GetEmployeeRoleByIdQuery(employeeRoleIdValidatedResult.Value!);
            Result<EmployeeRoleResponse> result = await _dispatcher.SendQueryAsync<GetEmployeeRoleByIdQuery, Result<EmployeeRoleResponse>>(query);
            return result.IsSuccess ? Ok(result.Value!) : HandleErrorResponse(result.ErrorType, result.Errors);
        }
        else
        {
            var query = new GetEmployeeRolesQuery(pageNumber, pageSize, sortBy, sortOrder, roleName, employeeRoleId);
            Result<PaginatedResponse<EmployeeRoleResponse>> result = await _dispatcher.SendQueryAsync<GetEmployeeRolesQuery, Result<PaginatedResponse<EmployeeRoleResponse>>>(query);
            return result.IsSuccess ? Ok(result.Value!) : HandleErrorResponse(result.ErrorType, result.Errors);
        }
    }

    private IActionResult HandleErrorResponse(ErrorType errorType, IEnumerable<Error> errors)
    {
        List<Error> safeErrors = errors?.ToList() ?? [];

        return errorType switch
        {
            ErrorType.Validation => _errorResponseService.CreateProblemDetails(StatusCodes.Status400BadRequest, "Validation Error", "Validation failed.", new Dictionary<string, object> { { "errors", safeErrors } }),
            ErrorType.NotFound => _errorResponseService.CreateProblemDetails(StatusCodes.Status404NotFound, "Not Found", "Resource not found.", new Dictionary<string, object> { { "errors", safeErrors } }),
            ErrorType.Conflict => _errorResponseService.CreateProblemDetails(StatusCodes.Status409Conflict, "Conflict", "Resource conflict.", new Dictionary<string, object> { { "errors", safeErrors } }),
            _ => _errorResponseService.CreateProblemDetails(StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };
    }
}

