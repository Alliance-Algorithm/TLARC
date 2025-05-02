using System.Runtime.InteropServices;

namespace HikCamera.Hardware;

//
// 摘要:
//     相机列表
public struct MV_CC_DEVICE_INFO_LIST
{
  //
  // 摘要:
  //     在线设备数量
  public uint nDeviceNum;

  //
  // 摘要:
  //     支持最多256个设备
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
  public IntPtr[] pDeviceInfo;
}
