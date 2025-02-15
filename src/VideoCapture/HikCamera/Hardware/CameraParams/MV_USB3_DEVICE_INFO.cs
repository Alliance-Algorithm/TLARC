// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_USB3_DEVICE_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>USB设备信息</summary>
  public struct MV_USB3_DEVICE_INFO
  {
    /// <summary>控制输入端点</summary>
    public byte CrtlInEndPoint;
    /// <summary>控制输出端点</summary>
    public byte CrtlOutEndPoint;
    /// <summary>流端点</summary>
    public byte StreamEndPoint;
    /// <summary>事件端点</summary>
    public byte EventEndPoint;
    /// <summary>供应商ID号</summary>
    public ushort idVendor;
    /// <summary>产品ID号</summary>
    public ushort idProduct;
    /// <summary>设备序列号</summary>
    public uint nDeviceNumber;
    /// <summary>设备GUID号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDeviceGUID;
    /// <summary>供应商名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chVendorName;
    /// <summary>型号名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chModelName;
    /// <summary>家族名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chFamilyName;
    /// <summary>设备版本号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chDeviceVersion;
    /// <summary>制造商名字</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chManufacturerName;
    /// <summary>序列号</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string chSerialNumber;
    /// <summary>用户自定义名字</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
    public byte[] chUserDefinedName;
    /// <summary>支持的USB协议</summary>
    public uint nbcdUSB;
    /// <summary>设备地址</summary>
    public uint nDeviceAddress;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public uint[] nReserved;
  }
}
