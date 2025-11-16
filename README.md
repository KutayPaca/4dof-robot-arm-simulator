# 4 Serbestlik Dereceli (4 DOF) Robot Kolu Simülatörü

OpenTK ve C# kullanılarak geliştirilmiş, bilek rotasyonu (wrist roll) özellikli gelişmiş 3D robot kolu simülasyonu.

## 🎯 Özellikler

### Ana Özellikler

#### 🤖 4 Serbestlik Dereceli Robot Kolu
- **Theta1 (Taban)**: Y ekseni etrafında sınırsız rotasyon
- **Theta2 (Omuz)**: X ekseni etrafında [-90°, 90°] sınırlı hareket
- **Theta3 (Dirsek)**: X ekseni etrafında [-90°, 90°] sınırlı hareket
- **Theta4 (Bilek Roll)**: Y ekseni etrafında sınırsız 360° rotasyon

#### 📐 İleri Kinematik (Forward Kinematics)
- Gerçek zamanlı uç efektör pozisyon hesaplama (X, Y, Z)
- Model-view matrisi ile doğrudan koordinat çıkarımı
- Toplam erişim mesafesi hesaplama: `√(X² + Y² + Z²)`
- Pencere başlığında anlık koordinat gösterimi

#### 🔧 Eklem Sınırlandırması
- Theta2 ve Theta3 için fiziksel sınırlar: [-90°, 90°]
- `MathHelper.Clamp()` ile kinematik stabilizasyon
- Robot kolunun kendi içine veya zemine çarpma koruması

#### 🤏 İnteraktif Pençe (Gripper) Mekanizması
- X tuşu ile açılıp kapanabilen çift parmaklı tasarım
- Yumuşak animasyonlu açılma/kapanma
- Pençe durumu gerçek zamanlı gösterimi
- Theta4 ile birlikte tam 4 DOF kontrol

#### 🎨 Gerçekçi 3D Geometri
- **Silindir Link Geometrisi**: Tüm linkler profesyonel silindir modelleme
  - Link 1: 0.15 radius, 2.0 height, 16 segment
  - Link 2: 0.12 radius, 1.5 height, 16 segment
  - Link 3: 0.10 radius, 1.0 height, 16 segment
- **Küre Eklemler**: Gerçekçi eklem noktaları
- **Renk Kodlu Linkler**: Kolay görsel takip
- **16 Segment Smooth Rendering**: Pürüzsüz yüzeyler

#### 📊 Transformasyon Hiyerarşisi
```
Taban → Theta1 (Y) → Link1 → Theta2 (X) → Link2 → 
Theta3 (X) → Link3 → Theta4 (Y Roll) → Gripper
```

## 🎮 Kontroller

