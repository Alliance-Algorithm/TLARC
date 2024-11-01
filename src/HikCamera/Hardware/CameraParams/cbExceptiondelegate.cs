// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.cbExceptiondelegate
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

namespace MvsSharp
{
  /// <summary>Exception callback</summary>
  /// <param name="nMsgType">Msg type</param>
  /// <param name="pUser">User defined variable</param>
  public delegate void cbExceptiondelegate(uint nMsgType, IntPtr pUser);
}
