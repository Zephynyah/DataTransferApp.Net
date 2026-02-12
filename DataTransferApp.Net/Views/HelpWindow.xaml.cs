using System.Windows;

namespace DataTransferApp.Net.Views
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml.
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }
    }
}