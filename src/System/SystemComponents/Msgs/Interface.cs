namespace AllianceDM.IO.ROS2Msgs
{

    public delegate void RevcAction<T>(T msg);
    class TlarcMsgs
    {
        static protected bool ReadLock = false;
        static internal event Action Input;
        // static internal event Action Output;

        static public void InputData()
        {
            ReadLock = true;
            Input();
            ReadLock = false;
        }
    }
}