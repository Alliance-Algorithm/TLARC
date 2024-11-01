// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.StrSymbolic
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using System.Runtime.InteropServices;

namespace MvsSharp.CameraParams
{
  /// <summary>节点描述</summary>
  public struct StrSymbolic
  {
    /// <summary>节点描述</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string str;
  }
}
