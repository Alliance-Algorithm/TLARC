using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;
using CostMap.Infrastructure.Data;
using g4;
using Kernel.Contract;
using Kernel.Contract.Navigation;
using Kernel.Utils;
using SkiaSharp;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CostMap.Infrastructure.Algorithm;

public static class GridMapInner
{
    internal enum ThresholdType
    {
        Greater,
        Less,
        GreaterEqual,
        LessEqual,
        Equal
    }

    private struct Header()
    {
        public string Identifier = "";
        public double RotationRad = 0;
        public float Resolution = 0.02f;
        public HeMatrix3x2 RotationMatrix;
        public HeVector2 Origin;

        public struct HeMatrix3x2
        {
            public float M11;
            public float M12;
            public float M21;
            public float M22;
            public float M31;
            public float M32;
        }

        public struct HeVector2
        {
            public float X;
            public float Y;
        }
    }

    /// <summary>
    /// 保存地图
    /// </summary>
    /// <param name="map2d">要保存的地图</param>
    /// <param name="path">地图文件夹</param>
    public static void SaveHighMap(OccupancyHighGrid2DMap map2d, string path)
    {
        path = path.TrimEnd('/').TrimEnd('\\');
        path = DirectoryParser.ExpandTildePath(path);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        using var bitmap = new SKBitmap((int)map2d.Data.GridMapData.Width, (int)map2d.Data.GridMapData.Height,
            SKColorType.Gray8,
            SKAlphaType.Opaque);
        unsafe
        {
            fixed (sbyte* ptr = map2d.Data.GridMapData.Data)
            {
                bitmap.SetPixels((nint)ptr);
            }

            using var wStream = new SKFileWStream(path + "/map.png");
            bitmap.Encode(wStream, SKEncodedImageFormat.Png, 100);
        }

        using var bitmap2 = new SKBitmap((int)map2d.Data.GridMapData.Width, (int)map2d.Data.GridMapData.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Unpremul);
        unsafe
        {
            fixed (float* ptr = map2d.High)
            {
                bitmap2.SetPixels((nint)ptr);
            }

            using var wStream = new SKFileWStream(path + "/high.png");
            bitmap2.Encode(wStream, SKEncodedImageFormat.Png, 100);
        }

        var header = new Header
        {
            Identifier = map2d.Data.GridMapData.Header.Identifier,
            RotationRad = map2d.Data.GridMapData.RotationRad,
            Origin = new Header.HeVector2
            { X = map2d.Data.GridMapData.Origin.X, Y = map2d.Data.GridMapData.Origin.Y },
            RotationMatrix = new Header.HeMatrix3x2
            {
                M11 = map2d.Data.GridMapData.RotationMatrix.M11,
                M12 = map2d.Data.GridMapData.RotationMatrix.M12,
                M21 = map2d.Data.GridMapData.RotationMatrix.M21,
                M22 = map2d.Data.GridMapData.RotationMatrix.M22,
                M31 = map2d.Data.GridMapData.RotationMatrix.M31,
                M32 = map2d.Data.GridMapData.RotationMatrix.M32
            },
            Resolution = map2d.Data.GridMapData.Resolution
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(header);
        File.WriteAllText(path + "/header.yaml", yaml);
    }

    /// <summary>
    /// 保存地图
    /// </summary>
    /// <param name="map2d">要保存的地图</param>
    /// <param name="path">地图文件夹</param>
    public static void SaveMap(GridMap2DData map2d, string path)
    {
        path = path.TrimEnd('/').TrimEnd('\\');
        path = DirectoryParser.ExpandTildePath(path);
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        using var bitmap = new SKBitmap((int)map2d.Width, (int)map2d.Height, SKColorType.Gray8, SKAlphaType.Opaque);
        unsafe
        {
            fixed (sbyte* ptr = map2d.Data)
            {
                bitmap.SetPixels((nint)ptr);
            }

            using var wStream = new SKFileWStream(path + "/map.png");
            bitmap.Encode(wStream, SKEncodedImageFormat.Png, 100);
        }

        var header = new Header
        {
            Identifier = map2d.Header.Identifier,
            RotationRad = map2d.RotationRad,
            Origin = new Header.HeVector2 { X = map2d.Origin.X, Y = map2d.Origin.Y },
            RotationMatrix = new Header.HeMatrix3x2
            {
                M11 = map2d.RotationMatrix.M11,
                M12 = map2d.RotationMatrix.M12,
                M21 = map2d.RotationMatrix.M21,
                M22 = map2d.RotationMatrix.M22,
                M31 = map2d.RotationMatrix.M31,
                M32 = map2d.RotationMatrix.M32
            },
            Resolution = map2d.Resolution
        };
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(header);
        File.WriteAllText(path + "/header.yaml", yaml);
    }

    /// <summary>
    /// 读取地图
    /// </summary>
    /// <param name="path">地图文件夹</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">地图信息</exception>
    public static OccupancyHighGrid2DMap LoadHighMap(string path)
    {
        path = path.TrimEnd('/').TrimEnd('\\');
        path = DirectoryParser.ExpandTildePath(path);
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        using var bitmap = SKBitmap.Decode(path + "/map.png");
        if (bitmap is null)
            throw new FileNotFoundException(path);
        var serializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = File.ReadAllText(path + "/header.yaml");
        var header = serializer.Deserialize<Header>(yaml);
        GridMap2DData map2d = new()
        {
            Header  = new() { Identifier = header.Identifier },
            Width   = (uint)bitmap.Width,
            Height  = (uint)bitmap.Height,
            Data    = new sbyte[bitmap.ByteCount]
        };
        Buffer.BlockCopy(bitmap.Bytes, 0, map2d.Data, 0, bitmap.ByteCount);
        map2d.RotationMatrix = new Matrix3x2
        {
            M11 = header.RotationMatrix.M11,
            M12 = header.RotationMatrix.M12,
            M21 = header.RotationMatrix.M21,
            M22 = header.RotationMatrix.M22,
            M31 = header.RotationMatrix.M31,
            M32 = header.RotationMatrix.M32
        };
        map2d.Origin = new Vector2
        {
            X = header.Origin.X,
            Y = header.Origin.Y
        };
        map2d.RotationRad = header.RotationRad;
        map2d.Resolution = header.Resolution;
        map2d.Resolution = header.Resolution;
        var map2dHigh = OccupancyHighGrid2DMap.Build_GridMap2DData(map2d);
        using var bitmap2 = SKBitmap.Decode(path + "/high.png",
            new SKImageInfo(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
        Buffer.BlockCopy(bitmap2.Bytes, 0, map2dHigh.High, 0, bitmap2.ByteCount);

        return map2dHigh;
    }

    /// <summary>
    /// 读取地图
    /// </summary>
    /// <param name="path">地图文件夹</param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException">地图信息</exception>
    public static GridMap2DData LoadMap(string path)
    {
        path = path.TrimEnd('/').TrimEnd('\\');
        path = DirectoryParser.ExpandTildePath(path);
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        using var bitmap = SKBitmap.Decode(path + "/map.png") ?? throw new FileNotFoundException(path);
        var serializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = File.ReadAllText(path + "/header.yaml");
        var header = serializer.Deserialize<Header>(yaml);
        GridMap2DData map2d = new()
        {
            Header = new() { Identifier = header.Identifier },
            Width = (uint)bitmap.Width,
            Height = (uint)bitmap.Height,
            Data = new sbyte[bitmap.ByteCount]
        };
        Buffer.BlockCopy(bitmap.Bytes, 0, map2d.Data, 0, bitmap.ByteCount);

        map2d.RotationMatrix = new Matrix3x2
        {
            M11 = header.RotationMatrix.M11,
            M12 = header.RotationMatrix.M12,
            M21 = header.RotationMatrix.M21,
            M22 = header.RotationMatrix.M22,
            M31 = header.RotationMatrix.M31,
            M32 = header.RotationMatrix.M32
        };
        map2d.Origin = new Vector2
        {
            X = header.Origin.X,
            Y = header.Origin.Y
        };
        map2d.RotationRad = header.RotationRad;
        map2d.Resolution = header.Resolution;
        map2d.Resolution = header.Resolution;
        return map2d;
    }

    /// <summary>
    /// 返回目标点是否有障碍物
    /// </summary>
    /// <param name="target">目标点</param>
    /// <param name="data">输入地图数据</param>
    /// <param name="threshold">阈值,与之作比较</param>
    /// <param name="type">比较方法
    ///<para>example: type == LessEqual then if(data &le; threshold) return true;</para>
    /// </param>
    /// <returns>如果有障碍物：true</returns>
    internal static bool CheckMoveable(Vector2 target,
                                       GridMap2DData data,
                                       sbyte threshold,
                                       ThresholdType type)
    {
        var vecInWorld = target - data.Origin;
        var vecInMap = (data.RotationMatrix * Matrix3x2.CreateTranslation(vecInWorld / data.Resolution)).Translation;
        if (vecInMap.X < 0 || vecInMap.X >= data.Width || vecInMap.Y < 0 || vecInMap.Y >= data.Height)
            return true;

        int xIndexInMap = (int)vecInMap.X,
            yIndexInMap = (int)vecInMap.Y;
        return type switch
        {
            ThresholdType.Equal => data.Data[xIndexInMap + yIndexInMap * data.Width] == threshold,
            ThresholdType.GreaterEqual => data.Data[xIndexInMap + yIndexInMap * data.Width] >= threshold,
            ThresholdType.LessEqual => data.Data[xIndexInMap + yIndexInMap * data.Width] <= threshold,
            ThresholdType.Less => data.Data[xIndexInMap + yIndexInMap * data.Width] < threshold,
            ThresholdType.Greater => data.Data[xIndexInMap + yIndexInMap * data.Width] > threshold,
            _ => true
        };
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">终点位置</param>
    /// <param name="data">输入地图数据</param>
    /// <param name="threshold">阈值,与之作比较</param>
    /// <param name="type">比较方法
    ///<para>example: type == LessEqual then if(data &le; threshold) return true;</para>
    /// </param>
    /// <returns>如果有障碍物：true</returns>
    internal static bool CheckMoveable(Vector2 from,
                                       Vector2 to,
                                       GridMap2DData data,
                                       sbyte threshold,
                                       ThresholdType type)
    {
        var fromVecInWorld = from - data.Origin;
        var fromVecInMap = (data.RotationMatrix *
                            Matrix3x2.CreateTranslation(fromVecInWorld / data.Resolution))
            .Translation;
        if (fromVecInMap.X < 0 || fromVecInMap.X >= data.Width || fromVecInMap.Y < 0 || fromVecInMap.Y >= data.Height)
            return false;
        var toVecInWorld = to - data.Origin;
        var toVecInMap = (data.RotationMatrix *
                          Matrix3x2.CreateTranslation(toVecInWorld / data.Resolution))
            .Translation;
        if (toVecInMap.X < 0 || toVecInMap.X >= data.Width || toVecInMap.Y < 0 || toVecInMap.Y >= data.Height)
            return false;

        Vector2i
            fromIndexInMap = new((int)fromVecInMap.X, (int)fromVecInMap.Y),
            toIndexInMap = new((int)toVecInMap.X, (int)toVecInMap.Y);


        var indexes = Geometry.ThickLine(fromIndexInMap, toIndexInMap);

        return indexes.All(predicate: indexInMap => type switch
        {
            ThresholdType.Equal => data.Data[indexInMap.x + indexInMap.y * data.Width] == threshold,
            ThresholdType.GreaterEqual => data.Data[indexInMap.x + indexInMap.y * data.Width] >= threshold,
            ThresholdType.LessEqual => data.Data[indexInMap.x + indexInMap.y * data.Width] <= threshold,
            ThresholdType.Less => data.Data[indexInMap.x + indexInMap.y * data.Width] < threshold,
            ThresholdType.Greater => data.Data[indexInMap.x + indexInMap.y * data.Width] > threshold,
            _ => false
        });
    }

    public static void SelectPointsInHighMap(ref Vector3[] points,
                                             float carHigh,
                                             float carStep,
                                             OccupancyHighGrid2DMap map)
    {
        var check = (Vector3 p) =>
        {
            var end =
                new Vector2i(
                    (int)((p.X - map.Data.GridMapData.Origin.X) / map.Data.GridMapData.Resolution),
                    (int)((p.Y - map.Data.GridMapData.Origin.Y) / map.Data.GridMapData.Resolution));
            if (end.x < 0 || end.x >= map.Data.GridMapData.Width || end.y < 0 || end.y >= map.Data.GridMapData.Height)
                return false;
            var index = end.x + end.y * map.Data.GridMapData.Width;
            return map.High[index] + carStep < p.Z && p.Z < map.High[index] + carHigh;
        };
        points = [..
            from p in points
            where check(p)
            select p];
    }
}