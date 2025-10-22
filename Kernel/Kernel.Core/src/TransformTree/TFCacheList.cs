using System.Numerics;
using Microsoft.Extensions.ObjectPool;

namespace Kernel.Core.TransformTree;

internal unsafe class TFCacheListNode
{
    public TFCacheListNode? Next = null;
    public TFCacheListNode? Prev = null;

    public long Time;

    public Matrix4x4 Transfrom;

    private bool _dirty;
    
    public bool Dirty
    {
        get => Volatile.Read(ref _dirty);
        set => Volatile.Write(ref _dirty, value);
    }
}

internal struct TFCacheListNodePolicy : IPooledObjectPolicy<TFCacheListNode>
{
    public readonly TFCacheListNode Create() => new();

    public readonly bool Return(TFCacheListNode obj) => true;
}


internal unsafe class TFCacheList
{
    DefaultObjectPool<TFCacheListNode> _objectPool = new(new TFCacheListNodePolicy());

    TFCacheListNode? _head = null;
    TFCacheListNode? _tail = null;
    public bool Find(long time, out TFCacheListNode? node)
    {   
        if(time <= 0) 
        {
            node = _head;
            return true;
        };
        node = null;
        if (_head == null)
            return false;

        if (time < _tail!.Time)
            return false;

        node = _head;
        while (node.Next != null && node!.Time > time) node = node!.Next;
        return true;
    }

    public void SetDirty(long time)
    {
        if (_head == null)
        {
            _head           = _objectPool.Get();
            _tail           = _head;
            _head.Time      = time;
            _head.Dirty     = true;
            _head.Prev      = null;
            _head.Next      = null;
            return;
        }
        if(time <= 0)
        {
            _head!.Dirty = true;
            return;
        }
        while(_tail!.Prev != null && time - ((long)10 << 32) > _tail.Time)
        {
            var tail    = _tail;
            _tail       = _tail.Prev;
            _objectPool.Return(tail);
        }

        if (_tail!.Time > time)
        {
            var tail        = _tail;
            _tail           = _objectPool.Get();
            _tail.Time      = time;
            _tail.Dirty     = true;
            _tail.Prev      = tail;
            tail.Next       = _tail;
            _tail.Next      = null;
            return;
        }
        var node = _head;
        while (node!.Time > time) node = node!.Next;
        if (node!.Time == time)
        {
            node.Dirty = true;
            return;
        }
        var newNode = _objectPool.Get();
        newNode.Time = time;
        newNode.Dirty = true;
        newNode.Next        = node;
        newNode.Prev        = node.Prev;
        node.Prev           = newNode;
        if (node == _head) _head = newNode;
        else newNode.Prev!.Next  = newNode;
    }
}