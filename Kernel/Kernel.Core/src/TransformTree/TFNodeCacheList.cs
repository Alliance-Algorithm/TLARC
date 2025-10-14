
using System.Numerics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.ObjectPool;
using Microsoft.VisualBasic;

namespace Kernel.Core.TransformTree;


internal unsafe class TFNodeCacheListNode
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UpdateNode(TFNodeCacheListNode node,Quaternion orientation, Vector3 translation)
    {
        node.Orientation   = orientation;
        node.Translation   = translation;
        
        var cq                          = Quaternion.Conjugate(node.Orientation);
        node.Affine                     = Matrix4x4 .CreateFromQuaternion(cq);
        node.Affine.Translation         = Vector3   .Transform(-node.Translation, cq);
        node.AffineInvert               = Matrix4x4 .CreateFromQuaternion(node.Orientation);
        node.AffineInvert.Translation   = node.Translation;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TFNodeCacheListNode Lerp(
                            TFNodeCacheListNode from,
                            TFNodeCacheListNode to, 
                            long                timeStamp)
    {
        var ament = Math.Clamp(timeStamp, from.Time, to.Time);
        var orien = Quaternion.Normalize(Quaternion.Lerp(from.Orientation,to.Orientation,ament));
        var trans = Vector3.Lerp(from.Translation,to.Translation,ament);
        
        var newNode = new TFNodeCacheListNode { Time = timeStamp };

        UpdateNode(newNode,orien,trans);

        return newNode;
    }

    
    public static TFNodeCacheListNode Default = new();
    public TFNodeCacheListNode? Next = null;
    public TFNodeCacheListNode? Prev = null;
    
    public long Time;
    

    public Matrix4x4    Affine          = Matrix4x4.Identity;
    public Matrix4x4    AffineInvert    = Matrix4x4.Identity;
    public Vector3      Translation     = new();
    public Quaternion   Orientation     = Quaternion.Identity;
}


internal struct TFNodeCacheListNodePolicy : IPooledObjectPolicy<TFNodeCacheListNode>
{
    public readonly TFNodeCacheListNode Create() => new();

    public readonly bool Return(TFNodeCacheListNode obj) => true;
}


internal unsafe class TFNodeCacheList
{
    private  TFNodeCacheListNode?                    _head       = null;
    private  TFNodeCacheListNode?                    _tail       = null;
    
    readonly DefaultObjectPool<TFNodeCacheListNode>  _objectPool = new(new TFNodeCacheListNodePolicy()); 

    public void Foreah(Action<TFNodeCacheListNode> func)
    {
        var node = _head;
        while(node != null)
        {
            func(node);
            node = node.Next;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TFNodeCacheListNode GetNode(long timeStamp)
    {
        if(_head == null)           return TFNodeCacheListNode.Default;
        if(timeStamp < 0)           return _head;
        if(_tail!.Time > timeStamp) return TFNodeCacheListNode.Default;
        if(_head!.Time < timeStamp) return _head;

        var node = _head;
        while(node != null && node.Time > timeStamp) node = node.Next;

        if(node == null)            return TFNodeCacheListNode.Default;
        if(node.Time == timeStamp)  return node;

        return TFNodeCacheListNode.Lerp(node, node.Prev!, timeStamp);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetNode(Vector3 translation, Quaternion orientation,long timeStamp)
    {
        bool changed(TFNodeCacheListNode node) =>   node.Translation == translation &&
                                                    node.Orientation == orientation;
        void update (TFNodeCacheListNode node) =>   TFNodeCacheListNode.UpdateNode(node,orientation,translation);
        
        if( _head == null)
        {
            _head = _objectPool.Get();
            _tail = _head;
            _head.Time = timeStamp;
            update(_head);
            return;
        }
        
        while(_tail!.Time < timeStamp)
        {
            var tail    = _tail;
            _tail       = _tail.Prev;
            _objectPool.Return(tail);
        }

        if(timeStamp <= 0 && changed(_head))
        {
            update(_head);   
            return;
        }

        if(_tail!.Time < timeStamp && changed(_tail))
        {
            update(_tail);
            return;
        }

        var node = _head;

        while(node!.Time > timeStamp) node = node.Next;
        
        if(node.Time == timeStamp && changed(node)) 
        {
            update(node);
            return;
        };

        var newNode = _objectPool.Get();
        newNode.Time = timeStamp;
        update(newNode);
        newNode.Next = node;
        newNode.Prev = node.Prev;
        node.Prev = newNode;
    }
}