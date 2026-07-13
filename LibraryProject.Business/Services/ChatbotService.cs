using System.Text;
using System.Text.Json;
using LibraryProject.Business.Interfaces;
using LibraryProject.Model.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryProject.Business.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly ILogger<ChatbotService> _logger;
        private static readonly HttpClient _http = new();

        public ChatbotService(IUnitOfWork uow, IConfiguration config,
            ILogger<ChatbotService> logger)
        {
            _uow = uow;
            _config = config;
            _logger = logger;
        }

        public async Task<string> AskAsync(string question)
        {
            //mini rag ım

            var books = await _uow.Books.FindAsync(b => b.IsActive);
            var catalog = string.Join("\n",
                books.Select(b => $"- {b.Title} ({b.Author})"));

            var systemPrompt = $"""
                Sen bir üniversite kütüphanesinin yardımcı asistanısın. Sadece Türkçe cevap ver.
                Kısa, net ve samimi ol. Yalnızca kütüphaneyle ilgili sorulara cevap ver;
                alakasız konularda kibarca kütüphane konularına yönlendir.

                KÜTÜPHANE KURALLARI:
                - Rezervasyon: günde en fazla 6 saat, kişi başı aynı anda en çok 3 aktif rezervasyon.
                - Rezervasyon başlangıcından itibaren 30 dk içinde giriş yapılmazsa otomatik iptal edilir.
                - 3 kez gelmeme (no-show) durumunda 1 hafta rezervasyon yasağı uygulanır.
                - Mola hakkı toplam 1,5 saattir, parçalara bölünebilir; 5 dk tolerans vardır, aşılırsa rezervasyon iptal olur.
                - Giriş/çıkış/mola işlemleri girişteki QR kod ekranı ve mobil uygulama ile yapılır.
                - Kitap ödünç: kişi başı en fazla 5 kitap, süre 20 gündür. Geç iadede 1 ay ödünç alma yasağı uygulanır.
                - Ödünç işlemleri kütüphane bankosunda görevli aracılığıyla yapılır; ayırtma yoktur.
                - Sadakat sistemi: kütüphanede geçirilen her saat için 10 puan kazanılır,
                  haftada 5 farklı gün gelenlere 50 bonus puan verilir. Puanlarla hediye talep edilir, admin onaylar.
                - Kütüphane 2 katlıdır; koltuk seçimi web üzerindeki krokiden yapılır.
                - Gürültü şikayeti için uygulamadaki "Gürültü İhbar Et" butonu kullanılır.

                KÜTÜPHANEDEKİ KİTAPLAR:
                {catalog}

                Kitap önerisi istenirse SADECE yukarıdaki katalogdan öner. Katalogda olmayan
                kitaplar için "maalesef koleksiyonumuzda yok" de.
                """;

            var payload = new
            {
                system_instruction = new { parts = new[] { new { text = systemPrompt } } },
                contents = new[]
                {
                    new { role = "user", parts = new[] { new { text = question } } }
                },
                generationConfig = new { maxOutputTokens = 500 }
            };

            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_config["Gemini:Model"]}:generateContent?key={_config["Gemini:ApiKey"]}";

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    var response = await _http.PostAsync(url,
                        new StringContent(JsonSerializer.Serialize(payload),
                            Encoding.UTF8, "application/json"));
                    var body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(body);
                        return doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString() ?? "Cevap alınamadı.";
                    }

                    // 503 gecici yogunluk varsa tekrar dene
                    if ((int)response.StatusCode == 503 && attempt < 3)
                    {
                        _logger.LogWarning("Gemini yoğun (deneme {Attempt}), tekrar deneniyor", attempt);
                        await Task.Delay(2000 * attempt); 
                        continue;
                    }

                    _logger.LogError("Gemini hatası: {Status} {Body}", response.StatusCode, body);
                    return "Asistan şu an çok yoğun, lütfen birazdan tekrar deneyin. 🙏";
                }

                return "Asistan şu an çok yoğun, lütfen birazdan tekrar deneyin. 🙏";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Chatbot çağrısı başarısız");
                return "Şu an cevap veremiyorum, lütfen daha sonra tekrar deneyin.";
            }
        }
    }
}