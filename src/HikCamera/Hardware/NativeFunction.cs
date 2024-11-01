
using System;
using System.Runtime.InteropServices;
using MvsSharp;
using MvsSharp.CameraParams;

namespace HikCamera.Hardware;

//
// 摘要:
//     MvCameraControl 动态库中导出的方法。
internal static class NativeFunction
{
    //
    // 摘要:
    //     动态库的文件名。
    public const string DllFileName = "MvCameraControl";

    //
    // 摘要:
    //     初始化 SDK。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_Initialize();

    //
    // 摘要:
    //     枚举设备，并建立设备列表。
    //
    // 参数:
    //   nTLayerTypes:
    //     接口类型。
    //
    //   stDevList:
    //     设备信息的列表。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_EnumDevices(KinTN.Hardware.HikMachineVisionCamera.MV_DEVICE_TYPES nTLayerTypes, ref MV_CC_DEVICE_INFO_LIST stDevList);

    //
    // 摘要:
    //     判断设备是否可访问。
    //
    // 参数:
    //   stDevInfo:
    //     设备信息。
    //
    //   nAccessMode:
    //     访问权限。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern bool MV_CC_IsDeviceAccessible(ref MV_CC_DEVICE_INFO stDevInfo, MV_ACCESS_MODE nAccessMode);

    //
    // 摘要:
    //     创建句柄。
    //
    // 参数:
    //   handle:
    //     创建的句柄。
    //
    //   stDevInfo:
    //     设备信息。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_CreateHandle(ref IntPtr handle, ref MV_CC_DEVICE_INFO stDevInfo);

    //
    // 摘要:
    //     打开设备。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   nAccessMode:
    //     访问权限。
    //
    //   nSwitchoverKey:
    //     切换访问权限时的密钥，传 0。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_OpenDevice(IntPtr handle, MV_ACCESS_MODE nAccessMode, ushort nSwitchoverKey);

    //
    // 摘要:
    //     获取枚举值。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   strKey:
    //     参数名。
    //
    //   pEnumValue:
    //     返回的枚举信息。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_GetEnumValue(IntPtr handle, string strKey, ref KinTN.Hardware.HikMachineVisionCamera.MV_CC_ENUMENTRY pEnumValue);

    //
    // 摘要:
    //     设置枚举值。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   strKey:
    //     参数名。
    //
    //   nValue:
    //     新的值。
    [DllImport("MvCameraControl", CharSet = CharSet.Ansi)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_SetEnumValue(IntPtr handle, string strKey, uint nValue);
    [DllImport("MvCameraControl", CharSet = CharSet.Ansi)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_SetFloatValue(IntPtr handle, string strKey, float fValue);

    [DllImport("MvCameraControl", CharSet = CharSet.Ansi)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_SetBoolValue(IntPtr handle, string strKey, bool bValue);


    [DllImport("MvCameraControl")]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_SetBayerCvtQuality(IntPtr handle, uint nBayerCvtQuality);
    //
    // 摘要:
    //     执行命令。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   strKey:
    //     命令名。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_SetCommandValue(IntPtr handle, string strKey);

    //
    // 摘要:
    //     启动采集。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_StartGrabbing(IntPtr handle);

    //
    // 摘要:
    //     获取一帧图像。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   pFrame:
    //     返回的帧信息。
    //
    //   nMsec:
    //     超时时间。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_GetImageBuffer(IntPtr handle, ref MV_FRAME_OUT pFrame, int nMsec);

    //
    // 摘要:
    //     释放图像缓存区。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    //
    //   pFrame:
    //     帧信息。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_FreeImageBuffer(IntPtr handle, ref MV_FRAME_OUT pFrame);


    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_ConvertPixelType(IntPtr handle, ref MV_CC_PIXEL_CONVERT_PARAM pFrame);

    //
    // 摘要:
    //     停止采集。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_StopGrabbing(IntPtr handle);

    //
    // 摘要:
    //     关闭设备。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_CloseDevice(IntPtr handle);

    //
    // 摘要:
    //     释放句柄。
    //
    // 参数:
    //   handle:
    //     由 MV_CC_CreateHandle 创建的句柄。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_DestroyHandle(IntPtr handle);

    //
    // 摘要:
    //     释放 SDK 资源。
    [DllImport("MvCameraControl", CallingConvention = CallingConvention.StdCall)]
    public static extern KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE MV_CC_Finalize();
}