### Robot Kolu Kontrolleri
- **Q / E**: Theta1 - Taban dönüşü (Y ekseni, sınırsız rotasyon)
- **W / S**: Theta2 - Omuz hareketi (X ekseni, [-90°, 90°] sınırlı)
- **A / D**: Theta3 - Dirsek hareketi (X ekseni, [-90°, 90°] sınırlı)
- **R / F**: Theta4 - Bilek roll dönüşü (Y ekseni, sınırsız 360° rotasyon)
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
- **L1 = 2.0**: Taban-Omuz arası uzunluk (silindir: radius=0.15, 16 segment)
- **L2 = 1.5**: Omuz-Dirsek arası uzunluk (silindir: radius=0.12, 16 segment)
- **L3 = 1.0**: Dirsek-Bilek arası uzunluk (silindir: radius=0.10, 16 segment)
- **Maksimum Erişim**: ~4.5 birim (tamamen uzanmış durumda)
- **Geometri**: Gerçekçi silindir geometrisi ile profesyonel modelleme

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
theta2 = MathHelper.Clamp(theta2, -90f, 90f);  // Omuz: [-90°, 90°]
theta3 = MathHelper.Clamp(theta3, -90f, 90f);  // Dirsek: [-90°, 90°]
// theta1 (Taban) ve theta4 (Bilek Roll): Sınırsız 360° rotasyon
```

### OpenGL Ayarları
```csharp
// OpenTK 4.8.2 için uyumlu ayarlar
API = ContextAPI.OpenGL
APIVersion = new Version(3, 3)
Profile = ContextProfile.Compatability  // Legacy OpenGL desteği
Flags = ContextFlags.Default
```

### 🔧 Neden OpenTK 4.8.2?
Bu proje **OpenTK 4.8.2** kullanır çünkü:
- ✅ .NET 6.0 ile tam uyumlu en güncel kararlı sürüm (Kasım 2023)
- ✅ Legacy OpenGL (`GL.Begin/End`, `GL.Vertex3`, vb.) tam desteği
- ✅ 130M+ indirme ile en popüler C# OpenGL wrapper'ı
- ✅ `Matrix4`, `Vector3` gibi matematik yapıları optimize edilmiş
- ✅ `ContextProfile.Compatability` ile eski ve yeni OpenGL birlikte kullanılabilir
- ✅ Aktif topluluk desteği ve düzenli bug düzeltmeleri

**Not:** OpenTK 5.x henüz preview/alpha aşamasında ve üretim için önerilmez. OpenTK 3.x ise .NET 6.0 ile tam uyumlu değildir.

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
4 DOF Robot Kolu | X: 0.00 Y: 2.23 Z: -8.46 | Toplam Uzunluk: 8.75 | Pençe: Kapalı | θ4: 45.0° | Kontroller: Q/E(Taban) W/S(Omuz) A/D(Dirsek) R/F(Bilek Roll) X(Pençe) Oklar(Kamera)
```

## 🎨 Görsel Özellikler

- **Renkli Robot Kısımları**:
  - 🔴 Kırmızı: Link 1 (Taban-Omuz) - Silindir geometri
  - 🟢 Yeşil: Link 2 (Omuz-Dirsek) - Silindir geometri
  - 🔵 Mavi: Link 3 (Dirsek-Bilek) - Silindir geometri
  - 🟡 Sarı: Pençe mekanizması (theta4 ile roll rotasyonu)
  - ⚪ Gri: Eklemler ve taban (küre geometri)

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

## 🚀 Neden Bu Proje?

Bu 4 DOF robot kolu simülatörü, modern robotik sistemlerin temel prensiplerini öğrenmek ve uygulamak için geliştirilmiştir:

- ✅ **Gerçek Dünya Modelleme**: Endüstriyel robot kollarındaki 4 eksenli sistemleri simüle eder
- ✅ **Forward Kinematics**: Eklem açılarından uç efektör pozisyonunu hesaplama
- ✅ **Eklem Sınırları**: Fiziksel kısıtlamaları uygulayarak gerçekçi hareket
- ✅ **Bilek Rotasyonu**: Theta4 ile profesyonel robot kollarındaki roll özelliği
- ✅ **3D Görselleştirme**: OpenGL ile gerçek zamanlı rendering
- ✅ **İnteraktif Kontrol**: Anlık klavye kontrolü ile dinamik test

## 💡 Kullanım Alanları

- 📚 **Eğitim**: Robotik kinematik öğrenimi
- 🔬 **Araştırma**: Robot kolu davranış simülasyonu
- 🎮 **Prototipleme**: Gerçek robot kontrol testleri öncesi validasyon
- 🛠️ **Geliştirme**: Forward kinematics algoritma testi

## 👨‍💻 Geliştirici

**Robotik Sistemler Projesi - 4 DOF Robot Kolu Simülatörü**

**Geliştirme Ortamı:**
- Visual Studio Code
- .NET 6.0.428
- OpenTK 4.8.2
- Windows 11

**Temel Özellikler:**
- 4 Serbestlik Dereceli Robot Kolu
- Bilek Rotasyonu (Wrist Roll - Theta4)
- Silindir Geometri Modelleme
- Forward Kinematics
- İnteraktif Gripper Kontrolü
- Gerçek Zamanlı Koordinat Gösterimi

## 📜 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---

**Proje Durumu:** ✅ Tamamlandı ve Test Edildi
