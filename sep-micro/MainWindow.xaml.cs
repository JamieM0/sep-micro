using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

namespace sep_micro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool encryptMode = true;
        public MainWindow()
        {
            InitializeComponent();
        }

        string[] selectedFilePaths;
        string[] selectedFileNames;
        private void btnSelectFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                selectedFilePaths = ofd.FileNames;
                selectedFileNames = ofd.SafeFileNames;

                FileHandler();
            }
        }

        private void FileHandler()
        {
            //If any of the selected files end with the extension '.aes', then all of the selected files need to. If they don't give an error
            foreach (string file in selectedFileNames)
            {
                if (file.EndsWith(".aes"))
                {
                    bool allEncrypted = true;
                    foreach (string subfile in selectedFileNames)
                    {
                        if (!subfile.EndsWith(".aes"))
                            allEncrypted = false;
                    }
                    if (allEncrypted)
                        encryptMode = false;
                    else
                    {
                        MessageBox.Show("You have some files that are already encrypted!\r\nPlease ensure that either all the selected files have the extension of '.aes', or none of them do.");
                        selectedFileNames = null;
                        selectedFilePaths = null;
                        lbFileDisplayPreview.Content = "Drag and drop files here";
                        return;
                    }

                }
            }

            if (selectedFilePaths.Length == 0)
            {
                MessageBox.Show("Drag and drop files here");
            }
            else if (selectedFilePaths.Length == 1)
            {
                lbFileDisplayPreview.Content = selectedFileNames[0];
            }
            else
            {
                lbFileDisplayPreview.Content = selectedFilePaths.Length.ToString() + " files selected";
            }

            if (!encryptMode)
            {
                btnFunction.Content = "Decrypt File";
            }
            else
            {
                btnFunction.Content = "Encrypt File";
            }
        }

        private void btnFunction_Click(object sender, RoutedEventArgs e)
        {
            if (pwSecretKey.Password == null || pwSecretKey.Password == "")
            {
                MessageBox.Show("You must enter a password.");
            }
            else if (selectedFilePaths == null || selectedFilePaths.Length < 1)
            {
                MessageBox.Show("You must select at least 1 file to encrypt or decrypt.");
            }
            else
            {
                pwSecretKey.IsEnabled = false;
                if (encryptMode)
                {
                    foreach (string file in selectedFilePaths)
                    {
                        AES.EncryptFile(file, file + ".aes", pwSecretKey.Password);
                    }
                    MessageBox.Show($"{selectedFilePaths.Length} files encrypted.", "SEP");
                }
                else
                {
                    foreach (string file in selectedFilePaths)
                    {
                        AES.DecryptFile(file, file.Substring(0, file.Length - 4), pwSecretKey.Password);
                    }
                    MessageBox.Show($"{selectedFilePaths.Length} files decrypted.", "SEP");
                }
                if (cbDeleteFiles.IsChecked == true)
                {
                    foreach (string file in selectedFilePaths)
                    {
                        OtherOperations.WipeFile(file, 3);
                    }
                }
                pwSecretKey.IsEnabled = true;
                pwSecretKey.Password = "";
                lbFileDisplayPreview.Content = "Drag and drop files here";
                selectedFileNames = null;
                selectedFilePaths = null;
            }
        }

        private void btnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            selectedFileNames = null;
            selectedFilePaths = null;
            lbFileDisplayPreview.Content = "Drag and drop files here";
            btnFunction.Content = "Ready";
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(pwSecretKey.Password);
        }

        private void ShowPasswordFunction()
        {
            pwVisible.Visibility = Visibility.Visible;
            pwSecretKey.Visibility = Visibility.Hidden;
            pwVisible.Text = pwSecretKey.Password;
        }

        private void HidePasswordFunction()
        {
            pwVisible.Visibility = Visibility.Hidden;
            pwSecretKey.Visibility = Visibility.Visible;
            pwVisible.Text = "";
        }

        private void btnShowPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowPasswordFunction();
        }

        private void btnShowPassword_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HidePasswordFunction();
        }

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                selectedFilePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                //Get safe file names
                selectedFileNames = new string[selectedFilePaths.Length];
                for (int i = 0; i < selectedFilePaths.Length; i++)
                {
                    selectedFileNames[i] = System.IO.Path.GetFileName(selectedFilePaths[i]);
                }
                if (selectedFilePaths.Length == 0)
                {
                    MessageBox.Show("Drag and drop files here");
                }
                else if (selectedFilePaths.Length == 1)
                {
                    lbFileDisplayPreview.Content = selectedFileNames[0];
                }
                else
                {
                    lbFileDisplayPreview.Content = selectedFilePaths.Length.ToString() + " files selected";
                }
            }

            FileHandler();
        }
    }
}