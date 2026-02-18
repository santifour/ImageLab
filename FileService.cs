using System.IO;

namespace ImageToolkit
{
    /// <summary>
    /// Service for handling file operations (saving, naming, validation)
    /// </summary>
    public class FileService
    {
        private readonly string[] _validExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };

        /// <summary>
        /// Validates if the file is a supported image format
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidImageFile(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return false;

                if (!File.Exists(filePath))
                    return false;

                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                return Array.Exists(_validExtensions, ext => ext == extension);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats file size in bytes to human-readable format (KB/MB)
        /// </summary>
        /// <param name="bytes">File size in bytes</param>
        /// <returns>Formatted string</returns>
        public string FormatFileSize(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;

            if (bytes >= MB)
            {
                double mb = bytes / (double)MB;
                return $"{mb:F2} MB";
            }
            else if (bytes >= KB)
            {
                double kb = bytes / (double)KB;
                return $"{kb:F2} KB";
            }
            else
            {
                return $"{bytes} bytes";
            }
        }

        /// <summary>
        /// Generates a unique filename by appending a number if file exists
        /// </summary>
        /// <param name="directory">Target directory</param>
        /// <param name="fileName">Original filename</param>
        /// <returns>Unique filename</returns>
        public string GenerateUniqueFileName(string directory, string fileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string fullPath = Path.Combine(directory, fileName);

            int counter = 1;
            while (File.Exists(fullPath))
            {
                string newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                fullPath = Path.Combine(directory, newFileName);
                counter++;
            }

            return Path.GetFileName(fullPath);
        }

        /// <summary>
        /// Saves a file to the specified location
        /// </summary>
        /// <param name="sourceFilePath">Source file path</param>
        /// <param name="destinationFilePath">Destination file path</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool SaveFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                // Ensure destination directory exists
                string? directory = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving file: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the file extension from a file path
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File extension (e.g., ".png")</returns>
        public string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant();
        }

        /// <summary>
        /// Gets the file name without extension
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>File name without extension</returns>
        public string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
