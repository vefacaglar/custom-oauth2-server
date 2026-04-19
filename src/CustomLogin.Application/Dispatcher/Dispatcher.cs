using CustomLogin.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace CustomLogin.Application.Dispatcher;

public interface IDispatcher
{
    Task<Result> Send(ICommand command, CancellationToken ct = default);
    Task<Result<TResult>> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default);
    Task<Result<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken ct = default);
}

public sealed class MediatorDispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public MediatorDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<Result> Send(ICommand command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        var task = (Task)handleMethod.Invoke(handler, [command, ct])!;
        await task.ConfigureAwait(false);

        var resultProperty = task.GetType().GetProperty("Result")
            ?? throw new InvalidOperationException("Task has no Result property.");

        return (Result)resultProperty.GetValue(task)!;
    }

    public async Task<Result<TResult>> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        var task = (Task<Result<TResult>>)handleMethod.Invoke(handler, [command, ct])!;
        return await task.ConfigureAwait(false);
    }

    public async Task<Result<TResult>> Query<TResult>(IQuery<TResult> query, CancellationToken ct = default)
    {
        var queryType = query.GetType();
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        var handleMethod = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handle method not found on {handlerType.Name}");

        var task = (Task<Result<TResult>>)handleMethod.Invoke(handler, [query, ct])!;
        return await task.ConfigureAwait(false);
    }
}
