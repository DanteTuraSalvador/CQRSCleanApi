using Microsoft.Extensions.DependencyInjection;
using TestNest.Admin.Application.Contracts;

namespace TestNest.Admin.Application.CQRS.Dispatcher;
public class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command) where TCommand : ICommand
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        ICommandHandler<TCommand, TResult> handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command);
    }

    public async Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query) where TQuery : IQuery<TResponse>
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IQueryHandler<TQuery, TResponse> handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.HandleAsync(query);
    }
}
