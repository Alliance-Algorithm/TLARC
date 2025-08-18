using CostMap.Infrastructure.Data;
using Kernel.DataInterfaces.Navigation;
using Kernel.Utils;

namespace CostMap.Infrastructure.Algorithm;

public static class InflationLayerBuilder
{
    static sbyte[] _distance_cache = [];
    static int _inflationDistance = 0;
    static int _i_s_2 = 0;
    public static void SetPara(int inflationDistance)
    {
        var max = _inflationDistance / 2;
        if (inflationDistance == 0)
        {
            _inflationDistance = 0;
            return;
        }
        _inflationDistance = inflationDistance - inflationDistance & 0x1 + 1;
        _i_s_2 = _inflationDistance / 2;
        _distance_cache = new sbyte[_inflationDistance * _inflationDistance];
        for (int i = 0; i < _inflationDistance; i++)
            for (int j = 0; j < _inflationDistance; j++)
            {
                var ex = i - _inflationDistance / 2;
                var ey = j - _inflationDistance / 2;
                _distance_cache[i + j * _inflationDistance] = (sbyte)(100 * Math.Clamp((max - Math.Sqrt(ex * ex + ey * ey)) / max, 0, 1));
            }
    }

    public static Grid2DMap Build(IGridMap2D map2D, int sizeX, int sizeY)
    {
        Grid2DMap grid2D = Grid2DMap.New_IGridMap2DData(map2D.Data);

        BlockParallel.For(sizeX, sizeY, _inflationDistance, _inflationDistance,
            (x, y) =>
            {
                if (!map2D.IsMoveAble(x, y))
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
                    }
            });
        return grid2D;
    }

}