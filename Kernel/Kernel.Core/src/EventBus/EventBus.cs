using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Kernel.Core.SoFucingFastAlgorithms;
using Kernel.DataInterfaces;

namespace Kernel.Core.EventBus;

public unsafe class EventBus<T> where T : ITlarcData
{
    public static EventBus<T> Instance => LazyInstance.Value;
    private static readonly Lazy<EventBus<T>> LazyInstance = new(() => new EventBus<T>());

    /// 事件类型和对应处理器
    private readonly ConcurrentDictionary<string, Action<T>[]> _handlers = new();

    private HybridDictionary<Action<T>[]> _fastHandlers = new([]);

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
    public void Subscribe(string name, Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _handlersLock.EnterWriteLock();

        if (!_handlers.TryGetValue(name, out var handlers))
        {
            handlers = [];
            _handlers[name] = handlers;
        }

        _handlers[name] = [..handlers, handler];
        _handlersLock.ExitWriteLock();
        _fastHandlers = new HybridDictionary<Action<T>[]>(_handlers);
    }

    /// <summary>
    ///     发布事件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(string name, T data)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));

        if (!_fastHandlers.TryGetValue(name, out var handlers))
            return;
        // var validHandlers = handlers.ToList();
        foreach (var handler in handlers)
            handler(data);
    }
}

public unsafe class EventBus
{
    public static EventBus Instance => LazyInstance.Value;
    private static readonly Lazy<EventBus> LazyInstance = new(() => new EventBus());

    /// 事件类型和对应处理器
    private readonly ConcurrentDictionary<string, Action[]> _handlers = new();

    private HybridDictionary<Action[]> _fastHandlers = new([]);

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
    public void Subscribe(string name, Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        _handlersLock.EnterWriteLock();

        if (!_handlers.TryGetValue(name, out var handlers))
        {
            handlers = [];
            _handlers[name] = handlers;
        }

        _handlers[name] = [.. handlers, handler];
        _handlersLock.ExitWriteLock();
        _fastHandlers = new HybridDictionary<Action[]>(_handlers);
    }

    /// <summary>
    ///     发布事件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(string name)
    {
        if (!_fastHandlers.TryGetValue(name, out var handlers))
            return;

        foreach (var handler in handlers)
            handler();
    }
}