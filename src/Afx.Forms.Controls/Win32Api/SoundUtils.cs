using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public class SoundUtils
    {
       private static string dataFilePathName = AppDomain.CurrentDomain.BaseDirectory + "Sound\\msg.wav";
        // Methods
       public static void MsgSound(string wavFile = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(wavFile) && File.Exists(wavFile))
                {
                    sndPlaySound(wavFile, 1);
                }
            }
            catch { }
        }

        [DllImport("winmm.dll")]
        private static extern bool PlaySound(string szSound, IntPtr hMod, int flags);
        [DllImport("winmm.dll")]
        private static extern bool sndPlaySound(string szSound, int flags);
    }
}
