// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_ACTION_CMD_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>动作命令信息</summary>
  public struct MV_ACTION_CMD_INFO
  {
    /// <summary>设备密钥</summary>
    public uint nDeviceKey;
    /// <summary>组键</summary>
    public uint nGroupKey;
    /// <summary>组掩码</summary>
    public uint nGroupMask;
    /// <summary>只有设置成1时Action Time才有效，非1时无效</summary>
    public uint bActionTimeEnable;
    /// <summary>预定的时间，和主频有关</summary>
    public long nActionTime;
    /// <summary>广播包地址</summary>
    public string pBroadcastAddress;
    /// <summary>等待ACK的超时时间，如果为0表示不需要ACK</summary>
    public uint nTimeOut;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public uint[] nReserved;
  }
}
