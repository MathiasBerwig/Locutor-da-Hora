using System;
using System.Windows;

namespace Locutor_da_Hora.Utils
{
    public static class Splasher
    {
        private static Window mSplash;

        public static Window Splash
        {
            get { return mSplash; }
            set { mSplash = value; }
        }

        public static void ShowSplash()
        {
            mSplash?.Show();
        }
        
        public static void CloseSplash()
        {
            if (mSplash != null)
            {
                mSplash.Close();
                if (mSplash is IDisposable)
                    (mSplash as IDisposable).Dispose();
            }
        }
    }

}
