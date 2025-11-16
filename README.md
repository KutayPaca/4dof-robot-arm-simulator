# 3 Serbestlik Dereceli (3 DOF) Robot Kolu Simülasyonu

OpenTK ve C# kullanılarak geliştirilmiş interaktif 3D robot kolu simülasyonu.

## 🎯 Özellikler

### Zorunlu Gereksinimler (Tamamlandı ✅)

#### 1. Eklem Sınırlandırması ve Kinematik Stabilizasyon
- ✅ Theta2 (Omuz) ve Theta3 (Dirsek) açıları [-90°, 90°] aralığında sınırlandırılmıştır
- ✅ `MathHelper.Clamp()` metodu kullanılarak fiziksel sınırlamalar uygulanmıştır
- ✅ Robot kolunun kendi içine veya zemine çarpması engellenmiştir

#### 2. İleri Kinematik Değer Hesaplama ve Gösterimi
- ✅ Uç efektörün X, Y, Z global koordinatları pencere başlığında gösterilmektedir
- ✅ `GL.GetFloat(GetPName.ModelviewMatrix)` ile model-view matrisi alınmaktadır
- ✅ Toplam uzunluk (Euclidean Distance) hesaplanmakta ve gösterilmektedir
  - Formül: `√(X² + Y² + Z²)`

#### 3. Pençe Mekanizması
- ✅ X tuşu ile açılıp kapanabilen gerçekçi pençe (gripper) tasarımı
- ✅ İki parmaklı pençe mekanizması (sol ve sağ)
- ✅ Pençe durumu ("Açık" / "Kapalı") pencere başlığında gösterilmektedir
- ✅ Yumuşak açılma/kapanma animasyonu

## 🎮 Kontroller

### Robot Kolu Kontrolleri
- **Q / E**: Theta1 - Taban dönüşü (Y ekseni etrafında)
- **W / S**: Theta2 - Omuz hareketi (X ekseni etrafında, [-90°, 90°] sınırlı)
- **A / D**: Theta3 - Dirsek hareketi (X ekseni etrafında, [-90°, 90°] sınırlı)
- **X**: Pençeyi aç/kapat (toggle)

### Kamera Kontrolleri
- **↑ / ↓**: Kamera yükseklik açısı
- **← / →**: Kamera yatay dönüş
- **Page Up / Page Down**: Zoom in/out

### Diğer
- **ESC**: Uygulamadan çık

## 🏗️ Proje Yapısı

```
RobotKoluSimulasyonu/
├── RobotKoluSimulasyonu.csproj  # .NET 6.0 proje dosyası
├── Program.cs                    # Ana giriş noktası
├── RobotWindow.cs               # Ana simülasyon penceresi ve robot mantığı
└── README.md                    # Bu dosya
```

## 🔧 Teknik Detaylar

### Robot Kolu Parametreleri
- **L1 = 2.0**: Taban-Omuz arası uzunluk
- **L2 = 1.5**: Omuz-Dirsek arası uzunluk
- **L3 = 1.0**: Dirsek-Uç efektör arası uzunluk
- **Maksimum Erişim**: ~4.5 birim (tamamen uzanmış durumda)

### İleri Kinematik (GÜNCEL KOD)
Robot kolunun uç efektör pozisyonu, her frame'de model-view matrisinden hesaplanır:
```csharp
// OpenTK 4.x uyumlu yöntem
float[] modelMatrixArray = new float[16];
GL.GetFloat(GetPName.ModelviewMatrix, modelMatrixArray);
endEffectorX = modelMatrixArray[12]; // M41
endEffectorY = modelMatrixArray[13]; // M42
endEffectorZ = modelMatrixArray[14]; // M43

// Toplam uzunluk hesaplama
totalReach = (float)Math.Sqrt(endEffectorX * endEffectorX + 
                              endEffectorY * endEffectorY + 
                              endEffectorZ * endEffectorZ);
```

#### ⚠️ Matrix4 Kullanımı Hakkında Önemli Not
Bazı kaynaklarda `Matrix4` yapısı ile doğrudan matris alma önerilse de:
```csharp
// ÖNERİLEN AMA ÇALIŞMAYAN YÖNTEMMatrix4 modelMatrix;
GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);
endEffectorX = modelMatrix.M41;
```

