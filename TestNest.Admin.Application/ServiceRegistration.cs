using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestNest.Admin.Application.Contracts; 
using TestNest.Admin.Application.Contracts.Interfaces.Service;
using TestNest.Admin.Application.CQRS.Dispatcher;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Commands;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Handlers;
using TestNest.Admin.Application.CQRS.EmployeeRoles.Queries;
using TestNest.Admin.Application.CQRS.Employees.Commands;
using TestNest.Admin.Application.CQRS.Employees.Handlers;
using TestNest.Admin.Application.CQRS.Employees.Queries;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Commands;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Handlers;
using TestNest.Admin.Application.CQRS.EstablishmentAddresses.Queries;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Commands;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Handlers;
using TestNest.Admin.Application.CQRS.EstablishmentContacts.Queries;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Commands;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Handlers;
using TestNest.Admin.Application.CQRS.EstablishmentMembers.Queries;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Commands;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Handlers;
using TestNest.Admin.Application.CQRS.EstablishmentPhones.Queries;
using TestNest.Admin.Application.CQRS.Establishments.Commands;
using TestNest.Admin.Application.CQRS.Establishments.Handlers;
using TestNest.Admin.Application.CQRS.Establishments.Queries;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Commands;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Handlers;
using TestNest.Admin.Application.CQRS.SocialMediaPlatforms.Queries;
using TestNest.Admin.Application.Exceptions;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Paginations;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;

