// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.CameraParams.MV_XML_Visibility
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp.CameraParams
{
  /// <summary>节点是否可见的权限等级</summary>
  public enum MV_XML_Visibility
  {
    /// <summary>Always visible</summary>
    V_Beginner = 0,
    /// <summary>Visible for experts or Gurus</summary>
    V_Expert = 1,
    /// <summary>Visible for Gurus</summary>
    V_Guru = 2,
    /// <summary>Not Visible</summary>
    V_Invisible = 3,
    /// <summary>Object is not yet initialized</summary>
    V_Undefined = 99, // 0x00000063
  }
}