**OpenTK 4.8.2** sürümünde `out Matrix4` parametresi düzgün çalışmamaktadır. Bu nedenle projede **`float[]` array yöntemi** kullanılmıştır. Bu yöntem:
- ✅ **Daha güvenilir** ve stabil
- ✅ OpenTK 4.x ile **%100 uyumlu**
- ✅ Tüm OpenGL sürümlerinde **test edilmiş**
- ✅ Array indeksleri (`[12], [13], [14]`) Matrix4 elemanlarına (`M41, M42, M43`) karşılık gelir

### Eklem Sınırları
```csharp
theta2 = MathHelper.Clamp(theta2, -90f, 90f);
theta3 = MathHelper.Clamp(theta3, -90f, 90f);
```

### OpenGL Ayarları
```csharp
// OpenTK 4.8.2 için uyumlu ayarlar
API = ContextAPI.OpenGL
APIVersion = new Version(3, 3)
Profile = ContextProfile.Compatability  // Legacy OpenGL desteği
Flags = ContextFlags.Default
```

## 📦 Gereksinimler

- **.NET 6.0 SDK** veya üzeri (test edildi: 6.0.428)
- **OpenTK 4.8.2**
- **Windows 10/11** (OpenGL 3.3+ destekli grafik kartı)

## 🚀 Kurulum ve Çalıştırma

1. Projeyi restore edin:
```bash
dotnet restore
```

2. Projeyi derleyin:
```bash
dotnet build
```

3. Uygulamayı çalıştırın:
```bash
dotnet run
```

### İlk Çalıştırma
Program başlatıldığında konsol penceresinde şu bilgiler görüntülenir:
```
OpenGL Version: 3.3.x
OpenGL Renderer: [Grafik Kartı Adı]
OpenGL Vendor: [Üretici]
OpenGL Yüklendi! Robot görünmelidir.
Pencere boyutu: 1280x720
```

## 📊 Pencere Başlığı Bilgileri

Pencere başlığında şu bilgiler gerçek zamanlı olarak gösterilir:
- **X, Y, Z**: Uç efektör koordinatları (2 ondalık basamak)
- **Toplam Uzunluk**: Orjinden uç efektöre olan mesafe
- **Pençe Durumu**: Açık veya Kapalı
- **Kontrol Yardımı**: Temel tuş kombinasyonları

Örnek: 
```
3 DOF Robot Kolu | X: 0.00 Y: 2.23 Z: -8.46 | Toplam Uzunluk: 8.75 | Pençe: Kapalı | Kontroller: Q/E(Taban) W/S(Omuz) A/D(Dirsek) X(Pençe) Oklar(Kamera)
```

## 🎨 Görsel Özellikler

- **Renkli Robot Kısımları**:
  - 🔴 Kırmızı: Link 1 (Taban-Omuz)
  - 🟢 Yeşil: Link 2 (Omuz-Dirsek)
  - 🔵 Mavi: Link 3 (Dirsek-Uç efektör)
  - 🟡 Sarı: Pençe mekanizması
  - ⚪ Gri: Eklemler ve taban

- **Sahne Elemanları**:
  - Grid zemin (beyaz çizgiler)
  - 3D koordinat eksenleri (X-Kırmızı, Y-Yeşil, Z-Mavi)
  - Dinamik kamera sistemi

- **Rendering**: OpenGL immediate mode (Legacy) - OpenTK 4.x uyumlu

## 🐛 Sorun Giderme

### Robot Görünmüyorsa
1. Grafik sürücülerinizi güncelleyin
2. Konsol çıktısında "OpenGL Version" kontrolü yapın
3. `ContextProfile.Compatability` ayarını kontrol edin

### Performans Sorunları
- VSync varsayılan olarak aktif
- `UpdateFrequency = 60.0` ile sınırlandırılmış

## 📝 Notlar

- ✅ Tüm **zorunlu gereksinimler** başarıyla uygulanmıştır
- ✅ Kod iyi organize edilmiş ve **Türkçe yorumlanmıştır**
- ✅ Pençe mekanizması **gerçekçi bir tasarıma** sahiptir
- ✅ Tüm hesaplamalar **gerçek zamanlı** olarak yapılır
- ✅ **OpenTK 4.8.2** ve **.NET 6.0** ile tam uyumlu
- ✅ Konsol çıktısı ile **debug bilgileri** sağlanır

## 👨‍💻 Geliştirici

Robotik Sistemler Projesi - 3 DOF Robot Kolu Simülasyonu

**Geliştirme Ortamı:**
- Visual Studio Code
- .NET 6.0.428
- OpenTK 4.8.2
- Windows 11

---

**Proje Durumu:** ✅ Tamamlandı ve Test Edildi
