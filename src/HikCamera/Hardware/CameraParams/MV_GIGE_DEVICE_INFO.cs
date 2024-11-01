// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_GIGE_DEVICE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>GigE设备信息</summary>
  public struct MV_GIGE_DEVICE_INFO
  {
    /// <summary>IP配置选项</summary>
    public uint nIpCfgOption;
    /// <summary>IP configuration:bit31-static bit30-dhcp bit29-lla</summary>
    public uint nIpCfgCurrent;
    /// <summary>curtent ip</summary>
    public uint nCurrentIp;
    /// <summary>curtent subnet mask</summary>
    public uint nCurrentSubNetMask;
    /// <summary>current gateway</summary>
    public uint nDefultGateWay;
    /// <summary>制造商名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string chManufacturerName;
    /// <summary>模型名</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string chModelName;
    /// <summary>设备版本信息</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string chDeviceVersion;
    /// <summary>制造商特殊信息</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
    public string chManufacturerSpecificInfo;
    /// <summary>序列号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string chSerialNumber;
    /// <summary>用户自定义名</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] chUserDefinedName;
    /// <summary>网口IP地址</summary>
    public uint nNetExport;
    /// <summary>预留</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] nReserved;
  }
}
