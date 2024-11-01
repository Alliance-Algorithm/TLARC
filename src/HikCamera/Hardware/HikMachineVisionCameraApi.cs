
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using MvsSharp;
using MvsSharp.CameraParams;
using TlarcKernel;
namespace HikCamera.Hardware;

//
// 摘要:
//     海康机器视觉相机 API。
public static class HikCameraApi
{
    //
    // 摘要:
    //     初始化 SDK。
    public static void InitializeSDK()
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_Initialize();
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     查找 USB 相机。
    public unsafe static Collection<MV_CC_DEVICE_INFO> EnumerateDevices()
    {
        Collection<MV_CC_DEVICE_INFO> collection = new Collection<MV_CC_DEVICE_INFO>();
        MV_CC_DEVICE_INFO_LIST stDevList = default;
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_EnumDevices(KinTN.Hardware.HikMachineVisionCamera.MV_DEVICE_TYPES.MV_USB_DEVICE, ref stDevList);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        for (int i = 0; i < stDevList.nDeviceNum; i++)
        {
            MV_CC_DEVICE_INFO item = Marshal.PtrToStructure<MV_CC_DEVICE_INFO>(stDevList.pDeviceInfo[i]);
            collection.Add(item);
        }

        return collection;
    }

    //
    // 摘要:
    //     转换到 GIGE 相机信息。
    //
    // 参数:
    //   deviceInfo:
    //     查找相机时得到的单个相机信息。
    public static MV_GIGE_DEVICE_INFO ToGigEDeviceInfos(MV_CC_DEVICE_INFO deviceInfo)
    {
        int num = Marshal.SizeOf<MV_GIGE_DEVICE_INFO>();
        IntPtr intPtr = Marshal.AllocHGlobal(num);
        Marshal.Copy(deviceInfo.SpecialInfo.stGigEInfo, 0, intPtr, num);
        MV_GIGE_DEVICE_INFO result = Marshal.PtrToStructure<MV_GIGE_DEVICE_INFO>(intPtr);
        Marshal.FreeHGlobal(intPtr);
        return result;
    }

