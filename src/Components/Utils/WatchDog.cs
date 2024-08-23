namespace Tlarc.Utils
{
    class WatchDog(float TimeLimitInSeccond, Action CallBack)
    {
        float timer = 0;
        float timeLimit = TimeLimitInSeccond;
        Action callBacck = CallBack;
        public void Feed()
        {
            timer = DateTime.Now.Second + DateTime.Now.Microsecond * 0.001f;
        }
        public void Update()
        {
            if (DateTime.Now.Second + DateTime.Now.Microsecond * 0.001f - timer > timeLimit)
                callBacck();
        }
    }
}