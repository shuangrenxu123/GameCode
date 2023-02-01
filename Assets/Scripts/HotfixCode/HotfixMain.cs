using System;

namespace Hotfix
{
    public class HotfixMain
    {
        static Action Update;
        static Action Start;
        // Start is called before the first frame update
        public static void start()
        {
            Start?.Invoke();
        }

        public static void update()
        {
            Update?.Invoke();
        }
    }
}
