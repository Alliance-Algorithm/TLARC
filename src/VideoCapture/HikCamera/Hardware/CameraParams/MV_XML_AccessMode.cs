// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_XML_AccessMode
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>节点的读写性</summary>
  public enum MV_XML_AccessMode
  {
    /// <summary>未实现</summary>
    AM_NI,
    /// <summary>不可获取</summary>
    AM_NA,
    /// <summary>只写</summary>
    AM_WO,
    /// <summary>只读</summary>
    AM_RO,
    /// <summary>可读可写</summary>
    AM_RW,
    /// <summary>未定义</summary>
    AM_Undefined,
    /// <summary>内部用于AccessMode循环检测</summary>
    AM_CycleDetect,
  }
}
