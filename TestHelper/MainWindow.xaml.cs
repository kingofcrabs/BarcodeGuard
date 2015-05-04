using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Linq;


namespace TestHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }
        public  string GetExeFolder()
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return s + "\\";
        }
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string sFolder = GetExeFolder();
            var files = Directory.EnumerateFiles(sFolder, "*.csv");
            List<string> lstFileStr = new List<string>(files);
            files = lstFileStr.OrderBy(x => GetNumber(x));
            lstFiles.ItemsSource = files;
        }

        private string GetNumber(string x)
        {
            int index = x.IndexOf(".csv");
            string s = x.Substring(index - 2, 2);
            return s;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (lstFiles.SelectedIndex == -1)
                return;
            string file = (string)lstFiles.SelectedItem;
            File.Copy(file,@"c:\biobanking\scan.csv",true);
            string sFolder = GetExeFolder();
            string sNotifier = sFolder + "";
            Process.Start("Notifier.exe", "gg").WaitForExit();
        }

    

       
    }
}
