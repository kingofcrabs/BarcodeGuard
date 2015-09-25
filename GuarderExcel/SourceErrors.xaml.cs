using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Guarder
{
    /// <summary>
    /// Interaction logic for SourceErrors.xaml
    /// </summary>
    public partial class SourceErrors : Window
    {
        private List<ErrorInfo> errorsInfo;

        public SourceErrors()
        {
            InitializeComponent();
            lvErrors.SelectionChanged += lvErrors_SelectionChanged;
        }

        void lvErrors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sampleDesc = lvErrors.SelectedItem as ErrorInfo;
            if(sampleDesc != null)
                Clipboard.SetText(sampleDesc.ExpectedBarcode);
        }

        public SourceErrors(List<ErrorInfo> errorsInfo, int grid)
            :this()
        {
            // TODO: Complete member initialization
            this.Title = string.Format("Grid:{0}", grid);
            this.errorsInfo = errorsInfo;
            lvErrors.ItemsSource = errorsInfo;
        }
    }
}
