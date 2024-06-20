namespace AllianceDM.IO.ROS2Msgs
{

    public delegate void RevcAction<T>(T msg);
    class TlarcMsgs
    {
        static internal event Action Input;
        // static internal event Action Output;

        static public void InputData() =>
            Input?.Invoke();
    }
}