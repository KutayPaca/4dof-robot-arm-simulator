// Gerekli kütüphaneleri dahil ediyoruz
using System; // Temel sistem işlevleri için
using OpenTK.Mathematics; // Matematik işlemleri ve vektörler için (Vector2i, Matrix4 vb.)
using OpenTK.Windowing.Common; // Pencere yönetimi için ortak sınıflar
using OpenTK.Windowing.Desktop; // Masaüstü pencere oluşturma ve yönetme için

// Proje isim alanı - tüm sınıflarımızı bu namespace içinde organize ediyoruz
namespace RobotKoluSimulasyonu
{
    /// <summary>
    /// Ana program sınıfı - Uygulamanın başlangıç noktası
    /// </summary>
    class Program
    {
        /// <summary>
        /// Programın giriş noktası - C# uygulaması başladığında ilk çalışan metod
        /// </summary>
        /// <param name="args">Komut satırı argümanları (şu an kullanılmıyor)</param>
        static void Main(string[] args)
        {
            // NativeWindowSettings: OpenGL penceresinin temel özelliklerini tanımlıyoruz
            var nativeWindowSettings = new NativeWindowSettings()
            {
                // Pencere boyutunu 1280x720 piksel olarak ayarlıyoruz
                ClientSize = new Vector2i(1280, 720),
                
                // Pencere başlığı - üst çubukta görünen metin
                Title = "3 DOF Robot Kolu Simülasyonu",
                
                // OpenGL API'sini kullanacağımızı belirtiyoruz (3D grafik oluşturma için)
                API = ContextAPI.OpenGL,
                
                // OpenGL sürümü 3.3 kullanıyoruz - modern özellikler için yeterli
                APIVersion = new Version(3, 3),
                
                // Uyumluluk profili - eski OpenGL komutlarını da kullanabilmemizi sağlar
                Profile = ContextProfile.Compatability,
                
                // Varsayılan bağlam bayrakları - debug veya özel özellik yok
                Flags = ContextFlags.Default,
            };

            // GameWindowSettings: Oyun döngüsü ve güncelleme ayarları
            var gameWindowSettings = new GameWindowSettings()
            {
                // Saniyede 60 kez güncelleme yapılacak (60 FPS hedefi)
                // Bu, OnUpdateFrame metodunun saniyede 60 kez çağrılmasını sağlar
                UpdateFrequency = 60.0
            };

            // RobotWindow sınıfından bir pencere örneği oluşturuyoruz
            // using bloğu, pencere kapatıldığında kaynakların otomatik temizlenmesini sağlar
            using (var window = new RobotWindow(gameWindowSettings, nativeWindowSettings))
            {
                // Pencereyi başlat ve render döngüsünü çalıştır
                // Bu metod, pencere kapatılana kadar devam eden ana döngüyü başlatır
                window.Run();
            }
        }
    }
}
