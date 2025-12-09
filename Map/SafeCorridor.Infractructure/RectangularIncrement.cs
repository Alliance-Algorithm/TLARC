using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using Kernel.Contract.Navigation;
using Kernel.Contract.Visualization;

namespace Tlarc.Map.SafeCorridor;

public static class RectangleIncrement
{
    const float IncrementMax    = 1.5f; 
    const float RatioMax        = 2f; 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static AABB2D CalculateAABB<TMap>(Vector2 point, TMap map, float resolution) where TMap : IMap2D
    {
        byte flag = 0b1111;
        byte dir  = 0b0001;
        
        float maxX,minX,maxY,minY;
        float tmpX,tmpY;

        maxX = minX = point.X;
        maxY = minY = point.Y;

        while (true)
        {
            if((flag & dir) == 0b0000)
            {
                dir = dir switch
                {
                    0b0001 => 0b0010,
                    0b0010 => 0b0100,
                    0b0100 => 0b1000,
                    0b1000 => 0b0001,
                    _      => 0b0001
                };
                continue;
            }

            switch(dir)
            {
                case 0b0001:
                dir  = 0b0010;
                tmpX = maxX + resolution;
                if(map.IsMoveAble(new(tmpX, maxY),new(tmpX,minY)))
                    flag &= 0b1110;
                else
                    maxX = tmpX;
                break;
                case 0b0010:
                dir  = 0b0100;
                tmpY = maxY + resolution;
                if(map.IsMoveAble(new(minX,tmpY),new(maxX, tmpY)))
                    flag &= 0b1101;
                else
                    maxY = tmpY;
                break;
                case 0b0100:
                dir  = 0b1000;
                tmpX = minX - resolution;
                if(map.IsMoveAble(new(tmpX, maxY),new(tmpX,minY)))
                    flag &= 0b1011;
                else
                    minX = tmpX;
                break;
                case 0b1000:
                dir = 0b0001;
                tmpY = minY - resolution;
                if(map.IsMoveAble(new(maxX, tmpY),new(minX,tmpY)))
                    flag &= 0b0111;
                else
                    minY = tmpY;
                break;
                default:
                    dir = 0b0001;
                break;
            }
            var dx = maxX - minX;
            var dy = maxY - minY;
            var r = dx / dy;
            if(dx > IncrementMax)   flag &= 0b1010;
            if(dy > IncrementMax)   flag &= 0b0101;
            // if(r > RatioMax)        flag &= 0b1010;
            // if(r > 1 / RatioMax)    flag &= 0b0101;

            if(flag == 0b0000) break;
        }

        return new(){MaxX = maxX, MinX = minX, MaxY = maxY, MinY = minY};
    }

    public  static SafeCorridor2DData<AABB2D> AABB<TMap>(Path2D path ,TMap map, float resolution) where TMap : IMap2D
    {
        var points = path.Points;
        List<AABB2D> bounds = [];
        static bool Inside(AABB2D bound, Vector2 point) => 
                point.X < bound.MaxX && point.X > bound.MinX 
            &&  point.Y < bound.MaxY && point.Y > bound.MinY;
        
        for(int i = 0, l = points.Length - 1; i <= l; ++i)
        {
            var bound = CalculateAABB(points[i],map,resolution);
            do ++i; while(i < l && Inside(bound, points[i]));
            bounds.Add(bound);
        } 

        return new(){Header = path.Header, Corridors = [.. bounds], Length = bounds.Count};
    }
}