using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Application.Specifications.Extensions;
using TestNest.Admin.Domain.Employees;

namespace TestNest.Admin.Application.Specifications.EmployeeSpecifications;

public class EmployeeCountSpecification : BaseSpecification<Employee>
{
    public EmployeeCountSpecification(
        string employeeNumber = null,
        string firstName = null,
        string middleName = null,
        string lastName = null,
        string emailAddress = null,
        int? employeeStatusId = null,
        string employeeRoleId = null,
        string establishmentId = null)
    {
        var spec = new BaseSpecification<Employee>(); 

        if (!string.IsNullOrEmpty(employeeNumber))
        {
            var numberSpec = new BaseSpecification<Employee>(
                e => EF.Functions.Like(e.EmployeeNumber.EmployeeNo.ToLowerInvariant(), $"%{employeeNumber.ToLowerInvariant()}%"));
            spec = spec.And(numberSpec);
        }

        if (!string.IsNullOrEmpty(firstName))
        {
            var firstNameSpec = new BaseSpecification<Employee>(
                e => EF.Functions.Like(e.EmployeeName.FirstName.ToLowerInvariant(), $"%{firstName.ToLowerInvariant()}%"));
            spec = spec.And(firstNameSpec);
        }

        if (!string.IsNullOrEmpty(middleName))
        {
            var middleNameSpec = new BaseSpecification<Employee>(
                e => EF.Functions.Like(e.EmployeeName.MiddleName.ToLowerInvariant(), $"%{middleName.ToLowerInvariant()}%"));
            spec = spec.And(middleNameSpec);
        }

        if (!string.IsNullOrEmpty(lastName))
        {
            var lastNameSpec = new BaseSpecification<Employee>(
                e => EF.Functions.Like(e.EmployeeName.LastName.ToLowerInvariant(), $"%{lastName.ToLowerInvariant()}%"));
            spec = spec.And(lastNameSpec);
        }

        if (!string.IsNullOrEmpty(emailAddress))
        {
            var emailAddressSpec = new BaseSpecification<Employee>(
                e => EF.Functions.Like(e.EmployeeEmail.Email.ToLowerInvariant(), $"%{emailAddress.ToLowerInvariant()}%"));
            spec = spec.And(emailAddressSpec);
        }

        if (employeeStatusId.HasValue)
        {
            spec = spec.And(new BaseSpecification<Employee>(e => e.EmployeeStatus.Id == employeeStatusId.Value));
        }

        if (!string.IsNullOrEmpty(employeeRoleId))
        {
            spec = spec.And(new BaseSpecification<Employee>(e => e.EmployeeRoleId.Value.ToString() == employeeRoleId));
        }

        if (!string.IsNullOrEmpty(establishmentId))
        {
            spec = spec.And(new BaseSpecification<Employee>(e => e.EstablishmentId.Value.ToString() == establishmentId));
        }

        Criteria = spec.Criteria;
    }
}
