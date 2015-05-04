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
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(s));
        }
    }
}