    //
    // 摘要:
    //     打开相机。
    //
    // 参数:
    //   deviceInfo:
    //     相机信息。
    //
    // 返回结果:
    //     打开的句柄。
    public static IntPtr Open(MV_CC_DEVICE_INFO deviceInfo)
    {
        IntPtr handle = IntPtr.Zero;
        if (!NativeFunction.MV_CC_IsDeviceAccessible(ref deviceInfo, MV_ACCESS_MODE.MV_ACCESS_EXCLUSIVE))
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE.MV_E_ACCESS_DENIED);
        }

        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_CreateHandle(ref handle, ref deviceInfo);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        mV_ERROR_CODE = NativeFunction.MV_CC_OpenDevice(handle, MV_ACCESS_MODE.MV_ACCESS_EXCLUSIVE, 0);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }


        mV_ERROR_CODE = NativeFunction.MV_CC_SetEnumValue(
            handle, "TriggerMode", 0);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
        mV_ERROR_CODE = NativeFunction.MV_CC_SetBoolValue(handle, "ReverseX", false);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
        mV_ERROR_CODE = NativeFunction.MV_CC_SetBoolValue(handle, "ReverseY", false);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        mV_ERROR_CODE = NativeFunction.MV_CC_SetEnumValue(handle, "ExposureAuto", 0);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        mV_ERROR_CODE = NativeFunction.MV_CC_SetFloatValue(handle, "ExposureTime", 5000.0f);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
        mV_ERROR_CODE = NativeFunction.MV_CC_SetFloatValue(handle, "Gain", 0);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
        mV_ERROR_CODE = NativeFunction.MV_CC_SetBoolValue(handle, "AcquisitionFrameRateEnable", false);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        mV_ERROR_CODE = NativeFunction.MV_CC_SetBayerCvtQuality(handle, 2);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
        return handle;

    }

    //
    // 摘要:
    //     获取枚举值。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    //
    //   key:
    //     参数名称。
    public static KinTN.Hardware.HikMachineVisionCamera.MV_CC_ENUMENTRY GetEnumValue(IntPtr handle, string key)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_CC_ENUMENTRY pEnumValue = default(KinTN.Hardware.HikMachineVisionCamera.MV_CC_ENUMENTRY);
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_GetEnumValue(handle, key, ref pEnumValue);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        return pEnumValue;
    }

    //
    // 摘要:
    //     设置枚举值。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    //
    //   key:
    //     参数名称。
    //
    //   value:
    //     新的值。
    public static void SetEnumValue(IntPtr handle, string key, uint value)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_SetEnumValue(handle, key, value);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     执行命令。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    //
    //   command:
    //     命令的名称。
    public static void ExecuteCommand(IntPtr handle, string command)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_SetCommandValue(handle, command);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     开始采集。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    public static void StartGrabbing(IntPtr handle)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_StartGrabbing(handle);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     获取缓冲区里的图像。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    //
    //   frame:
    //     帧信息。
    //
    //   timeoutInMilliseconds:
    //     超时时间。
    //
    // 返回结果:
    //     RGB 格式的图像。
    public unsafe static Mat GetImage(IntPtr handle, int timeoutInMilliseconds)
    {

        MV_FRAME_OUT frame = default(MV_FRAME_OUT);
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_GetImageBuffer(handle, ref frame, timeoutInMilliseconds);

        if (mV_ERROR_CODE != 0)
        {
            return new();
            // throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException($"HikCamera: {mV_ERROR_CODE}");
        }
        var converted_data_size_ =
            frame.stFrameInfo.nWidth * frame.stFrameInfo.nHeight * 3;


        Mat mat = new(frame.stFrameInfo.nHeight, frame.stFrameInfo.nWidth, DepthType.Cv8U, 3);

        MV_CC_PIXEL_CONVERT_PARAM convertParameter = default;
        convertParameter.nWidth = frame.stFrameInfo.nWidth;    // image width
        convertParameter.nHeight = frame.stFrameInfo.nHeight;   // image height
        convertParameter.pSrcData = frame.pBufAddr; // input data size
        convertParameter.nSrcDataLen = frame.stFrameInfo.nFrameLen; // input data size
        convertParameter.enSrcPixelType = (long)frame.stFrameInfo.enPixelType;                       // input pixel format
        convertParameter.enDstPixelType = (long)MvGvspPixelType.PixelType_Gvsp_BGR8_Packed; // output pixel format
        convertParameter.pDstBuffer = mat.DataPointer;     // output data buffer
        convertParameter.nDstBufferSize = (uint)converted_data_size_;       // output buffer size

        NativeFunction.MV_CC_ConvertPixelType(handle, ref convertParameter);
        // DisplayStructMemory(ref convertParameter);

        mV_ERROR_CODE = NativeFunction.MV_CC_FreeImageBuffer(handle, ref frame);
        if (mV_ERROR_CODE != 0)
        {
            return new();
            // throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException($"HikCamera: {mV_ERROR_CODE}");
        }
        return mat;
    }
    static unsafe void DisplayStructMemory<T>(ref T str) where T : struct
    {
        int size = Marshal.SizeOf(typeof(T));
        byte* ptr = (byte*)Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(str, (IntPtr)ptr, false);

            Console.WriteLine("\nMemory layout:");
            for (int i = 0; i < size; i++)
            {
                Console.Write($"{ptr[i]:X2} ");
            }
        }
        finally
        {
            Marshal.FreeHGlobal((IntPtr)ptr);
        }
    }
    //
    // 摘要:
    //     停止采集。
    //
    // 参数:
    //   handle:
    //     打开的句柄。
    public static void StopGrabbing(IntPtr handle)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_StopGrabbing(handle);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     关闭相机。
    //
    // 参数:
    //   handle:
    //     待关闭的句柄。
    public static void Close(IntPtr handle)
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_CloseDevice(handle);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }

        mV_ERROR_CODE = NativeFunction.MV_CC_DestroyHandle(handle);
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }

    //
    // 摘要:
    //     释放 SDK 资源。
    public static void FinalizeSDK()
    {
        KinTN.Hardware.HikMachineVisionCamera.MV_ERROR_CODE mV_ERROR_CODE = NativeFunction.MV_CC_Finalize();
        if (mV_ERROR_CODE != 0)
        {
            throw new KinTN.Hardware.HikMachineVisionCamera.HikMachineVisionCameraException(mV_ERROR_CODE);
        }
    }
}