// Decompiled with JetBrains decompiler
// Type: MvCamCtrl.NET.cbStreamExceptiondelegate
// Assembly: MvCamCtrl.Net, Version=4.1.0.3, Culture=neutral, PublicKeyToken=52fddfb3f94be800
// MVID: 48858B75-944A-430D-BA88-8043A97023D9
// Assembly location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.dll
// XML documentation location: C:\Program Files (x86)\MVS\Development\DotNet\win64\MvCamCtrl.Net.xml

using MvsSharp.CameraParams;

namespace MvsSharp
{
  /// <summary>U3V Stream exception callback</summary>
  /// <param name="enExceptionType">取流异常类型</param>
  /// <param name="pUser">用户自定义变量</param>
  public delegate void cbStreamExceptiondelegate(
    MV_CC_STREAM_EXCEPTION_TYPE enExceptionType,
    IntPtr pUser);
}
