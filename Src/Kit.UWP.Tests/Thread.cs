namespace System.Threading
{
#if WINRT || WINDOWS_UWP

    public class Thread
    {
        public static void Sleep(int milliseconds)
        {
            using (ManualResetEvent waitEvent = new ManualResetEvent(false))
            {
                waitEvent.WaitOne(milliseconds);
            }
        }
    }

#endif
}
