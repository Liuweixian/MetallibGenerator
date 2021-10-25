using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Unity.MetallibGenertaor
{
    public class CommandLine
    {
        public static bool Run(string cmdString, out string outputString, string warpString = "\r\n")
        {
            try
            {
                Process p = new Process();
#if UNITY_EDITOR_OSX
                p.StartInfo.FileName = "/bin/zsh";
                p.StartInfo.Arguments = "-i -c \"" + cmdString + "\"";
#elif UNITY_EDITOR_WIN
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = "/c " + cmdString;
#endif
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                string retString = null;
                string errorString = null;
                bool ret = true;
                p.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    retString += e.Data + warpString;
                });

                p.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorString += e.Data + warpString;
                        ret = false;
                    }
                });

                p.Start();

                p.StandardInput.Flush();
                p.StandardInput.Close();

                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                p.WaitForExit();
                p.Close();

                outputString = retString;
                if (!string.IsNullOrEmpty(errorString))
                    Debug.LogError(errorString);
                return ret;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message + "\r\n" + ex.StackTrace);
                outputString = null;
                return false;
            }
        }
    }
}