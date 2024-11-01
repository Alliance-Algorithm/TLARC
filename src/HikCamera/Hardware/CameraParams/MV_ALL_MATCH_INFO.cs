// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_ALL_MATCH_INFO
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>ALL MATHCH INFO</summary>
  public struct MV_ALL_MATCH_INFO
  {
    /// <summary>需要输出的信息类型，e.g. MV_MATCH_TYPE_NET_DETECT</summary>
    public uint nType;
    /// <summary>输出的信息缓存，由调用者分配</summary>
    public IntPtr pInfo;
    /// <summary>信息缓存的大小</summary>
    public uint nInfoSize;
  }
}
