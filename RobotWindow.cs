// Gerekli kütüphaneleri dahil ediyoruz
using System; // Temel sistem işlevleri ve matematik operasyonları için
using OpenTK.Graphics.OpenGL; // OpenGL grafik komutları için (GL sınıfı)
using OpenTK.Mathematics; // Vektör, matris ve matematiksel yardımcı fonksiyonlar için
using OpenTK.Windowing.Common; // Pencere olayları (OnLoad, OnResize vb.) için
using OpenTK.Windowing.Desktop; // GameWindow temel sınıfı için
using OpenTK.Windowing.GraphicsLibraryFramework; // Klavye ve fare girişi için (Keys enum)

// Proje isim alanı
namespace RobotKoluSimulasyonu
{
    /// <summary>
    /// Ana pencere sınıfı - Robot kolunun 3D görselleştirmesi ve kontrolünü sağlar
    /// GameWindow sınıfından türetilmiştir ve OpenGL render döngüsünü yönetir
    /// </summary>
    public class RobotWindow : GameWindow
    {
        // ============================================
        // ROBOT EKLEM AÇILARI (Degrees cinsinden)
        // ============================================
        
        /// <summary>
        /// Theta1: Robotun taban eklemi - Y ekseni etrafında 360° dönüş yapabilir
        /// Q tuşu ile saat yönünde, E tuşu ile saat yönünün tersine döner
        /// Sınırsız rotasyon - tam çevrim yapabilir
        /// </summary>
        private float theta1 = 0f;
        
        /// <summary>
        /// Theta2: Omuz eklemi - X ekseni etrafında dönüş
        /// W tuşu ile yukarı (pozitif), S tuşu ile aşağı (negatif) hareket
        /// -90° ile +90° arasında sınırlandırılmıştır (güvenlik için)
        /// </summary>
        private float theta2 = 0f;
        
        /// <summary>
        /// Theta3: Dirsek eklemi - X ekseni etrafında dönüş
        /// A tuşu ile yukarı (pozitif), D tuşu ile aşağı (negatif) hareket
        /// -90° ile +90° arasında sınırlandırılmıştır (güvenlik için)
        /// </summary>
        private float theta3 = 0f;
        
        /// <summary>
        /// Theta4: Bilek/Roll eklemi - Y ekseni etrafında 360° dönüş
        /// R tuşu ile saat yönünde, F tuşu ile saat yönünün tersine döner
        /// 4. Serbestlik derecesi - pençenin kendi ekseni etrafında dönmesini sağlar
        /// </summary>
        private float theta4 = 0f;
        
        // ============================================
        // ROBOT KOL UZUNLUKLARI (Birim cinsinden)
        // ============================================
        
        /// <summary>
        /// L1: İlk link uzunluğu - Taban ile omuz eklemi arası mesafe
        /// Robotun toplam yüksekliğinin temel bileşeni
        /// </summary>
        private const float L1 = 2.0f;
        
        /// <summary>
        /// L2: İkinci link uzunluğu - Omuz ile dirsek eklemi arası mesafe
        /// Robotun uzanma mesafesinin ana bileşeni
        /// </summary>
        private const float L2 = 1.5f;
        
        /// <summary>
        /// L3: Üçüncü link uzunluğu - Dirsek ile uç efektör arası mesafe
        /// Robotun ince hareketler için kullandığı son segment
        /// </summary>
        private const float L3 = 1.0f;
        
        // ============================================
        // PENÇE (GRIPPER) DURUMU
        // ============================================
        
        /// <summary>
        /// Pençenin açık veya kapalı olduğunu belirten bayrak
        /// true = açık (nesne bırakma), false = kapalı (nesne tutma)
        /// X tuşu ile toggle edilir
        /// </summary>
        private bool gripperOpen = false;
        
        /// <summary>
        /// Pençe parmaklarının açılma açısı (derece cinsinden)
        /// 0° = tamamen kapalı, 30° = tamamen açık
        /// Animasyon için yumuşak geçiş (lerp) kullanılır
        /// </summary>
        private float gripperAngle = 0f;
        
        // ============================================
        // UÇ EFEKTÖR KOORDİNATLARI
        // ============================================
        
        /// <summary>
        /// Uç efektörün X ekseni pozisyonu (yatay - sağ/sol)
        /// Model-view matrisinden hesaplanan dünya koordinatı
        /// </summary>
        private float endEffectorX = 0f;
        
        /// <summary>
        /// Uç efektörün Y ekseni pozisyonu (dikey - yukarı/aşağı)
        /// Model-view matrisinden hesaplanan dünya koordinatı
        /// </summary>
        private float endEffectorY = 0f;
        
        /// <summary>
        /// Uç efektörün Z ekseni pozisyonu (derinlik - ileri/geri)
        /// Model-view matrisinden hesaplanan dünya koordinatı
        /// </summary>
        private float endEffectorZ = 0f;
        
        /// <summary>
        /// Uç efektörün orijine (0,0,0) olan toplam uzaklığı
        /// Euclidean Distance formülü ile hesaplanır: sqrt(x² + y² + z²)
        /// Robotun ulaşma mesafesini gösterir
        /// </summary>
        private float totalReach = 0f;
        
        // ============================================
        // KAMERA KONTROL PARAMETRELERİ
        // ============================================
        
        /// <summary>
        /// Kameranın X ekseni etrafındaki rotasyon açısı (derece)
        /// Yukarı/aşağı ok tuşları ile kontrol edilir
        /// Sahneyi yukarıdan veya aşağıdan görüntülemeyi sağlar
        /// </summary>
        private float cameraAngleX = 20f;
        
