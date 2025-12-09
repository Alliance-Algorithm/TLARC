using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using Kernel.Core.SoFuckingFastAlgorithms;
using Kernel.Contract;

namespace Kernel.Core.EventBus;

public unsafe class EventBus<T> where T : ITlarcData
{
    public static EventBus<T> Instance => LazyInstance.Value;
    private static readonly Lazy<EventBus<T>> LazyInstance = new(() => new EventBus<T>());

    /// 事件类型和对应处理器
    private readonly ConcurrentDictionary<string, (Task[],Action<T>[])> _handlers = new();

    private HybridDictionary<(Task[],Action<T>[])> _fastHandlers = new([]);

    /// 同步锁
    private readonly ReaderWriterLockSlim _handlersLock = new();

    private readonly Lock _runningMapLock = new();

    /// 防止外部实例化
    private EventBus()
    {
        if (!typeof(T).IsInterface) Console.WriteLine($"EventBus<T> better use a interface in Project:Kernel.Contract as T, where T is {typeof(T).FullName}");
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
            handlers = ([],[]);
            _handlers[name] = handlers;
        }

        _handlers[name] = ([.. handlers.Item1, Task.CompletedTask],[.. handlers.Item2, handler]);
        _fastHandlers = new HybridDictionary<(Task[],Action<T>[])>(_handlers);
        _handlersLock.ExitWriteLock();
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
        for(int i = 0; i < handlers.Item1.Length; i++)
        {
            var action = handlers.Item2[i];
            if(handlers.Item1[i].IsCompleted)
                handlers.Item1[i] = Task.Run( () => action(data) );
        }
    }
}

public unsafe class EventBus
{
    public static EventBus Instance => LazyInstance.Value;
    private static readonly Lazy<EventBus> LazyInstance = new(() => new EventBus());

    /// 事件类型和对应处理器
    private readonly ConcurrentDictionary<string, (Task[],Action[])> _handlers = new();

    private HybridDictionary<(Task[],Action[])> _fastHandlers = new([]);

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
            handlers = ([],[]);
            _handlers[name] = handlers;
        }

        _handlers[name] = ([.. handlers.Item1, Task.CompletedTask],[.. handlers.Item2, handler]);
        _handlersLock.ExitWriteLock();
        _fastHandlers = new HybridDictionary<(Task[],Action[])>(_handlers);
    }

    /// <summary>
    ///     发布事件
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Publish(string name)
    {
        if (!_fastHandlers.TryGetValue(name, out var handlers))
            return;
        for(int i = 0; i < handlers.Item1.Length; i++)
        {
            if(handlers.Item1[i].IsCompleted)
                handlers.Item1[i] = Task.Run( () => handlers.Item2[i]() );
        }
    }
}
