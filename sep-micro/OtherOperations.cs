using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace sep_micro
{
    class OtherOperations
    {
        public static void WipeFile(string file, int passes)
        {
            try
            {
                if (File.Exists(file))
                {
                    // Set the file's attributes to normal in case it's read-only.
                    File.SetAttributes(file, FileAttributes.Normal);

                    // Calculate the total number of sectors in the file.
                    double sectors = Math.Ceiling(new FileInfo(file).Length / 512.0);

                    // Create a dummy buffer the same size as the sector.
                    byte[] dummyBuffer = new byte[512];

                    // Create a cryptographic Random Number Generator.
                    // This is what we'll be filling the file with.
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                    // Open a FileStream to the file.
                    FileStream inputStream = new FileStream(file, FileMode.Open);
                    for (int currentPass = 0; currentPass < passes; currentPass++)
                    {
                        // Go to the beginning of the stream
                        inputStream.Position = 0;

                        // Loop all sectors
                        for (int sectorsWritten = 0; sectorsWritten < sectors; sectorsWritten++)
                        {
                            // Fill the dummy buffer with random data
                            rng.GetBytes(dummyBuffer);

                            // Write it to the stream
                            inputStream.Write(dummyBuffer, 0, dummyBuffer.Length);
                        }
                    }

                    // Close the stream.
                    inputStream.Close();

                    // As an extra measure, let's calculate a random filename to rename our file to, before deleting it.
                    string newName = Path.GetDirectoryName(file) + "\\" + Path.GetRandomFileName();
                    File.Move(file, newName);

                    File.Delete(newName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: {0}", ex.Message);
                MessageBox.Show($"Sorry, these files could not be deleted. Please try again.\r\nMore Details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
