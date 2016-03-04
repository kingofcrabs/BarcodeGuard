using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.IO;
using System.Configuration;
using System.Windows.Forms;

namespace Notifier
{
    class Program
    {
        static void Main(string[] args)
        {
            //if (args.Count() != 1)
            //{
            //    Console.WriteLine("arguments length is not equal to 1, press any key to exit");
            //    var keyInfo = Console.ReadKey();
            //    return;
            //}
            //Console.WriteLine(args[0]);
            string s = "";
            if (args.Count() > 0)
                s = args[0];
            SendCommand(s);
           
        }

        static void SendCommand(string sContent)
        {

            string sProgramName = ConfigurationManager.AppSettings["dstProgramName"];
            using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", sProgramName,
                                                                           PipeDirection.Out,
                                                                        PipeOptions.None))
            {

                Console.WriteLine("Attempting to connect to pipe...\r\n");
                try
                {
                    pipeClient.Connect(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Cannot connect to server, reason: {0}.\r\n", ex.Message));
                    return;
                }
                Console.WriteLine("Connected to pipe.\r\n");

                using (StreamWriter sw = new StreamWriter(pipeClient))
                {
                    sw.Write(sContent);
                }
            }
        }
    }
}
