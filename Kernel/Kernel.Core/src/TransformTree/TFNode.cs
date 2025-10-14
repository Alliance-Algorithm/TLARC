using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.Core.SoFuckingFastAlgorithms;
using Kernel.Utils;

namespace Kernel.Core.TransformTree;

internal class TransformTreeNode(string identifier)
{
    private readonly TFNodeCacheList _cache = new();

    internal static     int             IdCount             = 0;
    internal readonly   int             Id                  = IdCount++;
    internal event      Action<long>    Changed             = new( static x => { });
    internal            string          Identifier { get; } = identifier;

    [MethodImpl(MethodImplOptions.AggressiveOptimization |
                MethodImplOptions.AggressiveInlining)]
    internal void ResetChanged() => Changed = static x => {  };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetTransform(Vector3 translate, Quaternion rotation, long timeStamp)
    {
        Changed(timeStamp);
        _cache.SetNode(translate,rotation,timeStamp);
    }

    internal void SetParent(TransformTreeNode parent)
    {
        Parent?.Children.Remove(this);
        Parent = parent;
        Parent?.Children.Add(this);
        ResetParentIds();
    }

    private void ResetParentIds()
    {
        if (Parent != null)
            ParentIds = [.. Parent.ParentIds, Identifier];

        foreach (var child in Children)
            child.ResetParentIds();
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Matrix4x4 GetAffineRef         (long timeStamp) => ref _cache.GetNode(timeStamp).Affine;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Matrix4x4 GetAffineInvertRef   (long timeStamp) => ref _cache.GetNode(timeStamp).AffineInvert;

    internal void AddCallBack(Action<long> callBack)
    {
        _cache.Foreah(x => callBack(x.Time));
        Changed += callBack;
    }

    public Vector3      Translation => _cache.GetNode(-1).Translation; 
    public Quaternion   Orientation => _cache.GetNode(-1).Orientation; 

    private TransformTreeNode? Parent
    {
        get;
        set;
    } = null;
    private HashSet<TransformTreeNode> Children
    {
        get;
        set;
    } = [];
    internal List<string> ParentIds
    {
        get;
        private set;
    } = [identifier];
}