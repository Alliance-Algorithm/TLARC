// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.cbEventdelegateEx
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using MvsSharp.CameraParams;

namespace MvsSharp
{
  /// <summary>Event callback</summary>
  /// <param name="pEventInfo">Event Info</param>
  /// <param name="pUser">User defined variable</param>
  public delegate void cbEventdelegateEx(ref MV_EVENT_OUT_INFO pEventInfo, IntPtr pUser);
}
