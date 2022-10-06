using System.Windows.Controls;

namespace EdlinSoftware.Safe.Views
{
    /// <summary>
    /// Interaction logic for LoginToStorageView.xaml
    /// </summary>
    public partial class LoginToStorageView : UserControl
    {
        public LoginToStorageView()
        {
            InitializeComponent();

            Loaded += (sender, e) => password.Focus();
        }
    }
}
