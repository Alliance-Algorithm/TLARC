// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.MV_ACCESS_MODE
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>设备的访问模式</summary>
  public enum MV_ACCESS_MODE
  {
    /// <summary>独占权限，其他APP只允许读CCP寄存器</summary>
    MV_ACCESS_EXCLUSIVE = 1,
    /// <summary>可以从5模式下抢占权限，然后以独占权限打开</summary>
    MV_ACCESS_EXCLUSIVEWITHSWITCH = 2,
    /// <summary>控制权限，其他APP允许读所有寄存器</summary>
    MV_ACCESS_CONTROL = 3,
    /// <summary>可以从5的模式下抢占权限，然后以控制权限打开</summary>
    MV_ACCESS_CONTROLWITHSWITCH = 4,
    /// <summary>以可被抢占的控制权限打开</summary>
    MV_ACCESS_CONTROLSWITCHENABLE = 5,
    /// <summary>可以从5的模式下抢占权限，然后以可被抢占的控制权限打开</summary>
    MV_ACCESS_CONTROLSWITCHENABLEWITHKEY = 6,
    /// <summary>读模式打开设备，适用于控制权限下</summary>
    MV_ACCESS_MONITOR = 7,
  }
}
