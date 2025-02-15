// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_SORT_METHOD
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>排序方式</summary>
  public enum MV_SORT_METHOD
  {
    /// <summary>按序列号排序</summary>
    SORTMETHOD_SERIALNUMBER,
    /// <summary>按用户自定义名字排序</summary>
    SORTMETHOD_USERID,
    /// <summary>按当前IP地址排序（升序）</summary>
    SORTMETHOD_CURRENTIP_ASC,
    /// <summary>按当前IP地址排序（降序）</summary>
    SORTMETHOD_CURRENTIP_DESC,
  }
}
