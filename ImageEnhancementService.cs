using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageToolkit
{
    /// <summary>
    /// Professional image enhancement service using traditional image processing techniques
    /// </summary>
    public class ImageEnhancementService
    {
        private const int MAX_DIMENSION = 8192; // Maximum output dimension to prevent memory issues

        /// <summary>
        /// Enhances an image using a high-fidelity professional pipeline
        /// </summary>
        public async Task<BitmapSource?> EnhanceImageAsync(BitmapSource source, EnhancementSettings settings)
        {
            return await Task.Run(() =>
            {
                try
                {
                    BitmapSource result = source;

                    // Step 1: Intelligent Noise Reduction
                    if (settings.NoiseReduction > 0)
                    {
                        // Use Median Filter instead of Gaussian for edge preservation
                        result = ApplyMedianFilter(result, (int)Math.Ceiling(settings.NoiseReduction / 2.0));
                    }

                    // Step 2: High-Fidelity Upscale
                    if (settings.UpscaleFactor > 1)
                    {
                        int newWidth = result.PixelWidth * settings.UpscaleFactor;
                        int newHeight = result.PixelHeight * settings.UpscaleFactor;

                        if (newWidth > MAX_DIMENSION || newHeight > MAX_DIMENSION)
                        {
                            throw new InvalidOperationException($"Output exceeds limit ({MAX_DIMENSION}x{MAX_DIMENSION})");
                        }

                        // Use Refined Lanczos-3 with sub-pixel precision
                        result = UpscaleProfessional(result, newWidth, newHeight);
                    }

                    // Step 3: Natural Contrast Adjustment (S-Curve)
                    if (settings.Contrast != 0)
                    {
                        result = ApplyNaturalContrast(result, settings.Contrast);
                    }

                    // Step 4: Detail Recovery (Professional Sharpening)
                    if (settings.Sharpness > 0)
                    {
                        result = ApplySmartSharpen(result, settings.Sharpness);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Professional Enhancement Error: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Professional upscale using corrected sub-pixel Lanczos-3 resampling
        /// </summary>
        private BitmapSource UpscaleProfessional(BitmapSource source, int newWidth, int newHeight)
        {
            var pixels = GetPixelData(source);
            var result = new byte[newWidth * newHeight * 4];

            double xRatio = (double)(source.PixelWidth - 1) / (newWidth - 1);
            double yRatio = (double)(source.PixelHeight - 1) / (newHeight - 1);

            const int a = 3; // Lanczos radius

            Parallel.For(0, newHeight, y =>
            {
                double srcY = y * yRatio;
                int yMin = (int)Math.Floor(srcY) - a + 1;
                int yMax = (int)Math.Floor(srcY) + a;

                for (int x = 0; x < newWidth; x++)
                {
                    double srcX = x * xRatio;
                    int xMin = (int)Math.Floor(srcX) - a + 1;
                    int xMax = (int)Math.Floor(srcX) + a;

                    double r = 0, g = 0, b = 0, alpha = 0;
                    double totalWeight = 0;

                    for (int py = yMin; py <= yMax; py++)
                    {
                        if (py < 0 || py >= source.PixelHeight) continue;
                        double weightY = LanczosKernel(srcY - py);
                        
                        for (int px = xMin; px <= xMax; px++)
                        {
                            if (px < 0 || px >= source.PixelWidth) continue;
                            
                            double weightX = LanczosKernel(srcX - px);
                            double weight = weightX * weightY;

                            if (weight == 0) continue;

                            int srcIndex = (py * source.PixelWidth + px) * 4;
                            
                            // Color math in semi-linear space to prevent darkening
                            b += Math.Pow(pixels[srcIndex], 2.2) * weight;
                            g += Math.Pow(pixels[srcIndex + 1], 2.2) * weight;
                            r += Math.Pow(pixels[srcIndex + 2], 2.2) * weight;
                            alpha += pixels[srcIndex + 3] * weight;
                            
                            totalWeight += weight;
                        }
                    }

                    int destIndex = (y * newWidth + x) * 4;
                    if (totalWeight > 0)
                    {
                        result[destIndex] = ClampByte(Math.Pow(b / totalWeight, 1.0 / 2.2));
                        result[destIndex + 1] = ClampByte(Math.Pow(g / totalWeight, 1.0 / 2.2));
                        result[destIndex + 2] = ClampByte(Math.Pow(r / totalWeight, 1.0 / 2.2));
                        result[destIndex + 3] = ClampByte(alpha / totalWeight);
                    }
                    else
                    {
                        // Fallback to nearest neighbor if weights fail
                        int fallbackIndex = ((int)Math.Round(srcY) * source.PixelWidth + (int)Math.Round(srcX)) * 4;
                        for (int c = 0; c < 4; c++) result[destIndex + c] = pixels[fallbackIndex + c];
                    }
                }
            });

            return CreateBitmapSource(result, newWidth, newHeight);
        }

        private double LanczosKernel(double x)
        {
            x = Math.Abs(x);
            if (x < 1e-9) return 1.0;
            if (x >= 3.0) return 0.0;
            
            double pX = Math.PI * x;
            return 3.0 * Math.Sin(pX) * Math.Sin(pX / 3.0) / (pX * pX);
        }

        /// <summary>
        /// Median Filter for professional noise reduction without blurring edges
        /// </summary>
        private BitmapSource ApplyMedianFilter(BitmapSource source, int radius)
        {
            if (radius < 1) return source;
            
            var pixels = GetPixelData(source);
            var result = new byte[pixels.Length];
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            Parallel.For(0, height, y =>
            {
                byte[] rList = new byte[(2 * radius + 1) * (2 * radius + 1)];
                byte[] gList = new byte[rList.Length];
                byte[] bList = new byte[rList.Length];

                for (int x = 0; x < width; x++)
                {
                    int count = 0;
                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        int py = Math.Clamp(y + ky, 0, height - 1);
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int px = Math.Clamp(x + kx, 0, width - 1);
                            int index = (py * width + px) * 4;
                            bList[count] = pixels[index];
                            gList[count] = pixels[index + 1];
                            rList[count] = pixels[index + 2];
                            count++;
                        }
                    }

                    Array.Sort(bList, 0, count);
                    Array.Sort(gList, 0, count);
                    Array.Sort(rList, 0, count);

                    int destIndex = (y * width + x) * 4;
                    result[destIndex] = bList[count / 2];
                    result[destIndex + 1] = gList[count / 2];
                    result[destIndex + 2] = rList[count / 2];
                    result[destIndex + 3] = pixels[destIndex + 3];
                }
            });

            return CreateBitmapSource(result, width, height);
        }

        /// <summary>
        /// Natural Sigmoid-based Contrast adjustment
        /// </summary>
        private BitmapSource ApplyNaturalContrast(BitmapSource source, double contrast)
        {
            var pixels = GetPixelData(source);
            var result = new byte[pixels.Length];
            
            // Normalize contrast to a curve factor
            double k = contrast / 50.0; 
            
            Parallel.For(0, pixels.Length / 4, i =>
            {
                int index = i * 4;
                for (int c = 0; c < 3; c++)
                {
                    double v = pixels[index + c] / 255.0;
                    // S-Curve: sigmoid(v)
                    double adjusted = 1.0 / (1.0 + Math.Exp(-k * (v - 0.5) * 10.0));
                    result[index + c] = ClampByte(adjusted * 255.0);
                }
                result[index + 3] = pixels[index + 3];
            });

            return CreateBitmapSource(result, source.PixelWidth, source.PixelHeight);
        }

        /// <summary>
        /// Professional Smart Sharpen (Modified Unsharp Mask)
        /// </summary>
        private BitmapSource ApplySmartSharpen(BitmapSource source, double strength)
        {
            // First, get a highly blurred version for the mask
            var blurred = ApplyGaussianBlur(source, 0.8);
            
            var original = GetPixelData(source);
            var blurData = GetPixelData(blurred);
            var result = new byte[original.Length];

            double amount = strength / 100.0 * 1.5;

            Parallel.For(0, original.Length / 4, i =>
            {
                int index = i * 4;
                for (int c = 0; c < 3; c++)
                {
                    int o = original[index + c];
                    int b = blurData[index + c];
                    
                    // Detail extraction
                    double detail = o - b;
                    
                    // Only sharpen details above a tiny threshold to avoid noise amplification
                    if (Math.Abs(detail) < 2) detail = 0;

                    result[index + c] = ClampByte(o + detail * amount);
                }
                result[index + 3] = original[index + 3];
            });

            return CreateBitmapSource(result, source.PixelWidth, source.PixelHeight);
        }

        private BitmapSource ApplyGaussianBlur(BitmapSource source, double sigma)
        {
            var pixels = GetPixelData(source);
            var result = new byte[pixels.Length];
            int width = source.PixelWidth;
            int height = source.PixelHeight;

            int kernelSize = (int)(sigma * 3) * 2 + 1;
            double[] kernel = GenerateGaussianKernel(kernelSize, sigma);
            int radius = kernelSize / 2;

            var temp = new byte[pixels.Length];
            
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double b=0, g=0, r=0, a=0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int px = Math.Clamp(x + k, 0, width - 1);
                        int idx = (y * width + px) * 4;
                        double w = kernel[k + radius];
                        b += pixels[idx] * w;
                        g += pixels[idx+1] * w;
                        r += pixels[idx+2] * w;
                        a += pixels[idx+3] * w;
                    }
                    int dest = (y * width + x) * 4;
                    temp[dest] = ClampByte(b);
                    temp[dest+1] = ClampByte(g);
                    temp[dest+2] = ClampByte(r);
                    temp[dest+3] = ClampByte(a);
                }
            });

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    double b=0, g=0, r=0, a=0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int py = Math.Clamp(y + k, 0, height - 1);
                        int idx = (py * width + x) * 4;
                        double w = kernel[k + radius];
                        b += temp[idx] * w;
                        g += temp[idx+1] * w;
                        r += temp[idx+2] * w;
                        a += temp[idx+3] * w;
                    }
                    int dest = (y * width + x) * 4;
                    result[dest] = ClampByte(b);
                    result[dest+1] = ClampByte(g);
                    result[dest+2] = ClampByte(r);
                    result[dest+3] = ClampByte(a);
                }
            });

            return CreateBitmapSource(result, width, height);
        }

        /// <summary>
        /// Generates a Gaussian kernel
        /// </summary>
        private double[] GenerateGaussianKernel(int size, double sigma)
        {
            double[] kernel = new double[size];
            double sum = 0;
            int radius = size / 2;

            for (int i = 0; i < size; i++)
            {
                int x = i - radius;
                kernel[i] = Math.Exp(-(x * x) / (2 * sigma * sigma));
                sum += kernel[i];
            }

            // Normalize
            for (int i = 0; i < size; i++)
            {
                kernel[i] /= sum;
            }

            return kernel;
        }

        /// <summary>
        /// Extracts pixel data from BitmapSource
        /// </summary>
        private byte[] GetPixelData(BitmapSource source)
        {
            int stride = source.PixelWidth * 4;
            byte[] pixels = new byte[source.PixelHeight * stride];

            source.CopyPixels(pixels, stride, 0);

            return pixels;
        }

        /// <summary>
        /// Creates BitmapSource from pixel data
        /// </summary>
        private BitmapSource CreateBitmapSource(byte[] pixels, int width, int height)
        {
            var bitmap = BitmapSource.Create(
                width, height,
                96, 96,
                PixelFormats.Bgra32,
                null,
                pixels,
                width * 4);

            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Clamps value to byte range
        /// </summary>
        private byte ClampByte(double value)
        {
            return (byte)Math.Clamp(value, 0, 255);
        }
    }

    /// <summary>
    /// Settings for image enhancement
    /// </summary>
    public class EnhancementSettings
    {
        public double Sharpness { get; set; } = 0;        // 0-100
        public double Contrast { get; set; } = 0;         // -100 to +100
        public double NoiseReduction { get; set; } = 0;   // 0-10
        public int UpscaleFactor { get; set; } = 1;       // 1, 2, or 3
    }
}
