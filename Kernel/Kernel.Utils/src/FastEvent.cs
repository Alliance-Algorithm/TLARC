namespace Kernel.Utils;

public sealed class FastEvent<T>
{
    private readonly Lock _lock = new();
    private Action<T>? _invocationList;
    private Action<T>[]? _invocationArray;

    public void AddHandler(Action<T> handler)
    {
        lock (_lock)
        {
            _invocationList += handler;
            _invocationArray = null; // 使缓存失效
        }
    }

    public void RemoveHandler(Action<T> handler)
    {
        lock (_lock)
        {
            _invocationList -= handler;
            _invocationArray = null;
        }
    }

    public void Raise(T data)
    {
        // 无锁读取当前委托链
        var current = _invocationList;
        if (current == null) return;

        // 使用缓存数组避免委托链修改影响
        var array = _invocationArray ??= current.GetInvocationList().Cast<Action<T>>().ToArray();

        // 快速遍历数组
        foreach (var action in array)
            action(data);
    }
}