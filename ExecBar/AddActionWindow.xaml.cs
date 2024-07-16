using System.Windows;
using Microsoft.Win32; // For Microsoft.Win32.OpenFileDialog
using System.Windows.Forms; // For System.Windows.Forms.FolderBrowserDialog

namespace ExecBar
{
    public partial class AddActionWindow : Window
    {
        public ActionItem ActionItem { get; private set; }

        public AddActionWindow()
        {
            InitializeComponent();
            ActionItem = new ActionItem();
            DataContext = ActionItem;
        }

        public AddActionWindow(ActionItem actionItem) : this()
        {
            ActionNameTextBox.Text = actionItem.ActionName;
            PathTextBox.Text = actionItem.Path;
        }

        private void FileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                PathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PathTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ActionItem.ActionName = ActionNameTextBox.Text;
            ActionItem.Path = PathTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
