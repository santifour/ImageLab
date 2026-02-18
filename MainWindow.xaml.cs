using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageToolkit
{
    public partial class MainWindow : Window
    {
        private readonly ImageService _imageService;
        private readonly FileService _fileService;
        private readonly ImageEnhancementService _enhancementService;
        private readonly LocalizationService _localization;
        private string? _currentImagePath;
        private BitmapSource? _currentImage;
        private BitmapSource? _enhancedImage;

        public MainWindow()
        {
            InitializeComponent();
            _imageService = new ImageService();
            _fileService = new FileService();
            _enhancementService = new ImageEnhancementService();
            _localization = new LocalizationService();
            
            UpdateUI();
        }

        #region Title Bar Events
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        
        private void MaximizeButton_Click(object sender, RoutedEventArgs e) 
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            _localization.SwitchLanguage();
            UpdateUI();
        }

        #endregion

        #region Localization

        private void UpdateUI()
        {
            // Title Bar
            LanguageButton.Content = _localization.IsTurkish ? "EN" : "TR";
            LanguageButton.ToolTip = _localization.Get("SwitchLanguage");

            // Headers
            PreviewHeaderText.Text = _localization.IsTurkish ? "Resim Önizleme" : "Image Preview";

            // File Section
            FileGroupBox.Header = _localization.Get("FileSection");
            SelectImageButton.Content = _localization.Get("SelectImage");
            ClearImageButton.Content = _localization.Get("Clear");
            PlaceholderText.Text = _localization.Get("DropImageHere");

            // Image Info
            ImageInfoGroupBox.Header = _localization.Get("ImageInfo");
            UpdateImageInfo();

            // Resize
            ResizeGroupBox.Header = _localization.Get("ResizeSection");
            WidthLabel.Text = _localization.Get("Width");
            HeightLabel.Text = _localization.Get("Height");
            KeepAspectRatioCheckBox.Content = _localization.Get("KeepAspectRatio");
            ResizeButton.Content = _localization.Get("ResizeAndSave");

            // Compression
            CompressionGroupBox.Header = _localization.Get("CompressionSection");
            QualityLabel.Text = _localization.Get("Quality");
            UpdateEstimatedSize();
            CompressButton.Content = _localization.Get("CompressAndSave");

            // Format Conversion
            FormatGroupBox.Header = _localization.Get("FormatSection");
            TargetFormatLabel.Text = _localization.Get("TargetFormat");
            ConvertButton.Content = _localization.Get("ConvertAndSave");

            // Enhancement
            EnhancementGroupBox.Header = _localization.Get("EnhancementSection");
            SharpnessLabel.Text = _localization.Get("Sharpness");
            ContrastLabel.Text = _localization.Get("Contrast");
            NoiseReductionLabel.Text = _localization.Get("NoiseReduction");
            UpscaleLabel.Text = _localization.Get("UpscaleFactor");
            PreviewEnhancementButton.Content = _localization.Get("Preview");
            ResetEnhancementButton.Content = _localization.Get("Reset");
            SaveEnhancementButton.Content = _localization.Get("SaveEnhanced");

            // Status Bar
            StatusText.Text = _currentImage == null ? _localization.Get("NoImageLoaded") : _localization.Get("Ready");
        }

        #endregion

        #region File Operations

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Resim Dosyaları|*.png;*.jpg;*.jpeg;*.bmp|Tüm Dosyalar|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImage(openFileDialog.FileName);
            }
        }

        private void LoadImage(string path)
        {
            try
            {
                var imageData = _imageService.LoadImage(path);
                if (imageData == null)
                {
                    MessageBox.Show(_localization.Get("LoadError"), _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _currentImage = imageData.BitmapImage;
                _currentImagePath = path;
                _enhancedImage = null;

                ImagePreview.Source = _currentImage;
                ImagePreview.Visibility = Visibility.Visible;
                PlaceholderText.Visibility = Visibility.Collapsed;

                ClearImageButton.IsEnabled = true;
                ResizeButton.IsEnabled = true;
                CompressButton.IsEnabled = true;
                ConvertButton.IsEnabled = true;
                PreviewEnhancementButton.IsEnabled = true;
                ResetEnhancementButton.IsEnabled = true;
                SaveEnhancementButton.IsEnabled = true;

                UpdateImageInfo();
                UpdateEstimatedSize();
                
                StatusText.Text = _localization.Get("Ready");
                StatusInfoText.Text = Path.GetFileName(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_localization.Get("LoadErrorMsg")} {ex.Message}", _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearImageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentImage = null;
            _currentImagePath = null;
            _enhancedImage = null;

            ImagePreview.Source = null;
            ImagePreview.Visibility = Visibility.Collapsed;
            PlaceholderText.Visibility = Visibility.Visible;

            ClearImageButton.IsEnabled = false;
            ResizeButton.IsEnabled = false;
            CompressButton.IsEnabled = false;
            ConvertButton.IsEnabled = false;
            PreviewEnhancementButton.IsEnabled = false;
            ResetEnhancementButton.IsEnabled = false;
            SaveEnhancementButton.IsEnabled = false;

            UpdateImageInfo();
            UpdateEstimatedSize();
            StatusText.Text = _localization.Get("NoImageLoaded");
            StatusInfoText.Text = "";
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string ext = Path.GetExtension(files[0]).ToLower();
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp")
                    {
                        LoadImage(files[0]);
                    }
                    else
                    {
                        MessageBox.Show(_localization.Get("InvalidFile"), _localization.Get("InvalidFileTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        #endregion

        #region Image Information

        private void UpdateImageInfo()
        {
            if (_currentImagePath != null && _currentImage != null)
            {
                FileInfo fi = new FileInfo(_currentImagePath);
                FileNameText.Text = $"{_localization.Get("File")}: {fi.Name}";
                ResolutionText.Text = $"{_localization.Get("Resolution")}: {_currentImage.PixelWidth} x {_currentImage.PixelHeight}";
                FileSizeText.Text = $"{_localization.Get("Size")}: {_fileService.FormatFileSize(fi.Length)}";
                
                ResizeWidthTextBox.Text = _currentImage.PixelWidth.ToString();
                ResizeHeightTextBox.Text = _currentImage.PixelHeight.ToString();
            }
            else
            {
                FileNameText.Text = $"{_localization.Get("File")}: -";
                ResolutionText.Text = $"{_localization.Get("Resolution")}: -";
                FileSizeText.Text = $"{_localization.Get("Size")}: -";
            }
        }

        private void ResizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null || _currentImagePath == null) return;

            if (!int.TryParse(ResizeWidthTextBox.Text, out int width) || width <= 0)
            {
                MessageBox.Show(_localization.Get("InvalidWidth"), _localization.Get("InvalidValue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(ResizeHeightTextBox.Text, out int height) || height <= 0)
            {
                MessageBox.Show(_localization.Get("InvalidHeight"), _localization.Get("InvalidValue"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _imageService.SaveResizedImage(_currentImage, _currentImagePath, width, height, KeepAspectRatioCheckBox.IsChecked ?? true);
            if (success)
            {
                MessageBox.Show(_localization.Get("ResizeSuccess"), _localization.Get("SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_localization.Get("SaveError"), _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (QualityValueText != null)
                QualityValueText.Text = ((int)e.NewValue).ToString();
            
            UpdateEstimatedSize();
        }

        private void UpdateEstimatedSize()
        {
            if (EstimatedSizeText == null) return;

            if (_currentImage != null)
            {
                int quality = (int)QualitySlider.Value;
                long estimatedSize = _imageService.EstimateCompressedSize(_currentImage, quality);
                EstimatedSizeText.Text = $"{_localization.Get("EstimatedSize")}: ~{_fileService.FormatFileSize(estimatedSize)}";
            }
            else
            {
                EstimatedSizeText.Text = $"{_localization.Get("EstimatedSize")}: -";
            }
        }

        private void CompressButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null || _currentImagePath == null) return;

            int quality = (int)QualitySlider.Value;
            bool success = _imageService.CompressJpgImage(_currentImage, _currentImagePath, quality);
            if (success)
            {
                MessageBox.Show(_localization.Get("CompressSuccess"), _localization.Get("SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_localization.Get("SaveError"), _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null || _currentImagePath == null) return;

            ComboBoxItem selectedItem = (ComboBoxItem)FormatComboBox.SelectedItem;
            string extension = selectedItem.Tag.ToString()!;
            string formatName = selectedItem.Content.ToString()!;

            bool success = _imageService.ConvertImageFormat(_currentImage, _currentImagePath, extension);
            if (success)
            {
                MessageBox.Show(string.Format(_localization.Get("ConvertSuccess"), formatName), _localization.Get("SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(_localization.Get("SaveError"), _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Enhancement Events

        private async void PreviewEnhancementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null) return;

            try
            {
                EnhancementStatusText.Text = _localization.Get("Processing");
                PreviewEnhancementButton.IsEnabled = false;

                var settings = new EnhancementSettings
                {
                    Sharpness = SharpnessSlider.Value,
                    Contrast = ContrastSlider.Value,
                    NoiseReduction = NoiseReductionSlider.Value,
                    UpscaleFactor = int.Parse(((ComboBoxItem)UpscaleComboBox.SelectedItem).Tag.ToString()!)
                };

                _enhancedImage = await _enhancementService.EnhanceImageAsync(_currentImage, settings);

                if (_enhancedImage != null)
                {
                    ImagePreview.Source = _enhancedImage;
                    EnhancementStatusText.Text = _localization.Get("PreviewInfo");
                }
                else
                {
                    MessageBox.Show(_localization.Get("EnhancementFailed"), _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                    EnhancementStatusText.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_localization.Get("EnhancementError")} {ex.Message}", _localization.Get("ErrorOccurred"), MessageBoxButton.OK, MessageBoxImage.Error);
                EnhancementStatusText.Text = "";
            }
            finally
            {
                PreviewEnhancementButton.IsEnabled = true;
            }
        }

        private void ResetEnhancementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null) return;

            SharpnessSlider.Value = 50;
            ContrastSlider.Value = 0;
            NoiseReductionSlider.Value = 2;
            UpscaleComboBox.SelectedIndex = 0;
            
            _enhancedImage = null;
            ImagePreview.Source = _currentImage;
            EnhancementStatusText.Text = "";
        }

        private void SaveEnhancementButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentImage == null || _currentImagePath == null) return;
            BitmapSource imageToSave = _enhancedImage ?? _currentImage;
            
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Dosyası|*.png|JPG Dosyası|*.jpg",
                FileName = "enhanced_" + Path.GetFileName(_currentImagePath)
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // For enhanced image, we use direct save via ImageService
                    // Need to check if ConvertImageFormat can take a target full path
                    // Actually, let's just use ConvertAndSave if I had it.
                    // Wait, I will use a simple implementation here or add it to ImageService.
                    
                    _imageService.ConvertImageFormat(imageToSave, saveFileDialog.FileName, Path.GetExtension(saveFileDialog.FileName));
                    MessageBox.Show(string.Format(_localization.Get("EnhancedSaved"), Path.GetFileName(saveFileDialog.FileName)), _localization.Get("SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{_localization.Get("SaveErrorMsg")} {ex.Message}", _localization.Get("ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SharpnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SharpnessValueText != null) SharpnessValueText.Text = ((int)e.NewValue).ToString();
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ContrastValueText != null) ContrastValueText.Text = ((int)e.NewValue).ToString();
        }

        private void NoiseReductionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (NoiseReductionValueText != null) NoiseReductionValueText.Text = ((int)e.NewValue).ToString();
        }

        #endregion
    }
}
