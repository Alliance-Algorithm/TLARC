namespace Kernel.Utils;

public static class BlockParallel
{
    struct Rect
    {
        public int OriginX;
        public int OriginY;
        public int SizeX;
        public int SizeY;
    }

    static int Count(int size, int kernelSize) => kernelSize > 0 ? (size / kernelSize) switch
    {
        < 3 => 1,
        < 5 => 2,
        _ => 3
    } : 3;

    static Rect[][] SplitToRect(
        int sizeX, int sizeY,
        int kernelSizeX, int kernelSizeY)
    {

        var xCount = Count(sizeX, kernelSizeX);
        var yCount = Count(sizeY, kernelSizeY);

        var xStep = sizeX / xCount + 1;
        var yStep = sizeY / yCount + 1;

        var xDomainSize = xStep - kernelSizeX;
        var yDomainSize = yStep - kernelSizeY;
        var xOuterSize = sizeX - (xStep * (xCount - 1));
        var yOuterSize = sizeY - (yStep * (yCount - 1));

        Rect[][] rs =
            [   new Rect[xCount * yCount],
                new Rect[(xCount - 1) * yCount],
                new Rect[(yCount - 1) * xCount],
                new Rect[(xCount - 1) * (yCount - 1)]];

        for (int i = 0; i < xCount; i++)
        {
            for (int j = 0; j < yCount; j++)
            {
                var index = i + j * xCount;
                rs[0][index].OriginX = i * xStep;
                rs[0][index].OriginY = j * yStep;
                rs[0][index].SizeX = i != (xCount - 1) ? xDomainSize : xOuterSize;
                rs[0][index].SizeY = j != (yCount - 1) ? yDomainSize : yOuterSize;
                if (i != xCount - 1)
                {
                    index = i + j * (xCount - 1);
                    rs[1][index].OriginX = i * xStep + xDomainSize;
                    rs[1][index].OriginY = j * yStep;
                    rs[1][index].SizeX = kernelSizeX;
                    rs[1][index].SizeY = j != (yCount - 1) ? yDomainSize : yOuterSize;
                }
                if (j != yCount - 1)
                {
                    index = i + j * xCount;
                    rs[2][index].OriginX = i * xStep;
                    rs[2][index].OriginY = j * yStep + yDomainSize;
                    rs[2][index].SizeX = i != (xCount - 1) ? xDomainSize : xOuterSize;
                    rs[2][index].SizeY = kernelSizeY;
                }
                if (i != xCount - 1 && j != yCount - 1)
                {
                    index = i + j * (xCount - 1);
                    rs[3][index].OriginX = i * xStep + xDomainSize;
                    rs[3][index].OriginY = j * yStep + yDomainSize;
                    rs[3][index].SizeX = kernelSizeX;
                    rs[3][index].SizeY = kernelSizeY;
                }
            }
        }
        return rs;
    }

    static void EnumIndexInRect(Rect rect, Action<int, int> action)
    {
        for (int i = rect.OriginX, k = rect.SizeX + rect.OriginX; i < k; i++)
            for (int j = rect.OriginY, l = rect.SizeY + rect.OriginY; j < l; j++)
                action(i, j);
    }
    /// <summary>
    /// Foreach 2d Mat in Parallel run action(index), Inner with System.Tasks.Parallel
    /// </summary>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    /// <param name="kernelSizeX"></param>
    /// <param name="KernelSizeY"></param>
    /// <param name="inIndexAction">Action<x,y></param>
    public static void For(
        int sizeX, int sizeY,
        int kernelSizeX, int kernelSizeY,
        Action<int, int> inIndexAction
    )
    {
        var recs = SplitToRect(sizeX, sizeY, kernelSizeX, kernelSizeY);
        Parallel.ForEach(recs[0], x => EnumIndexInRect(x, inIndexAction));
        Parallel.ForEach(recs[1], x => EnumIndexInRect(x, inIndexAction));
        Parallel.ForEach(recs[2], x => EnumIndexInRect(x, inIndexAction));
        Parallel.ForEach(recs[3], x => EnumIndexInRect(x, inIndexAction));
    }

    public static string Test()
    {
        int sizeX = 13;
        int sizeY = 17;
        int kernelSizeX = 3;
        int kernelSizeY = 3;
        string result = string.Empty;
        int[] m = new int[sizeX * sizeY];
        var recs = SplitToRect(sizeX, sizeY, kernelSizeX, kernelSizeY);
        Parallel.ForEach(recs[0], x => EnumIndexInRect(x, (i, j) => m[i + j * sizeX] += 1));
        Parallel.ForEach(recs[1], x => EnumIndexInRect(x, (i, j) => m[i + j * sizeX] += 2));
        Parallel.ForEach(recs[2], x => EnumIndexInRect(x, (i, j) => m[i + j * sizeX] += 3));
        Parallel.ForEach(recs[3], x => EnumIndexInRect(x, (i, j) => m[i + j * sizeX] += 4));

        result += $"rect count : {recs[0].Length}\n";

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                var index = i + j * sizeX;
                result += "\t";
                result += m[index];
                result += ",";
            }
            result += '\n';
        }
        return result;
    }
}