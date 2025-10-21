using System.Numerics;
using System.Runtime.CompilerServices;

namespace Kernel.Core.TransformTree;

internal class TransformCache
{
    private readonly TFCacheList _cache = new();
    private readonly Queue<Matrix4x4> _queue = new(10);
    private readonly Stack<Matrix4x4> _stack = new(10);

    public required TransformTreeNode[] RootToFrom { get; init; }
    public required TransformTreeNode[] ToToRoot { get; init; }

    public Action<long> CallBack => _cache.SetDirty;

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Matrix4x4 MultiMatrixLine(Span<TransformTreeNode> nodes,long timeStamp)
    {
        _queue.Clear();
        _stack.Clear();

        foreach (var node in nodes)
            _queue.Enqueue(node.GetAffineRef(timeStamp));
        
        while (true)
        {
            var count = _queue.Count;
            if (count == 1) break;

            while (count > 1)
            {
                _stack.Push(_queue.Dequeue() * _queue.Dequeue());
                count -= 2;
            }

            if (count == 1)
            {
                var p = _queue.Dequeue();

                while (_stack.Count > 0)
                    _queue.Enqueue(_stack.Pop());
                    
                _queue.Enqueue(p);
            }
            else
            {
                while (_stack.Count > 0)
                    _queue.Enqueue(_stack.Pop());
            }
        }

        return _queue.Dequeue();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Matrix4x4 MultiMatrixLine(Span<TransformTreeNode> nodes, Matrix4x4 last,long timeStamp)
    {
        _queue.Clear();
        _stack.Clear();
        foreach (var node in nodes)
            _queue.Enqueue(node.GetAffineRef(timeStamp));
        _queue.Enqueue(last);
        while (true)
        {
            var count = _queue.Count;
            if (count == 1)
                break;
            while (count > 1)
            {
                _stack.Push(_queue.Dequeue() * _queue.Dequeue());
                count -= 2;
            }

            if (count == 1)
            {
                var p = _queue.Dequeue();

                while (_stack.Count > 0)
                    _queue.Enqueue(_stack.Pop());
                _queue.Enqueue(p);
            }
            else
            {
                while (_stack.Count > 0)
                    _queue.Enqueue(_stack.Pop());
            }
        }

        return _queue.Dequeue();
    }


    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
    private void TransformUpdate(TFCacheListNode node,long timeStamp)
    {
        if (RootToFrom.Length != 0)
        {
            var mat = RootToFrom.Length switch
            {
                1 => RootToFrom[0].GetAffineInvertRef(timeStamp),
                2 => RootToFrom[0].GetAffineInvertRef(timeStamp) * RootToFrom[1].GetAffineInvertRef(timeStamp),
                3 => RootToFrom[0].GetAffineInvertRef(timeStamp) * RootToFrom[1].GetAffineInvertRef(timeStamp) *
                     RootToFrom[2].GetAffineInvertRef(timeStamp),
                4 => RootToFrom[0].GetAffineInvertRef(timeStamp) * RootToFrom[1].GetAffineInvertRef(timeStamp) *
                     RootToFrom[2].GetAffineInvertRef(timeStamp) * RootToFrom[3].GetAffineInvertRef(timeStamp),
                _ => MultiMatrixLine(RootToFrom,timeStamp)
            };

            node.Transfrom = ToToRoot.Length switch
            {
                0 => mat,
                1 => mat * ToToRoot[0].GetAffineRef(timeStamp),
                2 => mat * ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp),
                3 => mat * ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp) * ToToRoot[2].GetAffineRef(timeStamp),
                4 => mat * ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp) * ToToRoot[2].GetAffineRef(timeStamp) *
                     ToToRoot[3].GetAffineRef(timeStamp),
                _ => MultiMatrixLine(ToToRoot, mat, timeStamp)
            };
        }
        else
        {
            node.Transfrom = ToToRoot.Length switch
            {
                0 => Matrix4x4.Identity,
                1 => ToToRoot[0].GetAffineRef(timeStamp),
                2 => ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp),
                3 => ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp) * ToToRoot[2].GetAffineRef(timeStamp),
                4 => ToToRoot[0].GetAffineRef(timeStamp) * ToToRoot[1].GetAffineRef(timeStamp) * ToToRoot[2].GetAffineRef(timeStamp) *
                     ToToRoot[3].GetAffineRef(timeStamp),
                _ => MultiMatrixLine(ToToRoot,timeStamp)
            };
        }
    }
    Matrix4x4 _default = Matrix4x4.Identity;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Matrix4x4 GetTransform(long time)
    {
        if (!_cache.Find(time, out var node))
            return ref _default;
        if(node!.Dirty)
        {
            TransformUpdate(node,time);
            node!.Dirty = false;
        }
        return ref node.Transfrom;
    }
}