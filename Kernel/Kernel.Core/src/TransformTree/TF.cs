using System.Buffers;
using System.Collections.Frozen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kernel.Contract;
using Kernel.Contract.Geometry;
using Kernel.Contract.Tf;

namespace Kernel.Core.TransformTree;

public static class Tf
{
    private static readonly Dictionary<string, TransformTreeNode> Nodes = [];
    internal static FrozenDictionary<string, TransformTreeNode> HybridNodes = Nodes.ToFrozenDictionary();

    private static TransformCache?[] _caches = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private static bool CalculateTransferNode(in string identifierTo,
                                              in string identifierFrom,
                                              out string[] rootToFrom,
                                              out string[] rootToTo)
    {
        rootToFrom = [];
        rootToTo = [];
        var i = 0;
        if (!Nodes.TryGetValue(identifierFrom, out TransformTreeNode? value1))
            throw new TlarcTfError.FoundNoNodeException(identifierFrom);
        if (!Nodes.TryGetValue(identifierTo, out TransformTreeNode? value2))
            throw new TlarcTfError.FoundNoNodeException(identifierTo);
        var parentIdsFrom = CollectionsMarshal.AsSpan(value1.ParentIds);
        var parentIdsTo = CollectionsMarshal.AsSpan(value2.ParentIds);
        int max1 = value1.ParentIds.Count,
            max2 = value2.ParentIds.Count;

        while (i < max1 && i < max2 && parentIdsFrom[i] == parentIdsTo[i])
            i++;
        if (i == 0)
            return false;
        rootToFrom = [.. parentIdsFrom[i..]];
        rootToTo = [.. parentIdsTo[i..]];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private static int HashFunc(int i, int j) => i * TransformTreeNode.IdCount + j;


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vector3 CastImpl(in string identifierFrom, in string identifierTo, Vector3 position, long timeStamp)
    {
        var hashcode = Tf.HashFunc(HybridNodes[identifierFrom].Id, HybridNodes[identifierTo].Id);
        ref var cache = ref _caches[hashcode];
        if (cache is null)
        {
            if (!Tf.CalculateTransferNode(identifierTo, identifierFrom, out var rootToFrom, out var rootTo))
                throw new TlarcTfError.NoRoute(identifierFrom, identifierTo);

            cache = new TransformCache
            {
                RootToFrom = rootToFrom.Select(id => Nodes[id]).Reverse().ToArray(),
                ToToRoot = rootTo.Select(id => Nodes[id]).ToArray()
            };

            foreach (var node in
                     Nodes.Where(x => rootTo.Contains(x.Key) || rootToFrom.Contains(x.Key)))
                node.Value.AddCallBack(cache.CallBack);
        }

        ref var transform = ref cache.GetTransform(timeStamp);
        var ret = Vector3.Transform(position, transform);
        return ret;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CastImpl(in string identifierFrom, in string identifierTo, Vector3[] positions, Vector3[] outPositions, long timeStamp)
    {
        var hashcode = Tf.HashFunc(HybridNodes[identifierFrom].Id, HybridNodes[identifierTo].Id);
        ref var cache = ref _caches[hashcode];
        if (cache is null)
        {
            if (!Tf.CalculateTransferNode(identifierTo, identifierFrom, out var rootToFrom, out var rootTo))
                throw new TlarcTfError.NoRoute(identifierFrom, identifierTo);

            cache = new TransformCache
            {
                RootToFrom = rootToFrom.Select(id => Nodes[id]).Reverse().ToArray(),
                ToToRoot = [.. rootTo.Select(id => Nodes[id])]
            };

            foreach (var node in
                     Nodes.Where(x => rootTo.Contains(x.Key) || rootToFrom.Contains(x.Key))){
                // Console.WriteLine("Fuck"); 
                node.Value.AddCallBack(cache.CallBack);
            }
        }

        var transform = cache.GetTransform(timeStamp);
        var chunkSize = Math.Max(1, positions.Length / 2000);

        Parallel.For(0, (positions.Length + chunkSize - 1) / chunkSize, i =>
        {
            var start = i * chunkSize;
            var end = Math.Min(start + chunkSize, positions.Length);

            for (var j = start; j < end; j++)
                outPositions[j] = Vector3.Transform(positions[j], transform);
        });

    }

    /// <summary>
    /// 把坐标由From坐标系转移到To坐标系的表达
    /// </summary>
    /// <param name="identifierTo"></param>
    /// <param name="identifierFrom"></param>
    /// <param name="position">要转移的位置</param>
    /// <returns></returns>
    public static Vector3 Cast(
                            in string   identifierFrom, 
                            in string   identifierTo, 
                               Vector3  position, 
                               long     timeStamp = -1) => 
                        identifierFrom == identifierTo ? 
                        position : 
                        CastImpl(identifierFrom, identifierTo, position, timeStamp);

    public unsafe static Vector3[] Cast(
                                    in string       identifierFrom, 
                                    in string       identifierTo, 
                                       Vector3[]    position, 
                                       Vector3[]    outPositions, 
                                       long         timeStamp = -1)
    {
        if (identifierFrom  == identifierTo && 
            position        != outPositions) 
        {
            fixed (void* p1 = position)
            fixed (void* p2 = outPositions)
            Buffer.MemoryCopy(
                p1, 
                p2,  
                position.Length * sizeof(Vector3),
                position.Length * sizeof(Vector3));
            
            return outPositions;
        }
        
        CastImpl(
            identifierFrom, 
            identifierTo, 
            position, 
            outPositions, 
            timeStamp);
        return outPositions;
    }

    public static void AddTfNode(in string identifier, in string parentId)
    {
        if (identifier == parentId)
            return;
        if (!Nodes.TryGetValue(identifier, out var node))
        {
            node = new TransformTreeNode(identifier);
            Nodes[identifier] = node;
        }

        Nodes[identifier] = node;

        if (!Nodes.TryGetValue(parentId, out var parent))
        {
            parent = new TransformTreeNode(parentId);
            Nodes[parentId] = parent;
        }

        Nodes[parentId] = parent;

        node.SetParent(parent);

        _caches = new TransformCache[TransformTreeNode.IdCount * TransformTreeNode.IdCount];
        Nodes.Values.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll(x => x.ResetChanged());
        HybridNodes = Nodes.ToFrozenDictionary();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTfNode(string identifier, Vector3 translation, Quaternion rotation, long timeStamp = -1)
    {
        if (!Nodes.TryGetValue(identifier, out var node))
            throw new TlarcTfError.SetNoNodeException(identifier);
        node.SetTransform(translation, rotation, timeStamp);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TfCollection GetTree() => new()
    {
        TransformStampeds = [.. Nodes.Values
                                .Where (n => n.ParentIds.Count > 1)
                                .Select(n => Tf.GetNode(n.Identifier))]
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TransformStamped GetNode(string identifier)
    {
        if (!Nodes.TryGetValue(identifier, out var node))
            return new TransformStamped { Header = new Header{Identifier = identifier} };

        return new TransformStamped
        {
            Header          = new Header{ Identifier = identifier},
            ParentFrameId   = node.ParentIds.Count > 1 ? node.ParentIds[^2] : "",
            Pose            = new Pose{                
                Orientation     = node.Orientation,
                Position        = node.Translation
            }
        };
    }
}