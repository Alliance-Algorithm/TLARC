using System.Numerics;
using System.Runtime.InteropServices;
using Kernel.DataInterfaces.Sensor;
using Microsoft.Toolkit.HighPerformance;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Sensor;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Sensor
{
    private class PointCloud : IPointCloud
    {
        public string Identifier { get; set; } = "";
        public Vector3[] Points { get; set; } = [];
    }

    public static IPointCloud RmcsSlamSegmentationPart(ref PointCloud2.Priv map)
    {
        var data   = map.Data.AsSpan().Cast<byte, Vector4>();
        var points = new Vector3[data.Length];
        for (var i = 0; i < data.Length; i++)
            points[i] = new Vector3(data[i].X, data[i].Y, data[i].Z);

        PointCloud pointCloud = new()
        {
            Identifier = map.Header.FrameId.ToString(),
            Points = points
        };
        return pointCloud;
    }

    [StructLayout(LayoutKind.Explicit, Size = 48, Pack = 1)]
    private struct FastLioPointCloud
    {
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float z;
    }

    public static IPointCloud FastLioRegistered(ref PointCloud2.Priv map)
    {
        var data   = map.Data.AsSpan().Cast<byte, FastLioPointCloud>();
        var points = new Vector3[data.Length];
        for (var i = 0; i < data.Length; i++)
            points[i] = new Vector3(data[i].x, data[i].y, data[i].z);


        PointCloud pointCloud = new()
        {
            Identifier = map.Header.FrameId.ToString(),
            Points = points
        };
        return pointCloud;
    }

    public static void WriteIntoPointCloud(IPointCloud pointCloud, IRclNode node, ref PointCloud2.Priv map)
    {
        map.Fields = new PointField.PrivSequence(3);
        map.Fields.AsSpan()[0].Count = 1;
        map.Fields.AsSpan()[1].Count = 1;
        map.Fields.AsSpan()[2].Count = 1;
        map.Fields.AsSpan()[0].Offset = 0;
        map.Fields.AsSpan()[1].Offset = 4;
        map.Fields.AsSpan()[2].Offset = 8;
        map.Fields.AsSpan()[0].Datatype = 7;
        map.Fields.AsSpan()[1].Datatype = 7;
        map.Fields.AsSpan()[2].Datatype = 7;
        map.Fields.AsSpan()[0].Name.CopyFrom("x");
        map.Fields.AsSpan()[1].Name.CopyFrom("y");
        map.Fields.AsSpan()[2].Name.CopyFrom("z");
        map.IsBigendian = false;
        map.PointStep = 12;
        map.IsDense = true;
        map.Height = 1;
        map.Width = (uint)pointCloud.Points.Length;
        map.RowStep = (uint)pointCloud.Points.Length * 12;
        map.Data.CopyFrom(pointCloud.Points.AsSpan().Cast<Vector3, byte>());
        Std.FromData(pointCloud.Identifier, node, ref map.Header);
    }
}