using System.Numerics;
using System.Runtime.InteropServices;

namespace Kernel.Core.TransformTree;

public class Tf
{
    internal static readonly Dictionary<string, TransformTreeNode> Nodes = [];

    internal static Dictionary<(string from, string to), TransformCache> _caches = [];

    private static bool CalculateTransferNode(string       identifierTo,
                                              string       identifierFrom,
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

    /// <summary>
    /// 把坐标由From坐标系转移到To坐标系的表达
    /// </summary>
    /// <param name="identifierTo"></param>
    /// <param name="identifierFrom"></param>
    /// <param name="position">要转移的位置</param>
    /// <returns></returns>
    public static Vector3 Cast(string identifierFrom, string identifierTo, Vector3 position)
    {
        if (!Tf.CalculateTransferNode(identifierTo, identifierFrom, out var rootTo, out var rootFrom))
            throw new TlarcTfError.NoRoute(identifierFrom, identifierTo);

        if (!_caches.TryGetValue((identifierFrom, identifierTo), out var cache))
        {
            cache = new TransformCache
            {
                RootToFrom = rootFrom,
                RootToTo = rootTo
            };
            var nodesToSubscribe = Nodes
                .Where(x => rootTo.Contains(x.Key) || rootFrom.Contains(x.Key))
                .ToList();
            if (nodesToSubscribe.Count > Environment.ProcessorCount * 10)
                nodesToSubscribe.AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .ForAll(x => x.Value.Changed += cache.ChangedCallback);
            else
                foreach (var node in nodesToSubscribe)
                    node.Value.Changed += cache.ChangedCallback;

            Nodes.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(x =>
                {
                    if (rootTo.Contains(x.Key) || rootFrom.Contains(x.Key))
                        x.Value.Changed += cache.ChangedCallback;
                });
            _caches[(identifierFrom, identifierTo)] = cache;
        }


        ref var transform = ref cache.GetTransform();
        var ret = (
            Matrix4x4.CreateTranslation(position) * transform).Translation;
        return ret;
    }


    public static void AddTfNode(string identifier, string parentId)
    {
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

        _caches = [];
        Nodes.Values.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll(x => x.ResetChanged());
    }

    public static void SetTfNode(string identifier, Vector3 translation, Quaternion rotation)
    {
        if (!Nodes.TryGetValue(identifier, out var node))
            throw new TlarcTfError.SetNoNodeException(identifier);
        node.SetTransform(translation, rotation);
    }
}