        /// <summary>
        /// Kameranın Y ekseni etrafındaki rotasyon açısı (derece)
        /// Sol/sağ ok tuşları ile kontrol edilir
        /// Sahnenin etrafında dönmeyi sağlar
        /// </summary>
        private float cameraAngleY = 45f;
        
        /// <summary>
        /// Kameranın sahneye olan uzaklığı (zoom seviyesi)
        /// PageUp ile yaklaşır (5 birim minimum), PageDown ile uzaklaşır (20 birim maksimum)
        /// </summary>
        private float cameraDistance = 10f;

        /// <summary>
        /// RobotWindow yapıcı metodu (Constructor)
        /// Pencere örneği oluşturulduğunda çalışır
        /// </summary>
        /// <param name="gameWindowSettings">Oyun döngüsü ayarları (güncelleme hızı vb.)</param>
        /// <param name="nativeWindowSettings">Pencere özellikleri (boyut, başlık, OpenGL versiyonu vb.)</param>
        public RobotWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) // Üst sınıfın (GameWindow) yapıcısını çağırıyoruz
        {
            // Şu anda constructor boş - tüm başlangıç değerleri yukarıda tanımlandı
            // OpenGL yüklemesi OnLoad() metodunda yapılacak
        }

        /// <summary>
        /// OnLoad: Pencere oluşturulduğunda bir kez çalışan yükleme metodu
        /// OpenGL bağlamı hazır olduğunda çağrılır
        /// Başlangıç ayarları ve OpenGL yapılandırması burada yapılır
        /// </summary>
        protected override void OnLoad()
        {
            // Üst sınıfın OnLoad metodunu çağırıyoruz (zorunlu)
            base.OnLoad();
            
            // Debug için OpenGL bilgilerini konsola yazdırıyoruz
            // Bu bilgiler, sistemde hangi grafik kartı ve sürücüsünün kullanıldığını gösterir
            Console.WriteLine("OpenGL Version: " + GL.GetString(StringName.Version));
            Console.WriteLine("OpenGL Renderer: " + GL.GetString(StringName.Renderer));
            Console.WriteLine("OpenGL Vendor: " + GL.GetString(StringName.Vendor));
            
            // Arka plan rengini ayarlıyoruz - Koyu mavi-gri ton (RGB: 0.1, 0.1, 0.15)
            // Son parametre alpha (saydamlık): 1.0 = tamamen opak
            GL.ClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            
            // Derinlik testini etkinleştir - 3D nesnelerin doğru sırada render edilmesini sağlar
            // Bu olmadan, uzaktaki nesneler yakındaki nesnelerin üzerinde görünebilir
            GL.Enable(EnableCap.DepthTest);
            
            // Başarılı yüklenme mesajı
            Console.WriteLine("OpenGL Yüklendi! Robot görünmelidir.");
        }

        /// <summary>
        /// OnResize: Pencere boyutu değiştirildiğinde otomatik çalışan metod
        /// Viewport'u (render alanını) yeni pencere boyutuna uyarlar
        /// </summary>
        /// <param name="e">Yeni pencere boyutunu içeren olay argümanları</param>
        protected override void OnResize(ResizeEventArgs e)
        {
            // Üst sınıfın OnResize metodunu çağırıyoruz
            base.OnResize(e);
            
            // OpenGL viewport'unu (render alanını) güncelliyoruz
            // Parametreler: (x, y, genişlik, yükseklik)
            // (0, 0) = sol alt köşeden başla, tüm pencereyi kapla
            GL.Viewport(0, 0, e.Width, e.Height);
            
            // Debug amaçlı - yeni boyutları konsola yazdır
            Console.WriteLine($"Pencere boyutu: {e.Width}x{e.Height}");
        }

        /// <summary>
        /// OnUpdateFrame: Her frame'de (kare) bir kez çalışan güncelleme metodu
        /// Kullanıcı girişlerini işler, robot eklemlerini hareket ettirir
        /// Genellikle saniyede 60 kez çağrılır (60 FPS)
        /// </summary>
        /// <param name="args">Frame süresi bilgilerini içerir (delta time)</param>
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            // Üst sınıfın OnUpdateFrame metodunu çağırıyoruz
            base.OnUpdateFrame(args);
            
            // Klavye durumunu alıyoruz - hangi tuşların basılı olduğunu kontrol etmek için
            var keyboard = KeyboardState;
            
