using System;

namespace ZipCompress
{
    abstract class IZip
    {
        
        protected static bool stop = false;
        public void Started()
        {
            Operation();
        }
        protected static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            stop = true;
            e.Cancel = true;
        }

        abstract public void Operation();
    }
}
