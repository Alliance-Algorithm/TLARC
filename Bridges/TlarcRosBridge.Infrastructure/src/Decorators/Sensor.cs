using System;
using System.Numerics;
using System.Runtime.InteropServices;

using Microsoft.Toolkit.HighPerformance;
using Rcl;
using TlarcRosBridge.Infrastructure.Messages.Sensor;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Sensor
{
    public static Kernel.Contract.Sensor.PointCloud BuildPointCloud(ref Messages.Std.Header.Priv header, Vector3[] points) => new()
        {
            Header = new Kernel.Contract.Header{
                        Identifier = header.FrameId.ToString(),
                        Timestamp  = new(){
                            Second      = header.Stamp.Sec,
                            Nanosecond  = header.Stamp.Nanosec,
                        }},
            Points = points
        };
    public static Kernel.Contract.Sensor.PointCloud XYZPointCloud(ref PointCloud2.Priv map)
    {
        var data = map.Data.AsSpan().Cast<byte, Vector3>();
        var points = new Vector3[data.Length];
        for (var i = 0; i < data.Length; ++i)
            points[i] = new Vector3(data[i].X, data[i].Y, data[i].Z);
        return BuildPointCloud(ref map.Header,points);
    }

    [StructLayout(LayoutKind.Explicit, Size = 48, Pack = 1)]
    private struct FastLioPointCloud
    {
        [FieldOffset(0)] public float x;
        [FieldOffset(4)] public float y;
        [FieldOffset(8)] public float z;
    }

    public static Kernel.Contract.Sensor.PointCloud FastLioRegistered(ref PointCloud2.Priv map)
    {
        var data = map.Data.AsSpan().Cast<byte, FastLioPointCloud>();
        var points = new Vector3[data.Length];
        for (var i = 0; i < data.Length; ++i)
            points[i] = new Vector3(data[i].x, data[i].y, data[i].z);
        return BuildPointCloud(ref map.Header,points);
    }

    public static void WriteIntoPointCloud(Kernel.Contract.Sensor.PointCloud pointCloud, IRclNode node, ref PointCloud2.Priv map)
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
        Std.FromData(pointCloud.Header.Identifier, node, ref map.Header);
    }
}