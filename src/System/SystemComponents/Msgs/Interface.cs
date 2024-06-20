namespace AllianceDM.IO.ROS2Msgs
{

    class TlarcMsgs
    {
        static internal event Action Input;
        // static internal event Action Output;

        static public void InputData() =>
            Input?.Invoke();
    }
}