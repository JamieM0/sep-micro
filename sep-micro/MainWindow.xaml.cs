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
using System.Windows.Input;
using Microsoft.Win32;

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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == true)
            {
                selectedFilePaths = ofd.FileNames;
                selectedFileNames = ofd.SafeFileNames;

                //If any of the selected files end with the extension '.aes', then all of the selected files need to. If they don't give an error
                foreach (string file in selectedFileNames)
                {
                    if (file.EndsWith(".aes"))
                    {
                        bool allEncrypted = true;
                        foreach(string subfile in selectedFileNames)
                        {
                            if(!subfile.EndsWith(".aes"))
                                allEncrypted = false;
                        }
                        if (allEncrypted)
                            encryptMode = false;
                        else
                        {
                            MessageBox.Show("You have some files that are already encrypted!\r\nPlease ensure that either all the selected files have the extension of '.aes', or none of them do.");
                            selectedFileNames = null;
                            selectedFilePaths = null;
                            return;
                        }
                            
                    }
                }

                if (selectedFilePaths.Length == 0)
                {
                    MessageBox.Show("No files selected");
                }
                else if (selectedFilePaths.Length==1)
                {
                    lbFileDisplayPreview.Content = selectedFileNames[0];
                }
                else
                {
                    lbFileDisplayPreview.Content = selectedFilePaths.Length.ToString() + " files selected";
                }

                if(!encryptMode)
                {
                    btnFunction.Content = "Decrypt File";
                }
                else
                {
                    btnFunction.Content = "Encrypt File";
                }
            }
        }

        private void pbShowPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pbShowPassword.Source = new BitmapImage(new Uri("pack://application:,,,/password_reveal-active.png"));
        }

        private void pbShowPassword_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pbShowPassword.Source = new BitmapImage(new Uri("pack://application:,,,/password_reveal-default.png"));
        }

        private void pbCopy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            pbCopy.Source = new BitmapImage(new Uri("pack://application:,,,/copy-icon_active.jpg"));
            Clipboard.SetText(pwSecretKey.Password);
        }

        private void pbCopy_MouseUp(object sender, MouseButtonEventArgs e)
        {
            pbCopy.Source = new BitmapImage(new Uri("pack://application:,,,/copy-icon.jpg"));
        }

        private void btnFunction_Click(object sender, RoutedEventArgs e)
        {
            if(pwSecretKey.Password == null || pwSecretKey.Password=="")
            {
                MessageBox.Show("You must enter a password.");
            }
            else if(selectedFilePaths==null || selectedFilePaths.Length<1)
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
            }
        }
    }
}