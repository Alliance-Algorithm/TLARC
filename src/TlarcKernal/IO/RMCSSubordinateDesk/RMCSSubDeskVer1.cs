using System;
using MonoLibUsb;
using Tlarc.DataStructure;


namespace Tlarc.IO.RMCS;

class RMCSSubDeskVer1 : Component
{
    private UInt16 _pid;
    private UInt16 _vid;
    private byte _inEndpoint = 0x81;
    private byte _outEndpoint = 0x01;
    private byte _configuration = 0x00;
    private byte _interface = 0x01;
    private MonoUsbDeviceHandle _device;
    public (UInt16 Pid, UInt16 Vid) TargetDevice => (_pid, _vid);
    RingBuffer _readBuffer = new RingBuffer(1024);
    public virtual void OnCan1Receive(
        UInt32 canId,
        UInt64 canData,
        Boolean IsExtendedCANId,
        Boolean IsRemoteTransmission,
        Byte canDataLength)
    { }
    public virtual void OnCAN2Receive(
        UInt32 canId,
        UInt64 canData,
        Boolean IsExtendedCANId,
        Boolean IsRemoteTransmission,
        Byte canDataLength)
    { }
    public virtual void OnUART1Receive(byte[] uartData) { }
    public virtual void OnUART2Receive(byte[] uartData) { }
    public virtual void OnDBusReceive(byte[] dBusData) { }
    public virtual void OnAccelerometerReceive(Int16 x, Int16 y, Int16 z) { }
    public virtual void OnGyroscopeReceive(Int16 x, Int16 y, Int16 z) { }

    public virtual void OnStartBeforeUSBOpen(ref UInt16 pid, ref UInt16 vid, ref byte inEndpoint, ref byte outEndpoint) { }
    public virtual void OnStartAfterUSBOpen() { }

    public sealed override void Start()
    {
        OnStartBeforeUSBOpen(ref _pid, ref _vid, ref _inEndpoint, ref _outEndpoint);
        var sessionHandle = new MonoUsbSessionHandle();
        if (sessionHandle.IsInvalid) throw new Exception("Invalid session handle.");
        _device = MonoUsbApi.OpenDeviceWithVidPid(sessionHandle, (short)_vid, (short)_pid);

        if (_device == null || _device.IsInvalid) throw new Exception("usb Device Not Fount");

        var ret = MonoUsbApi.SetConfiguration(_device, _configuration);
        if (ret < 0)
            throw new Exception(string.Format("Set Configuration Error: Vid:{0} , Pid:{1} , Endpoint:{2}, Configuration:{3}, Interface:{4},ErrorCode: {5}", _vid, _pid, _inEndpoint, _configuration, _interface, ret));

        MonoUsbApi.ClaimInterface(_device, _interface);
        if (ret < 0)
            throw new Exception(string.Format("Set Interface Error: Vid:{0} , Pid:{1} , Endpoint:{2}, Configuration:{3}, Interface:{4} ,ErrorCode: {5}", _vid, _pid, _inEndpoint, _configuration, _interface, ret));


        Task.Run(() => { while (true) USBReadLoop(); });
    }
    private void USBReadLoop()
    {
        unsafe
        {
            using var header = _readBuffer.HeadOfBuffer;
            var ret = MonoUsbApi.BulkTransfer(_device, _inEndpoint, (nint)header.Pointer, _readBuffer.BufferSizeOfByte, out var actualLength, 0);
            if (ret < 0)
                throw new Exception(string.Format("Read Transform Error: Vid:{0} , Pid:{1} , Endpoint:{2}, Configuration:{3}, Interface:{4},ErrorCode: {5}", _vid, _pid, _inEndpoint, _configuration, _interface, ret));
            _readBuffer.UpdateHeadPointer(actualLength);
        }
    }
}