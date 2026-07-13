#  Kütüphane Rezervasyon ve Ödünç Sistemi

Bitirme projesi olarak geliştirilmiş, koltuk rezervasyonu ve kitap ödünç yönetimini tek çatıda toplayan tam yığın (full-stack) kütüphane otomasyonu.
QR tabanlı giriş/çıkış takibi, sadakat puanı sistemi, gerçek zamanlı koltuk durumu ve AI destekli asistan içerir.

##  Mimari

**Katmanlı mimari** — Repository & Unit of Work desenleri:
LibraryProject.Model      → Entity'ler, enum'lar, DTO'lar, arayüzler
LibraryProject.Data       → EF Core DbContext, Repository/UoW, Dapper sorguları
LibraryProject.Business   → İş kuralları (servisler)
LibraryProject (Web)      → ASP.NET Core MVC — 3 panel (Areas)
LibraryProject.Api        → JWT korumalı Mobile API
library_mobile (Flutter)  → Ziyaretçi mobil uygulaması

##  Teknolojiler

Backend | ASP.NET Core (.NET 10), EF Core + Dapper, ASP.NET Identity |
Arka plan işleri | Hangfire (no-show iptali, otomatik çıkış, puan, temizlik) |
Gerçek zamanlı | SignalR (gürültü ihbarı, canlı koltuk krokisi) |
Mobil | Flutter (Dart), JWT, mobile_scanner (QR kamera) |
AI | Google Gemini API (kural + katalog bağlamlı asistan) |
Loglama | Serilog |
Veritabanı | SQL Server |

##  Özellikler

### Web — 3 Panel
- **Admin:** istatistik paneli (Dapper), duyuru/kitap/çalışan/ziyaretçi/hediye CRUD,
  ada göre arama, canlı koltuk krokisi, hediye talep onayı
- **Personel:** kitap ödünç ver/iade al (gecikme yasağı otomatiği), günlük rezervasyon
  takibi + kroki, anlık gürültü ihbarı bildirimi (SignalR toast)
- **Ziyaretçi:** interaktif krokiden saat aralıklı rezervasyon, rezervasyon geçmişi,
  kitaplarım, sadakat puanı + hediye talebi, AI asistan, gürültü ihbar butonu

### Rezervasyon Motoru
- Günde en fazla 6 saat, kişi başı 3 aktif rezervasyon, çakışma kontrolü
- 30 dk giriş toleransı — aşımda otomatik no-show (3 no-show → 1 hafta yasak)
- 1,5 saat bölünebilir mola + 5 dk tolerans — aşımda otomatik iptal
- Çıkışta süreye göre puan (saat başı 10 puan) + haftalık düzenlilik bonusu (5 gün → 50 p)

### QR Akışı
- Girişte 5 sn'de bir yenilenen, sunucu doğrulamalı, tek kullanımlık (10 sn ömürlü) QR
- Mobil uygulamadan: Giriş / Mola Ver / Mola Bitir / Çıkış — kamera ile okutma

### AI Asistan
- Gemini tabanlı; kütüphane kuralları system prompt'ta, kitap kataloğu her soruda
  veritabanından beslenir (mini RAG) — katalog dışı kitap önermez

##  Ekran Görüntüleri

### Web
| | |
|---|---|
| ![Login](docs/screenshots/login.png) | ![Admin Dashboard](docs/screenshots/admin-dashboard.png) |
| ![Kroki](docs/screenshots/seat-map.png) | ![Rezervasyon](docs/screenshots/reservation.png) |
| ![Kitap Yönetimi](docs/screenshots/admin-books.png) | ![Sadakat](docs/screenshots/loyalty.png) |
| ![Chatbot](docs/screenshots/chatbot.png) | ![Ziyaretçi Duyurular](docs/screenshots/employee-announcements.png) |

### Kiosk
![Kiosk](docs/screenshots/kiosk.png)

### Mobil (Flutter)
| | | |
|---|---|---|
| ![Mobil Login](docs/screenshots/mobile-login.jpeg) | ![QR İşlemleri](docs/screenshots/mobile-qr.jpeg) | ![Tarayıcı](docs/screenshots/mobile-scanner.jpeg) |

##  Kurulum

### Backend
1. Repoyu klonlayın, `LibraryProject.sln`'i Visual Studio 2022+ ile açın
2. `LibraryProject/appsettings.json` ve `LibraryProject.Api/appsettings.json` içinde:
   - Connection string'i kendi SQL Server'ınıza göre düzenleyin
   - `Gemini:ApiKey` alanına [Google AI Studio](https://aistudio.google.com)'dan
     alacağınız anahtarı girin
   - API projesindeki `Jwt:Key` alanına en az 32 karakterlik bir anahtar girin
3. Package Manager Console (Default project: LibraryProject.Data):
   `Update-Database`
4. Çoklu başlangıç: Web + Api birlikte başlatın (F5) — seed veriler otomatik yüklenir

**Test hesapları:** `admin@library.com`, `personel@library.com`,
`ziyaretci1@library.com`, `kiosk@library.com` (şifreler: `Admin123`, `Personel123`,
`Ziyaretci123`, `Kiosk123`)

### Mobil
1. `library_mobile/lib/services/api_client.dart` içindeki `baseUrl` IP'sini kendi
   bilgisayarınızın yerel ağ IP'siyle değiştirin (`ipconfig` → IPv4)
2. API'nin `launchSettings.json`'ında `applicationUrl` = `http://0.0.0.0:5193` olmalı
3. Telefon ve bilgisayar aynı Wi-Fi ağında olmalı
4. `flutter pub get` → `flutter run`

##  Bilinen Sınırlamalar
- Mobilden yapılan işlemler web krokisini anlık tetiklemez (API'de SignalR hub'ı yok;
  çözüm: ortak mesaj altyapısı — gelecek geliştirme)
- TC kimlik doğrulaması format bazlıdır (NVİ entegrasyonu üretim kapsamı)
- Ücretsiz Gemini katmanı yoğun saatlerde gecikebilir (retry mekanizması mevcuttur)

##  Gelecek Geliştirmeler
Push notification (FCM) · SMS doğrulama · Mobilde rezervasyon & hediye talebi ·
WhatsApp bildirimleri · Embedding tabanlı katalog araması · Arayüz iyileştirme