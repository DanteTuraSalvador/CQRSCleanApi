namespace TestNest.Admin.Application.Contracts;
public interface IDispatcher
{
    // For commands (void or return a result)
    Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command)
        where TCommand : ICommand;

    // For queries (always return a response)
    Task<TResponse> SendQueryAsync<TQuery, TResponse>(TQuery query)
        where TQuery : IQuery<TResponse>;
}
