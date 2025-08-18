using CostMap.Infrastructure.Data;
using Kernel.DataInterfaces.Navigation;
using Kernel.Utils;

namespace CostMap.Infrastructure.Algorithm;

public static class InflationLayerBuilder
{
    static sbyte[] _distance_cache = [];
    static int _inflationDistance;
    static int _i_s_2;
    public static void SetPara(int inflationDistance)
    {
        if (inflationDistance == 0)
        {
            _inflationDistance = 0;
            return;
        }
        _inflationDistance = inflationDistance * 2 + +1;
        float max = _inflationDistance / 2.0f;
        _i_s_2 = _inflationDistance / 2;
        _distance_cache = new sbyte[_inflationDistance * _inflationDistance];
        for (int i = 0; i < _inflationDistance; i++)
            for (int j = 0; j < _inflationDistance; j++)
            {
                var ex = i - _i_s_2;
                var ey = j - _i_s_2;
                _distance_cache[i + j * _inflationDistance] = (sbyte)(100 * Math.Clamp((max - Math.Sqrt(ex * ex + ey * ey)) / max, 0, 1));
            }
    }

    public static Grid2DMap Build(IGridMap2D map2D)
    {
        Grid2DMap grid2D = Grid2DMap.New_IGridMap2DData(map2D.Data);
        if (_inflationDistance == 0)
            return grid2D;
        int sizeX = (int)map2D.Data.Width;
        int sizeY = (int)map2D.Data.Width;
        BlockParallel.For(sizeX, sizeY, _inflationDistance, _inflationDistance,
            (x, y) =>
            {
                if (map2D.IsMoveAble(x, y))
                    return;
                grid2D.DataChangeable.Data[x + y * sizeX] = 100;
                for (int i = -_i_s_2; i < _i_s_2; i++)
                    for (int j = -_i_s_2; j < _i_s_2; j++)
                    {
                        var ox = x + i;
                        var oy = y + j;
                        if (ox < 0 || oy < 0 || ox >= sizeX || oy >= sizeX || (ox == oy && ox == 0)) continue;
                        var index = ox + oy * sizeX;
                        grid2D.DataChangeable.Data[index] = Math.Max(grid2D.DataChangeable.Data[index], _distance_cache[i + _i_s_2 + (j + _i_s_2) * _inflationDistance]);
                        ;
                    }
            });
        return grid2D;
    }

}