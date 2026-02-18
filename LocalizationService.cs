using System.Collections.Generic;

namespace ImageToolkit
{
    /// <summary>
    /// Localization service for multi-language support
    /// </summary>
    public class LocalizationService
    {
        private Dictionary<string, string> _currentLanguage;
        private readonly Dictionary<string, string> _turkish;
        private readonly Dictionary<string, string> _english;
        
        public string CurrentLanguageCode { get; private set; }

        public LocalizationService()
        {
            _turkish = new Dictionary<string, string>
            {
                // Window
                ["WindowTitle"] = "ImageLab",
                
                // File Section
                ["FileSection"] = "Dosya",
                ["SelectImage"] = "Resim Seç",
                ["Clear"] = "Temizle",
                ["DropImageHere"] = "Buraya bir resim sürükleyin veya 'Resim Seç' butonunu kullanın",
                
                // Image Info
                ["ImageInfo"] = "Resim Bilgisi",
                ["File"] = "Dosya",
                ["Resolution"] = "Çözünürlük",
                ["Size"] = "Boyut",
                
                // Resize
                ["ResizeSection"] = "Yeniden Boyutlandır",
                ["Width"] = "Genişlik:",
                ["Height"] = "Yükseklik:",
                ["KeepAspectRatio"] = "En-boy oranını koru",
                ["ResizeAndSave"] = "Yeniden Boyutlandır ve Kaydet",
                
                // Compression
                ["CompressionSection"] = "Sıkıştırma (Sadece JPG)",
                ["Quality"] = "Kalite:",
                ["EstimatedSize"] = "Tahmini boyut",
                ["CompressAndSave"] = "Sıkıştır ve Kaydet",
                
                // Format Conversion
                ["FormatSection"] = "Format Dönüştürme",
                ["TargetFormat"] = "Hedef format:",
                ["ConvertAndSave"] = "Dönüştür ve Kaydet",
                
                // Enhancement
                ["EnhancementSection"] = "Profesyonel İyileştirme",
                ["Sharpness"] = "Keskinlik:",
                ["Contrast"] = "Kontrast:",
                ["NoiseReduction"] = "Gürültü Azaltma:",
                ["UpscaleFactor"] = "Büyütme faktörü:",
                ["Preview"] = "Önizleme",
                ["Reset"] = "Sıfırla",
                ["SaveEnhanced"] = "İyileştirilmiş Resmi Kaydet",
                ["Processing"] = "İşleniyor...",
                ["PreviewInfo"] = "Önizleme",
                ["Saved"] = "Kaydedildi!",
                
                // Status Bar
                ["Ready"] = "Hazır",
                ["NoImageLoaded"] = "Resim yüklenmedi",
                
                // Messages
                ["InvalidFile"] = "Geçersiz resim dosyası. Lütfen PNG, JPG, JPEG veya BMP dosyası seçin.",
                ["InvalidFileTitle"] = "Geçersiz Dosya",
                ["LoadError"] = "Resim yüklenemedi. Dosya bozuk olabilir.",
                ["ErrorTitle"] = "Hata",
                ["InvalidWidth"] = "Lütfen geçerli bir genişlik değeri girin.",
                ["InvalidHeight"] = "Lütfen geçerli bir yükseklik değeri girin.",
                ["InvalidValue"] = "Geçersiz Değer",
                ["ResizeSuccess"] = "Resim başarıyla yeniden boyutlandırıldı ve kaydedildi!",
                ["CompressSuccess"] = "Resim başarıyla sıkıştırıldı ve kaydedildi!",
                ["ConvertSuccess"] = "Resim başarıyla {0} formatına dönüştürüldü!",
                ["SaveError"] = "Resim kaydedilemedi.",
                ["SuccessTitle"] = "Başarılı",
                ["EnhancementError"] = "İyileştirme sırasında hata:",
                ["EnhancementFailed"] = "İyileştirme başarısız oldu.",
                ["ErrorOccurred"] = "Hata oluştu.",
                ["EnhancedSaved"] = "İyileştirilmiş resim kaydedildi:\n{0}",
                ["SaveErrorMsg"] = "Kaydetme hatası:",
                ["LoadErrorMsg"] = "Resim yüklenirken bir hata oluştu:",
                ["GeneralError"] = "Bir hata oluştu:",
                
                // Language
                ["Language"] = "Dil: Türkçe",
                ["SwitchLanguage"] = "Switch to English"
            };

            _english = new Dictionary<string, string>
            {
                // Window
                ["WindowTitle"] = "ImageLab",
                
                // File Section
                ["FileSection"] = "File",
                ["SelectImage"] = "Select Image",
                ["Clear"] = "Clear",
                ["DropImageHere"] = "Drop an image here or use 'Select Image' button",
                
                // Image Info
                ["ImageInfo"] = "Image Information",
                ["File"] = "File",
                ["Resolution"] = "Resolution",
                ["Size"] = "Size",
                
                // Resize
                ["ResizeSection"] = "Resize",
                ["Width"] = "Width:",
                ["Height"] = "Height:",
                ["KeepAspectRatio"] = "Keep aspect ratio",
                ["ResizeAndSave"] = "Resize and Save",
                
                // Compression
                ["CompressionSection"] = "Compression (JPG Only)",
                ["Quality"] = "Quality:",
                ["EstimatedSize"] = "Estimated size",
                ["CompressAndSave"] = "Compress and Save",
                
                // Format Conversion
                ["FormatSection"] = "Format Conversion",
                ["TargetFormat"] = "Target format:",
                ["ConvertAndSave"] = "Convert and Save",
                
                // Enhancement
                ["EnhancementSection"] = "Professional Enhancement",
                ["Sharpness"] = "Sharpness:",
                ["Contrast"] = "Contrast:",
                ["NoiseReduction"] = "Noise Reduction:",
                ["UpscaleFactor"] = "Upscale factor:",
                ["Preview"] = "Preview",
                ["Reset"] = "Reset",
                ["SaveEnhanced"] = "Save Enhanced Image",
                ["Processing"] = "Processing...",
                ["PreviewInfo"] = "Preview",
                ["Saved"] = "Saved!",
                
                // Status Bar
                ["Ready"] = "Ready",
                ["NoImageLoaded"] = "No image loaded",
                
                // Messages
                ["InvalidFile"] = "Invalid image file. Please select a PNG, JPG, JPEG, or BMP file.",
                ["InvalidFileTitle"] = "Invalid File",
                ["LoadError"] = "Failed to load image. The file may be corrupted.",
                ["ErrorTitle"] = "Error",
                ["InvalidWidth"] = "Please enter a valid width value.",
                ["InvalidHeight"] = "Please enter a valid height value.",
                ["InvalidValue"] = "Invalid Value",
                ["ResizeSuccess"] = "Image successfully resized and saved!",
                ["CompressSuccess"] = "Image successfully compressed and saved!",
                ["ConvertSuccess"] = "Image successfully converted to {0} format!",
                ["SaveError"] = "Failed to save image.",
                ["SuccessTitle"] = "Success",
                ["EnhancementError"] = "Enhancement error:",
                ["EnhancementFailed"] = "Enhancement failed.",
                ["ErrorOccurred"] = "Error occurred.",
                ["EnhancedSaved"] = "Enhanced image saved:\n{0}",
                ["SaveErrorMsg"] = "Save error:",
                ["LoadErrorMsg"] = "An error occurred while loading the image:",
                ["GeneralError"] = "An error occurred:",
                
                // Language
                ["Language"] = "Language: English",
                ["SwitchLanguage"] = "Türkçe'ye Geç"
            };

            // Default to Turkish
            CurrentLanguageCode = "tr";
            _currentLanguage = _turkish;
        }

        public string Get(string key)
        {
            return _currentLanguage.TryGetValue(key, out var value) ? value : key;
        }

        public void SwitchLanguage()
        {
            if (CurrentLanguageCode == "tr")
            {
                CurrentLanguageCode = "en";
                _currentLanguage = _english;
            }
            else
            {
                CurrentLanguageCode = "tr";
                _currentLanguage = _turkish;
            }
        }

        public bool IsTurkish => CurrentLanguageCode == "tr";
    }
}
