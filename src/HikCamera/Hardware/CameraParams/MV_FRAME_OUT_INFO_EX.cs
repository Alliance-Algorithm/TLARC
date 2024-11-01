// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_FRAME_OUT_INFO_EX
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>Frame Out Info Ex</summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8)]
  public struct MV_FRAME_OUT_INFO_EX
  {
    /// <summary>图像宽</summary> 
    public ushort nWidth;
    /// <summary>图像高</summary>
    public ushort nHeight;
    /// <summary>像素格式</summary>
    public ulong enPixelType;
    /// <summary>帧号</summary>
    public uint nFrameNum;
    /// <summary>时间戳高32位</summary>
    public uint nDevTimeStampHigh;
    /// <summary>时间戳低32位</summary>
    public uint nDevTimeStampLow;
    /// <summary>保留，8字节对齐</summary>
    public uint nReserved0;
    /// <summary>主机生成的时间戳</summary>
    public long nHostTimeStamp;
    /// <summary>Frame大小</summary>
    public uint nFrameLen;
    /// <summary>秒数</summary>
    public uint nSecondCount;
    /// <summary>周期数</summary>
    public uint nCycleCount;
    /// <summary>周期偏移量</summary>
    public uint nCycleOffset;
    /// <summary>增益</summary>
    public float fGain;
    /// <summary>曝光时间</summary>
    public float fExposureTime;
    /// <summary>平均亮度</summary>
    public uint nAverageBrightness;
    /// <summary>Red</summary>
    public uint nRed;
    /// <summary>Green</summary>
    public uint nGreen;
    /// <summary>Blue</summary>
    public uint nBlue;
    /// <summary>帧计数器</summary>
    public uint nFrameCounter;
    /// <summary>触发计数</summary>
    public uint nTriggerIndex;
    /// <summary>输入</summary>
    public uint nInput;
    /// <summary>输出</summary>
    public uint nOutput;
    /// <summary>水平偏移量</summary>
    public ushort nOffsetX;
    /// <summary>垂直偏移量</summary>
    public ushort nOffsetY;
    /// <summary>Chunk宽度</summary>
    public ushort nChunkWidth;
    /// <summary>Chunk高度</summary>
    public ushort nChunkHeight;
    /// <summary>丢包数</summary>
    public uint nLostPacket;
    /// <summary>为解析的Chunk数量</summary>
    public uint nUnparsedChunkNum;
    /// <summary>为解析的Chunk列表</summary>
    public MV_FRAME_OUT_INFO_EX.UNPARSED_CHUNK_LIST UnparsedChunkList;
    /// <summary>图像宽扩展</summary>
    public uint nExtendWidth;
    /// <summary>图像高扩展</summary>
    public uint nExtendHeight;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 34)]
    public uint[] nReserved;

    /// <summary>为解析的Chunk列表</summary>
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct UNPARSED_CHUNK_LIST
    {
      /// <summary>为解析的Chunk内容</summary>
      [FieldOffset(0)]
      public IntPtr pUnparsedChunkContent;
      /// <summary>对齐结构体，无实际用途</summary>
      [FieldOffset(0)]
      public long nAligning;
    }
  }
}
