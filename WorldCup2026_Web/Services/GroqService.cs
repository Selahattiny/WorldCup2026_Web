using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace WorldCup2026_Web.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GroqSettings:ApiKey"] ?? "";
        }

        public async Task<string> GetMatchAnalysisAsync(string homeTeam, string awayTeam)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "Groq API Key bulunamadı. Lütfen appsettings.json dosyasını kontrol edin.";

            var url = "https://api.groq.com/openai/v1/chat/completions";

            var prompt = $"Sen profesyonel bir futbol analistisin. 2026 Dünya Kupası'nda oynanacak olan {homeTeam} - {awayTeam} maçı hakkında çok kısa, heyecanlı ve vurucu bir analiz yap. Takımların güç dengesini yorumla ve cümlenin en sonunda mutlaka köşeli parantez içinde [Meksika %55 - %45 G.Afrika] tarzında bir yüzde tahmini ver. Maksimum 3 cümle olsun.";

            // Model adını en güncel ve kararlı sürüm olan "llama-3.1-8b-instant" ile güncelledik
            var requestBody = new GroqRequest
            {
                Model = "llama-3.1-8b-instant",
                Messages = new List<GroqMessage>
                {
                    new GroqMessage { Role = "user", Content = prompt }
                },
                Temperature = 0.7
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic result = JsonConvert.DeserializeObject(responseContent)!;
                    return result.choices[0].message.content.ToString();
                }

                return $"Groq API Hatası: {response.StatusCode} - {responseContent}";
            }
            catch (Exception ex)
            {
                return $"Yapay Zeka Bağlantı Hatası: {ex.Message}";
            }
        }
    }

    public class GroqRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonProperty("messages")]
        public List<GroqMessage> Messages { get; set; } = new();

        [JsonProperty("temperature")]
        public double Temperature { get; set; }
    }

    public class GroqMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }
}