using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Lib.Common.Components.Func
{
    public class GeneralTools
    {
        public static string GetLocalIP()
        {
            Match m = Regex.Match(RunApp("route", "print", true), @"0.0.0.0\s+0.0.0.0\s+(\d+.\d+.\d+.\d+)\s+(\d+.\d+.\d+.\d+)");

            return m.Success == false ? null : m.Groups[2].Value;

            static string RunApp(string filename, string arguments, bool recordLog)
            {
                try
                {
                    if (recordLog) Trace.WriteLine(filename + " " + arguments);

                    Process proc = new();
                    proc.StartInfo.FileName = filename;
                    proc.StartInfo.CreateNoWindow = true;
                    proc.StartInfo.Arguments = arguments;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();

                    using StreamReader sr = new(proc.StandardOutput.BaseStream, Encoding.UTF8);

                    string result = sr.ReadToEnd();

                    sr.Close();

                    if (recordLog) Trace.WriteLine(result);

                    if (!proc.HasExited) proc.Kill();

                    return result;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);

                    return e.Message;
                }
            }
        }
    }
}
