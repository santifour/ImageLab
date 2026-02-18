using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageToolkit
{
    /// <summary>
    /// Service for handling all image operations
    /// </summary>
    public class ImageService
    {
        /// <summary>
        /// Loads an image from the specified file path
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <returns>ImageData object containing image information, or null if loading fails</returns>
        public ImageData? LoadImage(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                // Get file size
                var fileInfo = new FileInfo(filePath);
                long fileSizeBytes = fileInfo.Length;

                // Load image using BitmapImage (WPF native)
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load entire image into memory
                bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it thread-safe and improve performance

                // Create ImageData object
                return new ImageData
                {
                    BitmapImage = bitmapImage,
                    Width = bitmapImage.PixelWidth,
                    Height = bitmapImage.PixelHeight,
                    FileSizeBytes = fileSizeBytes,
                    FilePath = filePath
                };
            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error loading image: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the dimensions of an image without fully loading it
        /// </summary>
        /// <param name="filePath">Path to the image file</param>
        /// <returns>Tuple containing width and height, or null if failed</returns>
        public (int Width, int Height)? GetImageDimensions(string filePath)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.None;
                bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmapImage.EndInit();

                return (bitmapImage.PixelWidth, bitmapImage.PixelHeight);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Resizes an image to the specified dimensions
        /// </summary>
        /// <param name="sourceImage">Source BitmapImage</param>
        /// <param name="newWidth">Target width</param>
        /// <param name="newHeight">Target height</param>
        /// <param name="keepAspectRatio">Whether to maintain aspect ratio</param>
        /// <returns>Resized BitmapSource</returns>
        public BitmapSource ResizeImage(BitmapSource sourceImage, int newWidth, int newHeight, bool keepAspectRatio)
        {
            try
            {
                int targetWidth = newWidth;
                int targetHeight = newHeight;

                if (keepAspectRatio)
                {
                    double aspectRatio = (double)sourceImage.PixelWidth / sourceImage.PixelHeight;
                    
                    if (newWidth > 0 && newHeight > 0)
                    {
                        double targetAspect = (double)newWidth / newHeight;
                        
                        if (targetAspect > aspectRatio)
                        {
                            targetWidth = (int)(newHeight * aspectRatio);
                            targetHeight = newHeight;
                        }
                        else
                        {
                            targetWidth = newWidth;
                            targetHeight = (int)(newWidth / aspectRatio);
                        }
                    }
                    else if (newWidth > 0)
                    {
                        targetWidth = newWidth;
                        targetHeight = (int)(newWidth / aspectRatio);
                    }
                    else if (newHeight > 0)
                    {
                        targetHeight = newHeight;
                        targetWidth = (int)(newHeight * aspectRatio);
                    }
                }

                // Use high-quality Fant scaling
                var transformedBitmap = new TransformedBitmap(sourceImage,
                    new ScaleTransform(
                        (double)targetWidth / sourceImage.PixelWidth,
                        (double)targetHeight / sourceImage.PixelHeight));

                transformedBitmap.Freeze();
                return transformedBitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resizing image: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves a resized image
        /// </summary>
        public bool SaveResizedImage(BitmapSource image, string originalFilePath, int width, int height, bool keepAspectRatio)
        {
            try
            {
                var resizedImage = ResizeImage(image, width, height, keepAspectRatio);
                
                string directory = Path.GetDirectoryName(originalFilePath) ?? "";
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
                string extension = Path.GetExtension(originalFilePath);
                string newFileName = $"{fileNameWithoutExt}_resized{extension}";
                string outputPath = Path.Combine(directory, newFileName);

                // Make sure filename is unique
                int counter = 1;
                while (File.Exists(outputPath))
                {
                    newFileName = $"{fileNameWithoutExt}_resized_{counter}{extension}";
                    outputPath = Path.Combine(directory, newFileName);
                    counter++;
                }

                return SaveImageToFile(resizedImage, outputPath, extension);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving resized image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Compresses a JPG image with specified quality
        /// </summary>
        public bool CompressJpgImage(BitmapSource image, string originalFilePath, int quality)
        {
            try
            {
                if (quality < 1 || quality > 100)
                    throw new ArgumentException("Quality must be between 1 and 100");

                string directory = Path.GetDirectoryName(originalFilePath) ?? "";
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
                string newFileName = $"{fileNameWithoutExt}_compressed.jpg";
                string outputPath = Path.Combine(directory, newFileName);

                // Make sure filename is unique
                int counter = 1;
                while (File.Exists(outputPath))
                {
                    newFileName = $"{fileNameWithoutExt}_compressed_{counter}.jpg";
                    outputPath = Path.Combine(directory, newFileName);
                    counter++;
                }

                using (var fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    var encoder = new JpegBitmapEncoder
                    {
                        QualityLevel = quality
                    };
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error compressing image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Estimates the file size for a compressed JPG
        /// </summary>
        public long EstimateCompressedSize(BitmapSource image, int quality)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    var encoder = new JpegBitmapEncoder
                    {
                        QualityLevel = quality
                    };
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(memoryStream);
                    return memoryStream.Length;
                }
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Converts image to specified format
        /// </summary>
        public bool ConvertImageFormat(BitmapSource image, string originalFilePath, string targetFormat)
        {
            try
            {
                string directory = Path.GetDirectoryName(originalFilePath) ?? "";
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFilePath);
                string extension = targetFormat.ToLowerInvariant();
                
                BitmapSource imageToSave = image;

                // Special handling for ICO - resize to 256x256
                if (extension == ".ico")
                {
                    imageToSave = ResizeImage(image, 256, 256, false);
                }

                string newFileName = $"{fileNameWithoutExt}_converted{extension}";
                string outputPath = Path.Combine(directory, newFileName);

                // Make sure filename is unique
                int counter = 1;
                while (File.Exists(outputPath))
                {
                    newFileName = $"{fileNameWithoutExt}_converted_{counter}{extension}";
                    outputPath = Path.Combine(directory, newFileName);
                    counter++;
                }

                return SaveImageToFile(imageToSave, outputPath, extension);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves a BitmapSource to file with the specified format
        /// </summary>
        private bool SaveImageToFile(BitmapSource image, string filePath, string extension)
        {
            try
            {
                BitmapEncoder encoder = extension.ToLowerInvariant() switch
                {
                    ".png" => new PngBitmapEncoder(),
                    ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 95 },
                    ".bmp" => new BmpBitmapEncoder(),
                    ".ico" => new PngBitmapEncoder(), // ICO uses PNG internally
                    _ => new PngBitmapEncoder()
                };

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                // For ICO, we need to do additional processing
                if (extension.ToLowerInvariant() == ".ico")
                {
                    ConvertPngToIco(filePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving image: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Converts a PNG file to ICO format
        /// </summary>
        private void ConvertPngToIco(string pngPath)
        {
            try
            {
                string icoPath = Path.ChangeExtension(pngPath, ".ico");
                
                // Load the PNG
                var bitmap = new BitmapImage(new Uri(pngPath));
                
                // Create ICO encoder
                using (var fileStream = new FileStream(icoPath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }

                // Delete temporary PNG
                if (File.Exists(pngPath))
                    File.Delete(pngPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting to ICO: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Data class to hold image information
    /// </summary>
    public class ImageData
    {
        public BitmapImage BitmapImage { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public long FileSizeBytes { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }
}