namespace TestNest.Admin.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddScoped<IDatabaseExceptionHandlerFactory, SqlServerExceptionHandlerFactory>();

        _ = services.AddScoped<IEstablishmentUniquenessChecker, EstablishmentUniquenessChecker>();
        _ = services.AddScoped<IEstablishmentAddressUniquenessChecker, EstablishmentAddressUniquenessChecker>();
        _ = services.AddScoped<IEstablishmentContactUniquenessChecker, EstablishmentContactUniquenessChecker>();
        _ = services.AddScoped<IEstablishmentPhoneUniquenessChecker, EstablishmentPhoneUniquenessChecker>();
        _ = services.AddScoped<IEstablishmentMemberUniquenessChecker, EstablishmentMemberUniquenessChecker>();

        _ = services.AddScoped<ICommandHandler<CreateEmployeeRoleCommand, Result<EmployeeRoleResponse>>, CreateEmployeeRoleCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEmployeeRoleCommand, Result<EmployeeRoleResponse>>, UpdateEmployeeRoleCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEmployeeRoleCommand, Result>, DeleteEmployeeRoleCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetEmployeeRoleByIdQuery, Result<EmployeeRoleResponse>>, GetEmployeeRoleByIdQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEmployeeRolesQuery, Result<PaginatedResponse<EmployeeRoleResponse>>>, GetEmployeeRolesQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEmployeeCommand, Result<EmployeeResponse>>, CreateEmployeeCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEmployeeCommand, Result<EmployeeResponse>>, UpdateEmployeeCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEmployeeCommand, Result>, DeleteEmployeeCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEmployeeCommand, Result<EmployeeResponse>>, PatchEmployeeCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetAllEmployeesQuery, Result<IEnumerable<EmployeeResponse>>>, GetAllEmployeesQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEmployeesQuery, Result<int>>, CountEmployeesQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEmployeeByIdQuery, Result<EmployeeResponse>>, GetEmployeeByIdQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateSocialMediaPlatformCommand, Result<SocialMediaPlatformResponse>>, CreateSocialMediaPlatformCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateSocialMediaPlatformCommand, Result<SocialMediaPlatformResponse>>, UpdateSocialMediaPlatformCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteSocialMediaPlatformCommand, Result>, DeleteSocialMediaPlatformCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetAllSocialMediaPlatformsQuery, Result<IEnumerable<SocialMediaPlatformResponse>>>, GetAllSocialMediaPlatformsQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountSocialMediaPlatformsQuery, Result<int>>, CountSocialMediaPlatformsQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetSocialMediaPlatformByIdQuery, Result<SocialMediaPlatformResponse>>, GetSocialMediaPlatformByIdQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEstablishmentCommand, Result<EstablishmentResponse>>, CreateEstablishmentCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEstablishmentCommand, Result<EstablishmentResponse>>, UpdateEstablishmentCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEstablishmentCommand, Result>, DeleteEstablishmentCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEstablishmentCommand, Result<EstablishmentResponse>>, PatchEstablishmentCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetAllEstablishmentQuery, Result<IEnumerable<EstablishmentResponse>>>, GetAllEstablishmentQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEstablishmentQuery, Result<int>>, CountEstablishmentQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentByIdQuery, Result<EstablishmentResponse>>, GetEstablishmentByIdQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>, CreateEstablishmentAddressCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEstablishmentAddressCommand, Result>, DeleteEstablishmentAddressCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>, UpdateEstablishmentAddressCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEstablishmentAddressCommand, Result<EstablishmentAddressResponse>>, PatchEstablishmentAddressCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetAllEstablishmentAddressQuery, Result<IEnumerable<EstablishmentAddressResponse>>>, GetAllEstablishmentAddressQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEstablishmentAddressQuery, Result<int>>, CountEstablishmentAddressQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentAddressByIdQuery, Result<EstablishmentAddressResponse>>, GetEstablishmentAddressByIdQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEstablishmentContactCommand, Result<EstablishmentContactResponse>>, CreateEstablishmentContactCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEstablishmentContactCommand, Result<EstablishmentContactResponse>>, UpdateEstablishmentContactCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEstablishmentContactCommand, Result<EstablishmentContactResponse>>, PatchEstablishmentContactCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEstablishmentContactCommand, Result>, DeleteEstablishmentContactCommandHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentContactByIdQuery, Result<EstablishmentContactResponse>>, GetEstablishmentContactByIdQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentContactsQuery, Result<IEnumerable<EstablishmentContactResponse>>>, GetEstablishmentContactsQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEstablishmentContactsQuery, Result<int>>, CountEstablishmentContactsQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>, CreateEstablishmentPhoneCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEstablishmentPhoneCommand, Result>, DeleteEstablishmentPhoneCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>, UpdateEstablishmentPhoneCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEstablishmentPhoneCommand, Result<EstablishmentPhoneResponse>>, PatchEstablishmentPhoneCommandHandler>();
        _ = services.AddScoped<IQueryHandler<CheckEstablishmentPhoneExistsQuery, Result<bool>>, CheckEstablishmentPhoneExistsQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEstablishmentPhonesQuery, Result<int>>, CountEstablishmentPhonesQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentPhoneByIdQuery, Result<EstablishmentPhoneResponse>>, GetEstablishmentPhoneByIdQueryHandler>();
        _ = services.AddScoped<IQueryHandler<ListEstablishmentPhonesQuery, Result<IEnumerable<EstablishmentPhoneResponse>>>, ListEstablishmentPhonesQueryHandler>();

        _ = services.AddScoped<ICommandHandler<CreateEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>, CreateEstablishmentMemberCommandHandler>();
        _ = services.AddScoped<ICommandHandler<DeleteEstablishmentMemberCommand, Result>, DeleteEstablishmentMemberCommandHandler>();
        _ = services.AddScoped<ICommandHandler<UpdateEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>, UpdateEstablishmentMemberCommandHandler>();
        _ = services.AddScoped<ICommandHandler<PatchEstablishmentMemberCommand, Result<EstablishmentMemberResponse>>, PatchEstablishmentMemberCommandHandler>();
        _ = services.AddScoped<IQueryHandler<CheckEstablishmentMemberWithEmployeeExistsQuery, Result<bool>>, CheckEstablishmentMemberWithEmployeeExistsQueryHandler>();
        _ = services.AddScoped<IQueryHandler<CountEstablishmentMembersQuery, Result<int>>, CountEstablishmentMembersQueryHandler>();
        _ = services.AddScoped<IQueryHandler<GetEstablishmentMemberByIdQuery, Result<EstablishmentMemberResponse>>, GetEstablishmentMemberByIdQueryHandler>();
        _ = services.AddScoped<IQueryHandler<ListEstablishmentMembersQuery, Result<IEnumerable<EstablishmentMemberResponse>>>, ListEstablishmentMembersQueryHandler>();

        _ = services.AddScoped<IDispatcher, Dispatcher>();

        return services;
    }
}
