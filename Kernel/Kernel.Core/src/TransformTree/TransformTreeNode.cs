using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.Core.SoFuckingFastAlgorithms;
using Kernel.Utils;

namespace Kernel.Core.TransformTree;

internal class TransformTreeNode(string identifier)
{
    internal static int IdCount = 0;
    internal readonly int Id = IdCount++;
    internal FastEvent Changed = new();
    internal string Identifier { get; } = identifier;

    [MethodImpl(MethodImplOptions.AggressiveOptimization |
                MethodImplOptions.AggressiveInlining)]
    internal void ResetChanged() => Changed = new FastEvent();

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void Update()
    {
        var cq = Quaternion.Conjugate(_rotation);
        _affine = Matrix4x4.CreateFromQuaternion(cq);
        _affine.Translation = Vector3.Transform(-_translate, cq);
        _affineInvert = Matrix4x4.CreateFromQuaternion(_rotation);
        _affineInvert.Translation = _translate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetTransform(Vector3 translate, Quaternion rotation)
    {
        if (translate == _translate && rotation == _rotation)
            return;

        _translate = translate;
        _rotation = rotation;

        Update();
        Changed.Raise();
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
    internal ref Matrix4x4 GetAffineRef() => ref _affine;

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref Matrix4x4 GetAffineInvertRef() => ref _affineInvert;

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
    internal Vector3 Translate => _translate;
    internal Quaternion Rotation => _rotation;

    private Matrix4x4 _affine = Matrix4x4.Identity;
    private Matrix4x4 _affineInvert = Matrix4x4.Identity;
    private Vector3 _translate = new();
    private Quaternion _rotation = Quaternion.Identity;

    private Matrix4x4 _tmpMatrix = Matrix4x4.Identity;
}