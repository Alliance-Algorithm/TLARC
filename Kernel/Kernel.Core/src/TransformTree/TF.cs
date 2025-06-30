using System.Collections.Frozen;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kernel.Core.SoFucingFastAlgorithms;
using Kernel.DataInterfaces;
using Kernel.DataInterfaces.Geometry;
using Kernel.DataInterfaces.Tf;

namespace Kernel.Core.TransformTree;

public static class Tf
{
    private static readonly Dictionary<string, TransformTreeNode> Nodes = [];
    internal static FrozenDictionary<string, TransformTreeNode> HybridNodes = Nodes.ToFrozenDictionary();

    private static TransformCache?[] _caches = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private static bool CalculateTransferNode(in  string   identifierTo,
                                              in  string   identifierFrom,
                                              out string[] rootToFrom,
                                              out string[] rootToTo)
    {
        rootToFrom = [];
        rootToTo = [];
        var i = 0;
        if (!Nodes.ContainsKey(identifierFrom))
            throw new TlarcTfError.FoundNoNodeException(identifierFrom);
        if (!Nodes.ContainsKey(identifierTo))
            throw new TlarcTfError.FoundNoNodeException(identifierTo);
        var parentIdsFrom = CollectionsMarshal.AsSpan(Nodes[identifierFrom].ParentIds);
        var parentIdsTo   = CollectionsMarshal.AsSpan(Nodes[identifierTo].ParentIds);
        int max1 = Nodes[identifierFrom].ParentIds.Count,
            max2 = Nodes[identifierTo].ParentIds.Count;

        while (i < max1 && i < max2 && parentIdsFrom[i] == parentIdsTo[i])
            i++;
        if (i == 0)
            return false;
        rootToFrom = [.. parentIdsFrom[i ..]];
        rootToTo = [.. parentIdsTo[i ..]];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    [SkipLocalsInit]
    private static int HashFunc(int i, int j) => i * TransformTreeNode.IdCount + j;


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vector3 CastImpl(in string identifierFrom, in string identifierTo, Vector3 position)
    {
        var     hashcode = Tf.HashFunc(HybridNodes[identifierFrom].Id, HybridNodes[identifierTo].Id);
        ref var cache    = ref _caches[hashcode];
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
                node.Value.Changed.AddHandler(cache.ChangedCallback);
        }

        ref var transform = ref cache.GetTransform();
        var     ret       = Vector3.Transform(position, transform);
        return ret;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static Vector3[] CastImpl(in string identifierFrom, in string identifierTo, Vector3[] positions)
    {
        var     hashcode = Tf.HashFunc(HybridNodes[identifierFrom].Id, HybridNodes[identifierTo].Id);
        ref var cache    = ref _caches[hashcode];
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
                node.Value.Changed.AddHandler(cache.ChangedCallback);
        }

        var transform = cache.GetTransform();
        var chunkSize = Math.Max(1, positions.Length / 2000);
        var ret       = new Vector3[positions.Length];

        Parallel.For(0, (positions.Length + chunkSize - 1) / chunkSize, i =>
        {
            var start = i * chunkSize;
            var end   = Math.Min(start + chunkSize, positions.Length);

            for (var j = start; j < end; j++)
                ret[j] = Vector3.Transform(positions[j], transform);
        });

        return ret;
    }

    /// <summary>
    /// 把坐标由From坐标系转移到To坐标系的表达
    /// </summary>
    /// <param name="identifierTo"></param>
    /// <param name="identifierFrom"></param>
    /// <param name="position">要转移的位置</param>
    /// <returns></returns>
    public static Vector3 Cast(in string identifierFrom, in string identifierTo, Vector3 position) =>
        identifierFrom == identifierTo ? position : Tf.CastImpl(identifierFrom, identifierTo, position);


    public static Vector3[] Cast(in string identifierFrom, in string identifierTo, Vector3[] position) =>
        identifierFrom == identifierTo ? position : Tf.CastImpl(identifierFrom, identifierTo, position);


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
    public static void SetTfNode(string identifier, Vector3 translation, Quaternion rotation)
    {
        if (!Nodes.TryGetValue(identifier, out var node))
            throw new TlarcTfError.SetNoNodeException(identifier);
        node.SetTransform(translation, rotation);
    }

    private class TransformStamped : ITransformStamped, IHeader, IPose
    {
        public IHeader Header => this;
        public string Identifier { get; set; } = "";

        public string ParentFrameId { get; set; } = "";


        public IPose Pose => this;
        public Quaternion Orientation { get; set; } = Quaternion.Identity;
        public Vector3 Position { get; set; } = Vector3.Zero;
    }

    private class TfCollection : ITfCollection
    {
        public ITransformStamped[] TransformStampeds { get; init; } = [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ITfCollection GetTree() => new TfCollection
    {
        TransformStampeds = Nodes.Values.Where(n => n.ParentIds.Count > 1).Select(n => Tf.GetNode(n.Identifier))
            .ToArray()
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ITransformStamped GetNode(string identifier)
    {
        if (!Nodes.TryGetValue(identifier, out var node))
            return new TransformStamped { Identifier = identifier };

        return new TransformStamped
        {
            Identifier = identifier,
            ParentFrameId = node.ParentIds.Count > 1 ? node.ParentIds[^2] : "",
            Orientation = node.Rotation,
            Position = node.Translate
        };
    }
}