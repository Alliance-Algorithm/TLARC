using TlarcRosBridge.Infrastructure.Messages.Std;
using Int16 = TlarcRosBridge.Infrastructure.Messages.Std.Int16;
using Int32 = TlarcRosBridge.Infrastructure.Messages.Std.Int32;
using Int64 = TlarcRosBridge.Infrastructure.Messages.Std.Int64;
using UInt16 = TlarcRosBridge.Infrastructure.Messages.Std.UInt16;
using UInt32 = TlarcRosBridge.Infrastructure.Messages.Std.UInt32;
using UInt64 = TlarcRosBridge.Infrastructure.Messages.Std.UInt64;

using System;

namespace TlarcRosBridge.Infrastructure.Decorators;

internal static class Std
{
    public static void FromData(ReadOnlySpan<char> dataIn, Rcl.IRclNode node, ref Header.Priv dataOut)
    {
        var now = (long)node.Clock.Elapsed.TotalNanoseconds;
        dataOut.Stamp.Sec = (int)(now / 1_000_000_000);
        dataOut.Stamp.Nanosec = (uint)(now % 1_000_000_000);
        dataOut.FrameId.CopyFrom(dataIn);
    }


    #region Int

    public static void FromData(sbyte dataIn, ref Int8.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(byte dataIn, ref UInt8.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(short dataIn, ref Int16.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(ushort dataIn, ref UInt16.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(int dataIn, ref Int32.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(uint dataIn, ref UInt32.Priv dataOut) => dataOut.Data = dataIn;


    public static void FromData(long dataIn, ref Int64.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(ulong dataIn, ref UInt64.Priv dataOut) => dataOut.Data = dataIn;

    //
    public static void FromData(ReadOnlySpan<sbyte> dataIn, ref Int8MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<byte> dataIn, ref UInt8MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<short> dataIn, ref Int16MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<ushort> dataIn, ref UInt16MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<int> dataIn, ref Int32MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<uint> dataIn, ref UInt32MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);


    public static void FromData(ReadOnlySpan<long> dataIn, ref Int64MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    public static void FromData(ReadOnlySpan<ulong> dataIn, ref UInt64MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    #endregion

    #region Float

    public static void FromData(float dataIn, ref Float32.Priv dataOut) => dataOut.Data = dataIn;

    public static void FromData(double dataIn, ref Float64.Priv dataOut) => dataOut.Data = dataIn;


    public static void FromData(ReadOnlySpan<float> dataIn, ref Float32MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);


    public static void FromData(ReadOnlySpan<double> dataIn, ref Float64MultiArray.Priv dataOut) =>
        dataOut.Data.CopyFrom(dataIn);

    #endregion

    public static void FromData(bool dataIn, ref Bool.Priv dataOut) => dataOut.Data = dataIn;
}