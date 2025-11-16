using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Kernel.Contract.Navigation;
using Microsoft.Extensions.ObjectPool;

namespace ALPlanner.Infrastructure.PathSearcher;

public class RRT2D
{   
    unsafe class Node()
    {
        public Vector2     point;
        public Node?       parent;
    }
    public RRT2D(int sizeX, int sizeY , float step, float mapResolution, float pEnd = 0.1f)
    {
        _pEnd  = pEnd;
        _sizeX = sizeX * mapResolution;
        _sizeY = sizeY * mapResolution;
        _sStep = step  * step;
        _step  = step;
        Nodes  = new Node[CountMax];
        for(int i = 0; i < CountMax; i++) Nodes[i] = new();
    }
    readonly float _pEnd;
    readonly float _sizeX;
    readonly float _sizeY;
    readonly float _sStep;
    readonly float _step;

    const int CountMax = 2000; 
    
    readonly Node[]         Nodes;
    readonly Stack<Vector2> Path  = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 RandomNode() => new(_sizeX * (Random.Shared.NextSingle() - 0.5f), _sizeY * (Random.Shared.NextSingle() - 0.5f));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 SetpTo(Vector2 from, Vector2 to) => Vector2.Normalize(to - from) * _step + from;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe Node InnerSearch<T>(Vector2 from, Vector2 to, T map) where T : IMap2D
    {
        static void SetNode(ref Node node, Vector2 point, Node? parent = null)
        {
            node.point  = point;
            node.parent = parent;
        }
        
        SetNode(ref Nodes[0],from);
        int count = 1;
        int step  = 0;
        do
        {   
            var parallel = Nodes.SkipLast(CountMax - count).AsParallel();
            var p = Random.Shared.NextSingle();

            Vector2 pRand;
            if(p < _pEnd)    pRand = to;
            else             pRand = RandomNode();
            
            var xNear =  parallel
                        .WithDegreeOfParallelism(Environment.ProcessorCount)
                        .MinBy(n => Vector2.DistanceSquared(n.point, pRand));
            var error = Vector2.DistanceSquared(xNear!.point, pRand);
            var pNext = SetpTo(xNear.point, pRand);
            
            if(map.IsMoveAble(xNear.point, pNext)) continue;

            SetNode(ref Nodes[count], pNext, xNear);
            ++count;
            
            if(Vector2.DistanceSquared(pNext, to) > _sStep) continue;
            SetNode(ref Nodes[count], to, Nodes[count - 1]);
            return Nodes[count];
        }while(count < CountMax && ++step < 2 * CountMax);

        return new(){parent = null,point = to};
    }

    public unsafe Vector2[] Search<T>(Vector2 from, Vector2 to, T map) where T : IMap2D
    {
        var node  = InnerSearch(from, to, map);
        if(node == null) return[];

        Path.Clear();
        while(true)
        {
            Path.Push(node.point);
            if(node.parent == null) return [.. Path];
            
            node = node.parent;
        }
    }
}