            // ESC tuşuna basıldıysa uygulamayı kapat
            if (keyboard.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            
            // Robot eklem hızını hesaplıyoruz
            // args.Time = önceki frame'den bu yana geçen süre (saniye cinsinden)
            // 60 derece/saniye * delta time = bu frame'de dönülecek açı
            // Bu, farklı FPS'lerde bile sabit hareket hızı sağlar
            float rotationSpeed = 60f * (float)args.Time;
            
            // ============================================
            // EKLEM KONTROLÜ: THETA1 (TABAN DÖNÜśÜ)
            // ============================================
            // Q tuşu: Taban eklemini saat yönünde döndürür (pozitif yön)
            // E tuşu: Taban eklemini saat yönünün tersine döndürür (negatif yön)
            // Bu eklem Y ekseni etrafında döner ve sınırsız rotasyon yapabilir
            if (keyboard.IsKeyDown(Keys.Q))
                theta1 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.E))
                theta1 -= rotationSpeed;
            
            // ============================================
            // EKLEM KONTROLÜ: THETA2 (OMUZ EKLEMİ)
            // ============================================
            // W tuşu: Omuz eklemini yukarı kaldırır (pozitif yön)
            // S tuşu: Omuz eklemini aşağı indirir (negatif yön)
            // Bu eklem X ekseni etrafında döner
            if (keyboard.IsKeyDown(Keys.W))
                theta2 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.S))
                theta2 -= rotationSpeed;
            
            // ============================================
            // EKLEM KONTROLÜ: THETA3 (DİRSEK EKLEMİ)
            // ============================================
            // A tuşu: Dirsek eklemini yukarı büker (pozitif yön)
            // D tuşu: Dirsek eklemini aşağı büker (negatif yön)
            // Bu eklem de X ekseni etrafında döner
            if (keyboard.IsKeyDown(Keys.A))
                theta3 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.D))
                theta3 -= rotationSpeed;
            
            // ============================================
            // EKLEM KONTROLÜ: THETA4 (BİLEK/ROLL EKLEMİ)
            // ============================================
            // R tuşu: Bileci saat yönünde döndürür (pozitif yön)
            // F tuşu: Bileci saat yönünün tersine döndürür (negatif yön)
            // Bu eklem Y ekseni etrafında döner ve sınırsız 360° rotasyon yapabilir
            if (keyboard.IsKeyDown(Keys.R))
                theta4 += rotationSpeed;
            if (keyboard.IsKeyDown(Keys.F))
                theta4 -= rotationSpeed;
            
            // ============================================
            // ZORUNLU GEREKSİNİM 1: EKLEM SINIRLARI
            // ============================================
            // Omuz (theta2) ve Dirsek (theta3) eklemlerini -90° ile +90° arasında sınırlıyoruz
            // MathHelper.Clamp: Değeri belirtilen minimum ve maksimum arasında tutar
            // Bu sınırlama robot kolunun kendine çarpmasını ve fiziksel olarak imkansız
            // pozisyonlara gitmesini önler
            theta2 = MathHelper.Clamp(theta2, -90f, 90f);
            theta3 = MathHelper.Clamp(theta3, -90f, 90f);
            // Theta4 (bilek) sınırsız - tam 360° dönüş yapabilir, bu gerçek robotlarda da yaygındır
            
            // ============================================
            // PENÇE (GRIPPER) KONTROLÜ
            // ============================================
            // X tuşu: Pençeyi aç/kapat (toggle işlemi)
            // IsKeyPressed: Tuşun bu frame'de basıldığını kontrol eder (her basışta 1 kez)
            // IsKeyDown'dan farkı: Sürekli basılı tutulsa bile sadece bir kez tetiklenir
            if (keyboard.IsKeyPressed(Keys.X))
            {
                // Pençe durumunu tersine çevir (açıksa kapat, kapalıysa aç)
                gripperOpen = !gripperOpen;
            }
            
            // ============================================
            // PENÇE ANİMASYONU (YUMUŞAK GEÇİŞ)
            // ============================================
            // Hedef açıyı belirliyoruz: Açıksa 30°, kapalıysa 0°
            float targetGripperAngle = gripperOpen ? 30f : 0f;
            
            // Linear interpolation (lerp) kullanarak yumuşak geçiş sağlıyoruz
            // Her frame'de hedef açıya doğru %5 yaklaşıyoruz (5f * deltaTime)
            // Bu, anlık değişim yerine pürüzsüz animasyon sağlar
            gripperAngle += (targetGripperAngle - gripperAngle) * 5f * (float)args.Time;
            
            // ============================================
            // KAMERA KONTROLÜ
            // ============================================
            // Yukarı Ok: Kamerayı yukarı eğ (X ekseni pozitif rotasyon)
            if (keyboard.IsKeyDown(Keys.Up))
                cameraAngleX += 30f * (float)args.Time;
            
            // Aşağı Ok: Kamerayı aşağı eğ (X ekseni negatif rotasyon)
            if (keyboard.IsKeyDown(Keys.Down))
                cameraAngleX -= 30f * (float)args.Time;
            
            // Sol Ok: Kamerayı sola döndür (Y ekseni pozitif rotasyon)
            if (keyboard.IsKeyDown(Keys.Left))
                cameraAngleY += 30f * (float)args.Time;
            
            // Sağ Ok: Kamerayı sağa döndür (Y ekseni negatif rotasyon)
            if (keyboard.IsKeyDown(Keys.Right))
                cameraAngleY -= 30f * (float)args.Time;
            
            // PageUp: Zoom in (kamerayı yaklaştır)
            if (keyboard.IsKeyDown(Keys.PageUp))
                cameraDistance -= 5f * (float)args.Time;
            
            // PageDown: Zoom out (kamerayı uzaklaştır)
            if (keyboard.IsKeyDown(Keys.PageDown))
                cameraDistance += 5f * (float)args.Time;
            
            // Kamera uzaklığını 5 ile 20 birim arasında sınırla
            // Çok yakın veya çok uzak kamera pozisyonlarını engeller
            cameraDistance = MathHelper.Clamp(cameraDistance, 5f, 20f);
        }

        /// <summary>
        /// OnRenderFrame: Her frame'de çalışan render (çizim) metodu
        /// Tüm 3D sahneyi çizer ve ekrana görüntüler
        /// Genellikle saniyede 60 kez çağrılır
        /// </summary>
        /// <param name="args">Frame süresi bilgilerini içerir</param>
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            // Üst sınıfın OnRenderFrame metodunu çağırıyoruz
            base.OnRenderFrame(args);
            
            // ============================================
            // EKRANI TEMİZLE
            // ============================================
            // Önceki frame'in renk ve derinlik verilerini temizle
            // ColorBufferBit: Renk bilgilerini temizler (arka plan rengine döner)
            // DepthBufferBit: Derinlik bilgilerini temizler (3D sıralama için gerekli)
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            // ============================================
            // PROJECTION MATRİSİ AYARLARI (PERSPEKTİF)
            // ============================================
            // Projection matrix: 3D sahnenin 2D ekrana nasıl yansıtılacağını belirler
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity(); // Matrix'i sıfırla (birim matris)
            
            // Perspektif hesaplamaları için parametreler
            float aspectRatio = (float)ClientSize.X / ClientSize.Y; // En/boy oranı
            float fov = 45f; // Field of View (görüş alanı) - derece cinsinden
            float near = 0.1f; // Near clipping plane - bu mesafeden daha yakın nesneler görünmez
            float far = 100f; // Far clipping plane - bu mesafeden daha uzak nesneler görünmez
            
            // Frustum (kesik piramit) hesaplamaları
            // FOV'dan viewport boyutlarını hesaplıyoruz
            float top = near * (float)Math.Tan(MathHelper.DegreesToRadians(fov) / 2.0);
            float bottom = -top;
            float right = top * aspectRatio; // En/boy oranına göre ayarla
            float left = -right;
            
            // Perspektif projeksiyon matrisini oluştur
            // Bu, gerçekçi 3D perspektif etkisi sağlar (uzaktaki nesneler küçük görünür)
            GL.Frustum(left, right, bottom, top, near, far);
            
            // ============================================
            // MODEL-VIEW MATRİSİ AYARLARI (KAMERA)
            // ============================================
            // ModelView matrix: Nesnelerin dünya uzayındaki konumunu ve kamera görünümünü belirler
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity(); // Matrix'i sıfırla
            
            // Kamera transformasyonları (ters sırada uygulanır: önce uzaklık, sonra rotasyon)
            // 1. Kamerayı geriye (Z ekseninde negatif) çek - sahneyi görüntüleyebilmek için
            GL.Translate(0f, -2f, -cameraDistance);
            
            // 2. Kamerayı X ekseni etrafında döndür (yukarı/aşağı bakış)
            GL.Rotate(cameraAngleX, 1f, 0f, 0f);
            
            // 3. Kamerayı Y ekseni etrafında döndür (sağ/sol dönüş)
            GL.Rotate(cameraAngleY, 0f, 1f, 0f);
            
            // ============================================
            // SAHNE ÇİZİMİ
            // ============================================
            // Referans zemin grid'ini çiz
            DrawGround();
            
            // Koordinat eksenlerini çiz (X=Kırmızı, Y=Yeşil, Z=Mavi)
            DrawAxes();
            
            // Robot kolunu ve tüm eklemlerini çiz
            DrawRobotArm();
            
            // ============================================
            // BUFFER SWAP (ÇİFT TAMPONLAMA)
            // ============================================
            // Arka planda çizilen frame'i ekrana getir
            // Bu, titreme (flickering) olmadan pürüzsüz animasyon sağlar
            SwapBuffers();
        }

        /// <summary>
        /// DrawGround: Zemin grid'ini çizer
        /// Referans olması için yere (Y=0 seviyesine) bir ızgara çizer
        /// Bu, robot kolunun 3D uzaydaki pozisyonunu anlamayı kolaylaştırır
        /// </summary>
        private void DrawGround()
        {
            // Çizgi çizmeye başla - her iki vertex arasında bir çizgi oluşturulur
            GL.Begin(PrimitiveType.Lines);
            
            // Grid rengini koyu gri olarak ayarla (RGB: 0.3, 0.3, 0.3)
            GL.Color3(0.3f, 0.3f, 0.3f);
            
            // 21x21 boyutunda bir grid oluştur (-10'dan +10'a)
            for (int i = -10; i <= 10; i++)
            {
                // X ekseni boyunca paralel çizgiler (Z yönünde uzanan)
                GL.Vertex3(i, 0, -10);  // Çizgi başlangıcı
                GL.Vertex3(i, 0, 10);   // Çizgi bitişi
                
                // Z ekseni boyunca paralel çizgiler (X yönünde uzanan)
                GL.Vertex3(-10, 0, i);  // Çizgi başlangıcı
                GL.Vertex3(10, 0, i);   // Çizgi bitişi
            }
            
            // Çizgi çizimini bitir
            GL.End();
        }

        /// <summary>
        /// DrawAxes: 3D koordinat eksenlerini çizer
        /// Dünya uzayının orijininden (0,0,0) başlayan renkli oklar
        /// X=Kırmızı, Y=Yeşil, Z=Mavi (standart renk kodlaması)
        /// </summary>
        private void DrawAxes()
        {
            // Çizgi çizmeye başla
            GL.Begin(PrimitiveType.Lines);
            
            // ============================================
            // X EKSENİ (KIRMIZI) - Sağ/Sol yönü
            // ============================================
            GL.Color3(1f, 0f, 0f);  // Saf kırmızı renk
            GL.Vertex3(0, 0, 0);    // Orijinden başla
            GL.Vertex3(2, 0, 0);    // X yönünde 2 birim uzanan çizgi
            
            // ============================================
            // Y EKSENİ (YEŞİL) - Yukarı/Aşağı yönü
            // ============================================
            GL.Color3(0f, 1f, 0f);  // Saf yeşil renk
            GL.Vertex3(0, 0, 0);    // Orijinden başla
            GL.Vertex3(0, 2, 0);    // Y yönünde 2 birim uzanan çizgi
            
            // ============================================
            // Z EKSENİ (MAVİ) - İleri/Geri yönü
            // ============================================
            GL.Color3(0f, 0f, 1f);  // Saf mavi renk
            GL.Vertex3(0, 0, 0);    // Orijinden başla
            GL.Vertex3(0, 0, 2);    // Z yönünde 2 birim uzanan çizgi
            
            // Çizgi çizimini bitir
            GL.End();
        }

        /// <summary>
        /// DrawRobotArm: 4 serbestlik dereceli robot kolunun tamamını çizer
        /// Her eklemi sırayla dönüştürerek (transformation) kinematik zinciri oluşturur
        /// Forward kinematics prensibiyle çalışır
        /// </summary>
        private void DrawRobotArm()
        {
            // Matrix stack'e yeni bir transformation seviyesi ekle
            // Bu, robot kolu çizimini bitirdiğimizde önceki duruma dönmemizi sağlar
            GL.PushMatrix();
            
            // ============================================
            // TABAN (BASE) ÇİZİMİ
            // ============================================
            // Gri renkte taban platformu
            GL.Color3(0.5f, 0.5f, 0.5f);
            // Kısa, geniş silindir (yarıçap: 0.5, yükseklik: 0.3, 20 segment)
            DrawCylinder(0.5f, 0.3f, 20);
            
            // ============================================
            // THETA1 ROTASYONU (TABAN EKLEMİ)
            // ============================================
            // Y ekseni etrafında theta1 kadar döndür
            // Bu dönüş, robotun tüm gövdesini tabanda döndürür
            GL.Rotate(theta1, 0f, 1f, 0f);
            
            // ============================================
            // LINK 1 (TABAN-OMUZ ARASI)
            // ============================================
            // Geçici matrix kaydet (Link çizimi için)
            GL.PushMatrix();
            GL.Color3(0.8f, 0.2f, 0.2f); // Kırmızımsı renk
            // İnce, uzun silindir (yarıçap: 0.15, yükseklik: L1=2.0, 16 segment)
            DrawCylinder(0.15f, L1, 16);
            GL.PopMatrix(); // Link çiziminden sonra matrix'i geri al
            
            // ============================================
            // OMUZ EKLEMİ
            // ============================================
            // Link 1'in sonuna git (Y yönünde L1 kadar yukarı)
            GL.Translate(0f, L1, 0f);
            // Gri renkli küre ile eklem noktasını göster
            GL.Color3(0.6f, 0.6f, 0.6f);
            DrawSphere(0.25f, 20, 20); // Yarıçap: 0.25
            
            // ============================================
            // THETA2 ROTASYONU (OMUZ EKLEMİ)
            // ============================================
            // X ekseni etrafında theta2 kadar döndür
            // Bu dönüş, omuzdan sonraki tüm kolu yukarı/aşağı hareket ettirir
            GL.Rotate(theta2, 1f, 0f, 0f);
            
            // ============================================
            // LINK 2 (OMUZ-DİRSEK ARASI)
            // ============================================
            GL.PushMatrix();
            GL.Color3(0.2f, 0.8f, 0.2f); // Yeşilimsi renk
            // Orta boyut silindir (yarıçap: 0.12, yükseklik: L2=1.5, 16 segment)
            DrawCylinder(0.12f, L2, 16);
            GL.PopMatrix();
            
            // ============================================
            // DİRSEK EKLEMİ
            // ============================================
            // Link 2'nin sonuna git
            GL.Translate(0f, L2, 0f);
            GL.Color3(0.6f, 0.6f, 0.6f); // Gri eklem
            DrawSphere(0.2f, 20, 20); // Biraz daha küçük küre
            
            // ============================================
            // THETA3 ROTASYONU (DİRSEK EKLEMİ)
            // ============================================
            // X ekseni etrafında theta3 kadar döndür
            // Bu dönüş, dirsekten sonraki kolu büker
            GL.Rotate(theta3, 1f, 0f, 0f);
            
            // ============================================
            // LINK 3 (DİRSEK-UÇ EFEKTÖR ARASI)
            // ============================================
            GL.PushMatrix();
            GL.Color3(0.2f, 0.2f, 0.8f); // Mavimsi renk
            // İnce silindir (yarıçap: 0.1, yükseklik: L3=1.0, 16 segment)
            DrawCylinder(0.1f, L3, 16);
            GL.PopMatrix();
            
            // ============================================
            // UÇ EFEKTÖR POZİSYONU
            // ============================================
            // Link 3'ün sonuna git - bu nokta uç efektörün pozisyonu
            GL.Translate(0f, L3, 0f);
            
            // ============================================
            // THETA4 ROTASYONU (BİLEK/ROLL EKLEMİ)
            // ============================================
            // Y ekseni etrafında theta4 kadar döndür
            // Bu, 4. serbestlik derecesi - pençenin kendi ekseni etrafında dönmesi
            GL.Rotate(theta4, 0f, 1f, 0f);
            
            // ============================================
            // ZORUNLU GEREKSİNİM 2: UÇ EFEKTÖR KOORDİNATLARI
            // ============================================
            // Şu anki model-view matrisini al (tüm transformationlar uygulanmış)
            float[] modelMatrixArray = new float[16];
            GL.GetFloat(GetPName.ModelviewMatrix, modelMatrixArray);
            
            // 4x4 matrisin son sütunu (translation bileşeni) dünya koordinatlarıdır
            // OpenGL matrisleri column-major sırada saklanır
            endEffectorX = modelMatrixArray[12]; // M41 - X pozisyonu
            endEffectorY = modelMatrixArray[13]; // M42 - Y pozisyonu\n            endEffectorZ = modelMatrixArray[14]; // M43 - Z pozisyonu
            
            // ============================================
            // ZORUNLU GEREKSİNİM 2: TOPLAM UZUNLUK HESABI
            // ============================================
            // Euclidean Distance formülü: √(x² + y² + z²)
            // Uç efektörün orijine (0,0,0) olan uzaklığını hesaplar
            // Bu, robotun maksimum erişim mesafesini gösterir
            totalReach = (float)Math.Sqrt(endEffectorX * endEffectorX + 
                                          endEffectorY * endEffectorY + 
                                          endEffectorZ * endEffectorZ);
            
            // ============================================
            // ZORUNLU GEREKSİNİM 3: PENÇE MEKANİZMASI
            // ============================================
            // Uç efektöre pençeyi çiz (açılıp kapanabilen)
            DrawGripper();
            
            // Matrix stack'ten transformation'ı kaldır
            // Robot kolu çizimini tamamladık, orijinal duruma dön
            GL.PopMatrix();
            
            // Pencere başlığını güncel koordinatlarla güncelle
            UpdateTitle();
        }

        /// <summary>
        /// DrawGripper: Robot kolunun uç efektöründe bulunan pençeyi çizer
        /// İki parmaklı bir pençe mekanizması simüle eder
        /// Pençe açıklığı gripperAngle değişkeni ile kontrol edilir
        /// </summary>
        private void DrawGripper()
        {
            // ============================================
            // PENÇE TABANI (BAĞLANTI NOKTASI)
            // ============================================
            // Sarı renkte pençe tabanı - uç efektöre bağlantı
            GL.Color3(0.9f, 0.9f, 0.1f);
            // Kısa silindir (yarıçap: 0.15, yükseklik: 0.2, 12 segment)
            DrawCylinder(0.15f, 0.2f, 12);
            
            // Pençe tabanının üstüne çık (parmakların başladığı nokta)
            GL.Translate(0f, 0.2f, 0f);
            
            // ============================================
            // SOL PENÇE PARMAĞI
            // ============================================
            GL.PushMatrix(); // Sol parmak için transformation kaydet
            
            // Pençe açıklığına göre sola döndür (Z ekseni etrafında)
            // Negatif açı = sola açılma
            GL.Rotate(-gripperAngle, 0f, 0f, 1f);
            
            // Sol tarafa kaydır
            GL.Translate(-0.1f, 0.2f, 0f);
            
            // Biraz daha koyu sarı renk
            GL.Color3(0.8f, 0.8f, 0.1f);
            
            // Ana parmak segmenti (dikdörtgen prizma)
            DrawBox(0.08f, 0.4f, 0.08f); // Genişlik, yükseklik, derinlik
            
            // Sol pençe ucu (parmağın içe kıvrılan kısmı)
            GL.Translate(0f, 0.2f, 0f); // Parmağın ucuna git
            GL.Rotate(-30f, 0f, 0f, 1f); // İçe doğru kıvır
            GL.Translate(-0.05f, 0.1f, 0f); // Kıvrım sonrası pozisyonla
            DrawBox(0.08f, 0.2f, 0.06f); // Daha küçük uç parçası
            
            GL.PopMatrix(); // Sol parmak transformation'ını bitir
            
            // ============================================
            // SAĞ PENÇE PARMAĞI
            // ============================================
            GL.PushMatrix(); // Sağ parmak için transformation kaydet
            
            // Pençe açıklığına göre sağa döndür (Z ekseni etrafında)
            // Pozitif açı = sağa açılma (sol parmağın ayna görüntüsü)
            GL.Rotate(gripperAngle, 0f, 0f, 1f);
            
            // Sağ tarafa kaydır
            GL.Translate(0.1f, 0.2f, 0f);
            
            // Biraz daha koyu sarı renk
            GL.Color3(0.8f, 0.8f, 0.1f);
            
            // Ana parmak segmenti
            DrawBox(0.08f, 0.4f, 0.08f);
            
            // Sağ pençe ucu (parmağın içe kıvrılan kısmı)
            GL.Translate(0f, 0.2f, 0f); // Parmağın ucuna git
            GL.Rotate(30f, 0f, 0f, 1f); // İçe doğru kıvır (pozitif yön)
            GL.Translate(0.05f, 0.1f, 0f); // Kıvrım sonrası pozisyonla
            DrawBox(0.08f, 0.2f, 0.06f); // Daha küçük uç parçası
            
            GL.PopMatrix(); // Sağ parmak transformation'ını bitir
        }

        /// <summary>
        /// UpdateTitle: Pencere başlığını güncel robot durumu bilgileriyle günceller
        /// Uç efektör koordinatları, toplam uzunluk, pençe durumu ve kontrol bilgilerini gösterir
        /// Her frame'de çağrılır, böylece kullanıcı anlık değerleri görebilir
        /// </summary>
        private void UpdateTitle()
        {
            // Pençe durumunu Türkçe metne çevir
            string gripperStatus = gripperOpen ? "Açık" : "Kapalı";
            
            // Pencere başlığını formatla ve güncelle
            // string.Format: Değişkenleri belirtilen formatta metne dönüştürür
            // {0:F2} formatı: İlk parametre, 2 ondalık basamak (Fixed-point notation)
            Title = string.Format(
                "4 DOF Robot Kolu | " + // Uygulama adı
                "X: {0:F2} Y: {1:F2} Z: {2:F2} | " + // Uç efektör 3D koordinatları
                "Toplam Uzunluk: {3:F2} | " + // Orijine olan uzaklık
                "Pençe: {4} | " + // Pençe durumu (Açık/Kapalı)
                "θ4: {5:F1}° | " + // 4. eklem açısı (theta4)
                "Kontroller: Q/E(Taban) W/S(Omuz) A/D(Dirsek) R/F(Bilek Roll) X(Pençe) Oklar(Kamera)", // Kullanım talimatları
                endEffectorX,    // {0} - X koordinatı
                endEffectorY,    // {1} - Y koordinatı
                endEffectorZ,    // {2} - Z koordinatı
                totalReach,      // {3} - Toplam uzunluk
                gripperStatus,   // {4} - Pençe durumu metni
                theta4           // {5} - Theta4 açısı
            );
        }

        /// <summary>
        /// DrawCylinder: 3D silindir geometrisi çizer
        /// Robot kolunun linklerini (eklemler arası bağlantılar) oluşturmak için kullanılır
        /// Alt ve üst kapaklar + yan yüzey olmak üzere 3 bölümden oluşur
        /// </summary>
        /// <param name="radius">Silindirin yarıçapı</param>
        /// <param name="height">Silindirin yüksekliği (Y ekseni boyunca)</param>
        /// <param name="segments">Silindiri oluşturan segment sayısı (daha fazla = daha pürüzsüz)</param>
        private void DrawCylinder(float radius, float height, int segments)
        {
            // ============================================
            // ALT KAPAK (Y = 0 seviyesinde)
            // ============================================
            // TriangleFan: Merkez noktadan başlayarak dışa doğru üçgenler oluşturur
            GL.Begin(PrimitiveType.TriangleFan);
            
            // Normal vektör: Aşağı bakıyor (0, -1, 0) - ışık hesaplamaları için
            GL.Normal3(0f, -1f, 0f);
            
            // Merkez nokta (silindirin tabanının ortası)
            GL.Vertex3(0f, 0f, 0f);
            
            // Daire çevresi boyunca vertex'ler oluştur
            for (int i = 0; i <= segments; i++)
            {
                // 0'dan 2π'ye kadar eşit aralıklarla açılar
                float angle = 2f * (float)Math.PI * i / segments;
                
                // Polar koordinatları Kartezyen'e çevir
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                
                // Vertex ekle (Y = 0)
                GL.Vertex3(x, 0f, z);
            }
            GL.End();
            
            // ============================================
            // ÜST KAPAK (Y = height seviyesinde)
            // ============================================
            GL.Begin(PrimitiveType.TriangleFan);
            
            // Normal vektör: Yukarı bakıyor (0, 1, 0)
            GL.Normal3(0f, 1f, 0f);
            
            // Merkez nokta (silindirin üst kapağının ortası)
            GL.Vertex3(0f, height, 0f);
            
            // Daire çevresi boyunca vertex'ler oluştur
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * (float)Math.PI * i / segments;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                
                // Vertex ekle (Y = height)
                GL.Vertex3(x, height, z);
            }
            GL.End();
            
            // ============================================
            // YAN YÜZEY (Silindirin gövdesi)
            // ============================================
            // QuadStrip: Alt ve üst vertexleri sırayla birleştirerek dikdörtgenler oluşturur
            GL.Begin(PrimitiveType.QuadStrip);
            
            // Her segment için alt ve üst vertex çifti
            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * (float)Math.PI * i / segments;
                float x = radius * (float)Math.Cos(angle);
                float z = radius * (float)Math.Sin(angle);
                
                // Normal vektör: Dışa doğru (radyal yön) - ışık hesaplamaları için
                // Normalize edilmiş: (x/radius, 0, z/radius)
                GL.Normal3(x / radius, 0f, z / radius);
                
                // Alt vertex (Y = 0)
                GL.Vertex3(x, 0f, z);
                
                // Üst vertex (Y = height)
                GL.Vertex3(x, height, z);
            }
            GL.End();
        }

        /// <summary>
        /// DrawBox: 3D dikdörtgen prizma (kutu) geometrisi çizer
        /// Pençe parmaklarını oluşturmak için kullanılır
        /// Merkez noktasından eşit uzaklıkta 6 yüzden oluşur
        /// </summary>
        /// <param name="width">Kutunun genişliği (X ekseni boyunca)</param>
        /// <param name="height">Kutunun yüksekliği (Y ekseni boyunca)</param>
        /// <param name="depth">Kutunun derinliği (Z ekseni boyunca)</param>
        private void DrawBox(float width, float height, float depth)
        {
            // Yarı boyutları hesapla - merkez noktadan kenarlara olan uzaklık
            // Bu, kutuyu orijin (0,0,0) etrafında merkezler
            float w = width / 2f;   // Yarı genişlik
            float h = height / 2f;  // Yarı yükseklik
            float d = depth / 2f;   // Yarı derinlik
            
            // Quads (dörtgenler) çizmeye başla - her 4 vertex bir yüz oluşturur
            GL.Begin(PrimitiveType.Quads);
            
            // ============================================
            // ÖN YÜZ (+Z yönünde)
            // ============================================
            // Normal vektör: İzleyiciye doğru (0, 0, 1) - ışık hesaplamaları için
            GL.Normal3(0f, 0f, 1f);
            // Vertex'ler saat yönünün tersine (counter-clockwise) sırayla
            // Sol alt köşe
            GL.Vertex3(-w, -h, d);
            // Sağ alt köşe
            GL.Vertex3(w, -h, d);
            // Sağ üst köşe
            GL.Vertex3(w, h, d);
            // Sol üst köşe
            GL.Vertex3(-w, h, d);
            
            // ============================================
            // ARKA YÜZ (-Z yönünde)
            // ============================================
            // Normal vektör: İzleyiciden uzağa (0, 0, -1)
            GL.Normal3(0f, 0f, -1f);
            // Vertex sırası: arkadan bakıldığında saat yönünün tersine
            GL.Vertex3(-w, -h, -d);  // Sol alt
            GL.Vertex3(-w, h, -d);   // Sol üst
            GL.Vertex3(w, h, -d);    // Sağ üst
            GL.Vertex3(w, -h, -d);   // Sağ alt
            
            // ============================================
            // ÜST YÜZ (+Y yönünde)
            // ============================================
            // Normal vektör: Yukarı (0, 1, 0)
            GL.Normal3(0f, 1f, 0f);
            // Yukarıdan bakıldığında saat yönünün tersine
            GL.Vertex3(-w, h, -d);  // Sol arka
            GL.Vertex3(-w, h, d);   // Sol ön
            GL.Vertex3(w, h, d);    // Sağ ön
            GL.Vertex3(w, h, -d);   // Sağ arka
            
            // ============================================
            // ALT YÜZ (-Y yönünde)
            // ============================================
            // Normal vektör: Aşağı (0, -1, 0)
            GL.Normal3(0f, -1f, 0f);
            // Aşağıdan bakıldığında saat yönünün tersine
            GL.Vertex3(-w, -h, -d);  // Sol arka
            GL.Vertex3(w, -h, -d);   // Sağ arka
            GL.Vertex3(w, -h, d);    // Sağ ön
            GL.Vertex3(-w, -h, d);   // Sol ön
            
            // ============================================
            // SAĞ YÜZ (+X yönünde)
            // ============================================
            // Normal vektör: Sağa (1, 0, 0)
            GL.Normal3(1f, 0f, 0f);
            // Sağdan bakıldığında saat yönünün tersine
            GL.Vertex3(w, -h, -d);  // Alt arka
            GL.Vertex3(w, h, -d);   // Üst arka
            GL.Vertex3(w, h, d);    // Üst ön
            GL.Vertex3(w, -h, d);   // Alt ön
            
            // ============================================
            // SOL YÜZ (-X yönünde)
            // ============================================
            // Normal vektör: Sola (-1, 0, 0)
            GL.Normal3(-1f, 0f, 0f);
            // Soldan bakıldığında saat yönünün tersine
            GL.Vertex3(-w, -h, -d);  // Alt arka
            GL.Vertex3(-w, -h, d);   // Alt ön
            GL.Vertex3(-w, h, d);    // Üst ön
            GL.Vertex3(-w, h, -d);   // Üst arka
            
            // Quad çizimini bitir
            GL.End();
        }

        /// <summary>
        /// DrawSphere: 3D küre geometrisi çizer
        /// Robot eklemlerini (omuz, dirsek) görselleştirmek için kullanılır
        /// Küresel koordinat sistemi (latitude/longitude) kullanarak oluşturulur
        /// </summary>
        /// <param name="radius">Kürenin yarıçapı</param>
        /// <param name="slices">Boylam dilimi sayısı (dikey dilimler, daha fazla = daha pürüzsüz)</param>
        /// <param name="stacks">Enlem halka sayısı (yatay halkalar, daha fazla = daha pürüzsüz)</param>
        private void DrawSphere(float radius, int slices, int stacks)
        {
            // Küreyi yatay halkalar (stacks) halinde çiz
            // Her halka, bir QuadStrip ile oluşturulur
            for (int i = 0; i < stacks; i++)
            {
                // ============================================
                // ENLEM (LATITUDE) HESAPLAMALARI
                // ============================================
                // lat0: Mevcut halkanın enlem açısı (-π/2'den +π/2'ye, yani -90° ile +90° arası)
                // -0.5 = güney kutbu (-90°), +0.5 = kuzey kutbu (+90°)
                float lat0 = (float)Math.PI * (-0.5f + (float)i / stacks);
                
                // z0: Mevcut halkanın Y ekseni pozisyonu (dikey)
                // Sin(lat) = -1'den +1'e değişir, radius ile çarpınca küre yüksekliği
                float z0 = radius * (float)Math.Sin(lat0);
                
                // zr0: Mevcut halkanın yarıçapı (yatay düzlemde)
                // Cos(lat) = ekvatorda 1, kutuplarda 0
                float zr0 = radius * (float)Math.Cos(lat0);
                
                // lat1: Bir sonraki halkanın enlem açısı
                float lat1 = (float)Math.PI * (-0.5f + (float)(i + 1) / stacks);
                
                // z1 ve zr1: Bir sonraki halkanın koordinatları
                float z1 = radius * (float)Math.Sin(lat1);
                float zr1 = radius * (float)Math.Cos(lat1);
                
                // QuadStrip: İki halka arasında dörtgenler oluşturur
                // Her vertex çifti bir dikey kenar oluşturur
                GL.Begin(PrimitiveType.QuadStrip);
                
                // Her boylam dilimi için (tam daire: 0'dan 2π'ye)
                for (int j = 0; j <= slices; j++)
                {
                    // ============================================
                    // BOYLAM (LONGITUDE) HESAPLAMALARI
                    // ============================================
                    // lng: Boylam açısı (0'dan 2π'ye, 0° ile 360° arası)
                    float lng = 2f * (float)Math.PI * (float)j / slices;
                    
                    // x ve y: Birim çember üzerinde kosinüs ve sinüs
                    // Bunlar yatay düzlemde (XZ düzlemi) dairesel hareketi sağlar
                    float x = (float)Math.Cos(lng);
                    float y = (float)Math.Sin(lng);
                    
                    // ============================================
                    // MEVCUT HALKA VERTEX'İ
                    // ============================================
                    // Normal vektör: Küre merkezinden dışa doğru
                    // Küre için normal = normalize edilmiş pozisyon vektörü
                    GL.Normal3(x * zr0, y * zr0, z0);
                    
                    // Vertex pozisyonu: Küresel koordinatları Kartezyen'e çevir
                    // X = cos(lng) * cos(lat) * radius
                    // Y = sin(lat) * radius  (dikey pozisyon)
                    // Z = sin(lng) * cos(lat) * radius
                    // Not: Y ve Z yer değiştirilmiş çünkü Y yukarı ekseni
                    GL.Vertex3(x * zr0, z0, y * zr0);
                    
                    // ============================================
                    // SONRAKİ HALKA VERTEX'İ
                    // ============================================
                    // Aynı boylam ama bir üst enlem
                    GL.Normal3(x * zr1, y * zr1, z1);
                    GL.Vertex3(x * zr1, z1, y * zr1);
                }
                
                // Bu halka çiftini bitir
                GL.End();
            }
        }
    }
}
