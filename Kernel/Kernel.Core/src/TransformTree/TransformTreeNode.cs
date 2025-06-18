using System.Numerics;

namespace Kernel.Core.TransformTree;

internal class TransformTreeNode(string identifier)
{
    internal event Action Changed = delegate { };
    public string Identifier { get; } = identifier;

    internal void ResetChanged()
    {
        Changed = delegate { };
    }

    internal void SetTransform(Vector3 translate, Quaternion rotation)
    {
        _affine = Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translate);
        Changed();
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

    internal ReadOnlySpan<Matrix4x4> GetAffineRef()
    {
        return new ReadOnlySpan<Matrix4x4>(ref _affine);
    }

    private TransformTreeNode? Parent { get; set; } = null;
    private HashSet<TransformTreeNode> Children { get; set; } = [];

    internal List<string> ParentIds { get; private set; } = [identifier];

    private Matrix4x4 _affine = Matrix4x4.Identity;
}