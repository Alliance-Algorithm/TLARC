// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_CAML_BAUDRATE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>CameraLink相机波特率</summary>
  public enum MV_CAML_BAUDRATE
  {
    /// <summary>未知波特率</summary>
    MV_CAML_BAUDRATE_UNKNOW = 0,
    /// <summary>9600</summary>
    MV_CAML_BAUDRATE_9600 = 1,
    /// <summary>19200</summary>
    MV_CAML_BAUDRATE_19200 = 2,
    /// <summary>38400</summary>
    MV_CAML_BAUDRATE_38400 = 4,
    /// <summary>57600</summary>
    MV_CAML_BAUDRATE_57600 = 8,
    /// <summary>115200</summary>
    MV_CAML_BAUDRATE_115200 = 16, // 0x00000010
    /// <summary>230400</summary>
    MV_CAML_BAUDRATE_230400 = 32, // 0x00000020
    /// <summary>460800</summary>
    MV_CAML_BAUDRATE_460800 = 64, // 0x00000040
    /// <summary>921600</summary>
    MV_CAML_BAUDRATE_921600 = 128, // 0x00000080
    /// <summary>自动选择波特率</summary>
    MV_CAML_BAUDRATE_AUTOMAX = 1073741824, // 0x40000000
  }
}
