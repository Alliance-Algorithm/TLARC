using System.Collections.Concurrent;
using Kernel.DataInterfaces;

namespace Kernel.Core.EventBus;

/// <summary>
///     线程安全的事件总线
/// </summary>
public class EventBus
{
    public static EventBus Instance => LazyInstance.Value;
    private static readonly Lazy<EventBus> LazyInstance = new(() => new EventBus());

    /// 事件类型和对应处理器
    private readonly ConcurrentDictionary<string, List<Delegate>> _handlers = new();

    private readonly HashSet<string> _runnings = [];

    /// 同步锁
    private readonly ReaderWriterLockSlim _handlersLock = new();

    private readonly Lock _runningMapLock = new();

    /// 防止外部实例化
    private EventBus()
    {
    }


    /// <summary>
    ///     订阅事件
    /// </summary>
    public void Subscribe<TData>(string name, Action<TData> handler) where TData : ITlarcData
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _handlersLock.EnterWriteLock();

        if (!_handlers.TryGetValue(name, out var handlers))
        {
            handlers = [];
            _handlers[name] = handlers;
        }

        handlers.Add(handler);
        _handlersLock.ExitWriteLock();
    }

    public void Subscribe(string name, Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _handlersLock.EnterWriteLock();

        if (!_handlers.TryGetValue(name, out var handlers))
        {
            handlers = [];
            _handlers[name] = handlers;
        }

        handlers.Add(handler);
        _handlersLock.ExitWriteLock();
    }

    /// <summary>
    ///     发布事件
    /// </summary>
    public void Publish<TData>(string name, TData data) where TData : ITlarcData
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        _handlersLock.EnterReadLock();

        if (_handlers.TryGetValue(name, out var handlers))
        {
            var validHandlers = handlers.OfType<Action<TData>>().ToList();
            Parallel.ForEach(validHandlers, handler => handler(data));
        }

        _handlersLock.ExitReadLock();
    }

    public void Publish(string name)
    {
        _handlersLock.EnterReadLock();

        if (_handlers.TryGetValue(name, out var handlers))
        {
            var validHandlers = handlers.OfType<Action>().ToList();
            Parallel.ForEach(validHandlers, handler => handler());
        }

        _handlersLock.ExitReadLock();
    }
}