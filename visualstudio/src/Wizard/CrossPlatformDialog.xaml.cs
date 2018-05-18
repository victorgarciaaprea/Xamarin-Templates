using EnvDTE80;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Xamarin.Templates.Wizards
{
    /// <summary>
    /// Interaction logic for AzureDialog.xaml
    /// </summary>
    public partial class CrossPlatformDialog : DialogBase
    {
        public CrossPlatformDialog ()
        {
            InitializeComponent ();
        }

        internal void SetUWPEnabled(DTE2 dte, string latestSdk)
        {
            var model = DataContext as XPlatViewModel;
            model.IsUWPEnabled = !string.IsNullOrEmpty(latestSdk) && model.GetUWPEnabled(dte);
            model.IsUWPSelected = model.IsUWPEnabled;
        }

        internal string GetTemplatePath()
        {
            var model = DataContext as XPlatViewModel;
            return model.SelectedTemplatePath;
        }

        private void CodeSharingStrategy_Navigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        private void ButtonClick (object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
