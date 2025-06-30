using System.Numerics;
using System.Runtime.CompilerServices;

namespace Kernel.Core.TransformTree;

internal class TransformCache
{
    private bool _changed = true;
    private Matrix4x4 _cache;
    private readonly Queue<Matrix4x4> _queue = new(10);
    private readonly Stack<Matrix4x4> _stack = new(10);

    public required TransformTreeNode[] RootToFrom { get; init; }
    public required TransformTreeNode[] ToToRoot { get; init; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ChangedCallback() => Volatile.Write(ref _changed, true);

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Matrix4x4 MultiMatrixLine(Span<TransformTreeNode> nodes)
    {
        _queue.Clear();
        _stack.Clear();
        foreach (var node in nodes)
            _queue.Enqueue(node.GetAffineRef());
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

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private Matrix4x4 MultiMatrixLine(Span<TransformTreeNode> nodes, Matrix4x4 last)
    {
        _queue.Clear();
        _stack.Clear();
        foreach (var node in nodes)
            _queue.Enqueue(node.GetAffineRef());
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
    private void TransformUpdate()
    {
        if (RootToFrom.Length != 0)
        {
            var mat = RootToFrom.Length switch
            {
                1 => RootToFrom[0].GetAffineInvertRef(),
                2 => RootToFrom[0].GetAffineInvertRef() * RootToFrom[1].GetAffineInvertRef(),
                3 => RootToFrom[0].GetAffineInvertRef() * RootToFrom[1].GetAffineInvertRef() *
                     RootToFrom[2].GetAffineInvertRef(),
                4 => RootToFrom[0].GetAffineInvertRef() * RootToFrom[1].GetAffineInvertRef() *
                     RootToFrom[2].GetAffineInvertRef() * RootToFrom[3].GetAffineInvertRef(),
                _ => MultiMatrixLine(RootToFrom)
            };

            _cache = ToToRoot.Length switch
            {
                0 => mat,
                1 => mat * ToToRoot[0].GetAffineRef(),
                2 => mat * ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef(),
                3 => mat * ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef() * ToToRoot[2].GetAffineRef(),
                4 => mat * ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef() * ToToRoot[2].GetAffineRef() *
                     ToToRoot[3].GetAffineRef(),
                _ => MultiMatrixLine(ToToRoot, mat)
            };
        }
        else
        {
            _cache = ToToRoot.Length switch
            {
                0 => Matrix4x4.Identity,
                1 => ToToRoot[0].GetAffineRef(),
                2 => ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef(),
                3 => ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef() * ToToRoot[2].GetAffineRef(),
                4 => ToToRoot[0].GetAffineRef() * ToToRoot[1].GetAffineRef() * ToToRoot[2].GetAffineRef() *
                     ToToRoot[3].GetAffineRef(),
                _ => MultiMatrixLine(ToToRoot)
            };
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Matrix4x4 GetTransform()
    {
        if (!_changed)
            return ref _cache;
        _changed = false;
        TransformUpdate();
        return ref _cache;
    }
}