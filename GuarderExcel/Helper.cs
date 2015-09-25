using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace Guarder
{
    class Helper
    {
        public static void CloseWaiter(string windowTitle)
        {
            Thread.Sleep(1000);
            windowTitle = windowTitle.ToLower();
            Process[] processlist = Process.GetProcesses();
            foreach (Process process in processlist)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    if (process.MainWindowTitle.ToLower().Contains(windowTitle))
                    {
                        process.CloseMainWindow();
                    }
                }
            }
        }

        public static void WriteRetryOrIgnore(bool bRetry)
        {
            string retryOrIgnore = bRetry ? "Retry" : "Ignore";
            string sFile = GetExeFolder() + "retryOrIgnore.txt";
            File.WriteAllText(sFile, retryOrIgnore);
        }

        public static void WriteResult(bool bok)
        {
            string sFile = GetExeFolder() + "result.txt";
            File.WriteAllText(sFile, bok.ToString());
        }

        public static string GetExeFolder()
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }


    }
}
