namespace Tlarc.IO.ROS2Msgs
{

    class TlarcRosMsgs
    {
        internal event Action Input;
        // static internal event Action Output;

        public void InputData() =>
           Input?.Invoke();
    }
}