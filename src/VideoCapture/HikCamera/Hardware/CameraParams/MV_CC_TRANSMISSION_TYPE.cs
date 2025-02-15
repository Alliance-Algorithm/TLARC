// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_CC_TRANSMISSION_TYPE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>传输模式，可以为单播模式、组播模式等</summary>
  public struct MV_CC_TRANSMISSION_TYPE
  {
    /// <summary>传输模式</summary>
    public MV_GIGE_TRANSMISSION_TYPE enTransmissionType;
    /// <summary>目标IP，组播模式下有意义</summary>
    public uint nDestIp;
    /// <summary>目标Port，组播模式下有意义</summary>
    public ushort nDestPort;
    /// <summary>保留字节</summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public uint[] nReserved;
  }
}
