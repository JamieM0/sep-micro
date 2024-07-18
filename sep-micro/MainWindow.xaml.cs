using Microsoft.Win32;
using System.Text;
using System;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.IO.Compression;

namespace sep_micro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool encryptMode = true;
        string[] selectedFilePaths;
        string[] selectedFileNames;
        bool usingKeyfile = false;
        string keyfilePath = "";
        string zipFilePath = "";
        bool encryptFolderMode = false;
        string folderSelected = "";

        public MainWindow()
        {
            InitializeComponent();
        }
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

            ConditionChecker();
        }

        private void btnFunction_Click(object sender, RoutedEventArgs e)
        {
            if (selectedFilePaths == null || selectedFilePaths.Length < 1)
            {
                MessageBox.Show("You must select at least 1 file to encrypt or decrypt.");
            }
            else if ((pwSecretKey.Password == null || pwSecretKey.Password == "") && !usingKeyfile)
            {
                MessageBox.Show("You must enter a password or use a keyfile.");
            }
            else
            {
                pwSecretKey.IsEnabled = false;
                string password = pwSecretKey.Password;
                folderSelected = selectedFilePaths[0];
                if (usingKeyfile)
                {
                    password += OtherOperations.CalculateSHA256(keyfilePath);
                }
                
                if(encryptFolderMode==true)
                {
                    selectedFilePaths = Directory.GetFiles(selectedFilePaths[0]);
                    selectedFileNames = new string[Directory.GetFiles(folderSelected).Length];
                }

                if(encryptMode && cbCombineFiles.IsChecked==true)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = "Multiple Files.zip";
                    saveFileDialog.Filter = "All files (*.*)|*.*";
                    saveFileDialog.RestoreDirectory = true;

                    bool? result = saveFileDialog.ShowDialog();
                    if (result == true)
                    {
                        zipFilePath = saveFileDialog.FileName;
                        try
                        {
                            using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                            {
                                foreach (string filePath in selectedFilePaths)
                                {
                                    string fileName = Path.GetFileName(filePath);
                                    archive.CreateEntryFromFile(filePath, fileName);
                                }
                            }
                            AES.EncryptFile(zipFilePath, zipFilePath + ".aes", password);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else if (encryptMode)
                {
                    foreach (string file in selectedFilePaths)
                    {
                        AES.EncryptFile(file, file + ".aes", password);
                    }
                    MessageBox.Show($"{selectedFilePaths.Length} files encrypted.", "SEP");
                }
                else
                {
                    foreach (string file in selectedFilePaths)
                    {
                        AES.DecryptFile(file, file.Substring(0, file.Length - 4), password);
                    }
                    MessageBox.Show($"{selectedFilePaths.Length} files decrypted.", "SEP");
                }

                if (cbDeleteFiles.IsChecked == true)
                {
                    foreach (string file in selectedFilePaths)
                    {
                        OtherOperations.WipeFile(file, 3);
                    }
                    if(encryptFolderMode==true)
                    {
                        Directory.Delete(folderSelected);
                    }
                }
                if (cbCombineFiles.IsChecked == true)
                {
                    OtherOperations.WipeFile(zipFilePath, 3);
                }

                ClearOptions();
            }
        }

        private void ClearOptions()
        {
            pwSecretKey.IsEnabled = true;
            pwSecretKey.Password = "";
            lbFileDisplayPreview.Content = "Drag and drop files here";
            selectedFileNames = null;
            selectedFilePaths = null;
            usingKeyfile = false;
            lbKeyfile.Content = "No keyfile selected.";
            keyfilePath = "";
            cbRecursiveEncryption.IsChecked = false;
            cbRecursiveEncryption.Visibility = Visibility.Hidden;
            encryptFolderMode = false;
            btnFunction.Content = "Ready";
            cbCombineFiles.IsChecked = false;
            encryptMode = true;
            usingKeyfile = false;
        }

        private void btnClearFiles_Click(object sender, RoutedEventArgs e)
        {
            ClearOptions();
        }

        private void ConditionChecker()
        {
            try
            {
                if (selectedFilePaths.Length > 1 || encryptFolderMode==true)
                {
                    cbCombineFiles.IsEnabled = true;
                }
                else
                {
                    cbCombineFiles.IsChecked = false;
                    cbCombineFiles.IsEnabled = false;
                }
            }
            catch { }
            //try
            //{
            //    if ()
            //    {

            //    }
            //}
            //catch { }
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {

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

                //If the selectedFilePaths is a folder, then check and disable cbCombineFiles
                if(Directory.Exists(selectedFilePaths[0]) && selectedFilePaths.Length>1)
                {
                    MessageBox.Show("You cannot enter multiple folders or a folder with files.\r\n\r\nPlease either enter 1 folder, or multiple files with no folders.");
                    lbFileDisplayPreview.Content = "Drag and drop files here";
                }
                else if (selectedFilePaths.Length == 0)
                {
                    lbFileDisplayPreview.Content = "Drag and drop files here";
                }
                else if (Directory.Exists(selectedFilePaths[0]))
                {
                    lbFileDisplayPreview.Content = $"{selectedFilePaths[0]}";
                    cbCombineFiles.IsChecked = true;
                    cbRecursiveEncryption.Visibility = Visibility.Visible;
                    encryptFolderMode = true;
                    FileHandler();
                }
                else if (selectedFilePaths.Length == 1)
                {
                    lbFileDisplayPreview.Content = selectedFileNames[0];
                    FileHandler();
                }
                else
                {
                    lbFileDisplayPreview.Content = selectedFilePaths.Length.ToString() + " files selected";
                    FileHandler();
                }
            }
        }

        private void cbCombineFiles_Checked(object sender, RoutedEventArgs e)
        {

        }

        Random rnd = new Random();
        private void btnGeneratePassword_Click(object sender, RoutedEventArgs e)
        {
            //Generate a random password
            pwSecretKey.Password = GeneratePassword(16);
        }

        private void btnCopyPassword_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(pwSecretKey.Password);
        }

        private void SelectKeyfile(string filePath)
        {
            lbKeyfile.Content = Path.GetFileName(filePath);
            usingKeyfile = true;
            keyfilePath = filePath;
        }

        private void GenerateKeyfile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Keyfile";
            saveFileDialog.Filter = "All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, GeneratePassword(128), Encoding.UTF8);
                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    SelectKeyfile(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while saving the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnShowPassword_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void pwVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private string GeneratePassword(int length)
        {
            //Generate a random password
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+=-<?>:{}[]";
            var stringChars = new char[length];

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[rnd.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        private void btnSelectKeyfile_Click(object sender, RoutedEventArgs e)
        {
            //Open file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                SelectKeyfile(openFileDialog.FileName);
            }
        }

        private void cbRecursiveEncryption_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void cbCombineFiles_Unchecked(object sender, RoutedEventArgs e)
        {
            if (encryptFolderMode == true)
            {
                cbCombineFiles.IsChecked = true;
            }
        }
    }
}