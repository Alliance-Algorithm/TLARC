// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_DEVICE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>ch:设备信息 | en:Device information</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct MV_CC_DEVICE_INFO
  {
    /// <summary>主版本号</summary>
    public ushort nMajorVer;
    /// <summary>次版本号</summary>
    public ushort nMinorVer;
    /// <summary>MAC高地址</summary>
    public uint nMacAddrHigh;
    /// <summary>MAC低地址</summary>
    public uint nMacAddrLow;
    /// <summary>设备传输层协议类型，e.g. MV_GIGE_DEVICE</summary>
    public uint nTLayerType;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
    /// <summary>设备类型</summary>
    public MV_CC_DEVICE_INFO.SPECIAL_INFO SpecialInfo;

    /// <summary>构造函数</summary>
    /// <param name="nAnyNum">输入任意数，因为不接受无参构造函数</param>
    public MV_CC_DEVICE_INFO(uint nAnyNum)
    {
      this.nMajorVer = (ushort)0;
      this.nMinorVer = (ushort)0;
      this.nMacAddrHigh = 0U;
      this.nMacAddrLow = 0U;
      this.nTLayerType = 0U;
      this.nReserved = new uint[4];
      this.SpecialInfo.stGigEInfo = new byte[216];
      this.SpecialInfo.stCamLInfo = new byte[536];
      this.SpecialInfo.stUsb3VInfo = new byte[540];
    }

    /// <summary>ch:特定类型的设备信息 | en:Special devcie information</summary>
    [StructLayout(LayoutKind.Explicit, Size = 540)]
    public struct SPECIAL_INFO
    {
      /// <summary>GigE</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
      [FieldOffset(0)]
      public byte[] stGigEInfo;
      /// <summary>Camera Link</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 536)]
      [FieldOffset(0)]
      public byte[] stCamLInfo;
      /// <summary>Usb</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 540)]
      [FieldOffset(0)]
      public byte[] stUsb3VInfo;
    }
  }
}
