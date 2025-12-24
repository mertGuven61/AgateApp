using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgateApp.Services
{
    public class GroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GroqService> _logger;
        private readonly string _apiUrl = "https://api.groq.com/openai/v1/chat/completions";

        public GroqService(HttpClient httpClient, IConfiguration configuration, ILogger<GroqService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"] ?? throw new InvalidOperationException("Groq API key not found in configuration.");
            _logger = logger;
        }

        public async Task<string> GenerateAdvertDescriptionAsync(Models.Advert advert)
        {
            try
            {
                _logger.LogInformation("Groq API çağrısı başlatılıyor. Reklam ID: {AdvertId}", advert.Id);

                // Reklam bilgilerini topla
                var advertInfo = new StringBuilder();
                advertInfo.AppendLine($"Reklam Başlığı: {advert.Title}");
                advertInfo.AppendLine($"Medya Kanalı: {advert.MediaChannel ?? "Belirtilmemiş"}");
                advertInfo.AppendLine($"Üretim Durumu: {advert.ProductionStatus}");
                advertInfo.AppendLine($"Planlanan Başlangıç Tarihi: {advert.ScheduledRunDateStart:dd.MM.yyyy}");
                advertInfo.AppendLine($"Planlanan Bitiş Tarihi: {advert.ScheduledRunDateEnd:dd.MM.yyyy}");
                advertInfo.AppendLine($"Maliyet: {advert.Cost:C}");

                if (advert.Campaign != null)
                {
                    advertInfo.AppendLine($"Kampanya: {advert.Campaign.Title}");
                }

                // Groq API için prompt hazırla
                var prompt = $@"Aşağıdaki reklam bilgilerini analiz ederek, profesyonel ve detaylı bir açıklama yaz. 
Açıklama Türkçe olmalı ve reklamın özelliklerini, hedefini ve önemini vurgulamalı.

Reklam Bilgileri:
{advertInfo}

Lütfen bu reklam için kısa, öz ve profesyonel bir açıklama oluştur (2-3 paragraf).";

                var requestBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 500
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                _logger.LogInformation("Groq API'ye istek gönderiliyor...");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Groq API yanıtı alındı. Status: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Groq API hatası: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    throw new Exception($"API Hatası ({response.StatusCode}): {responseContent}");
                }

                var responseJson = JsonDocument.Parse(responseContent);

                // Groq API yanıtından metni çıkar (OpenAI uyumlu format)
                if (responseJson.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    var choice = choices[0];
                    if (choice.TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentProp))
                    {
                        var generatedText = contentProp.GetString();
                        _logger.LogInformation("AI açıklama başarıyla oluşturuldu. Uzunluk: {Length}", generatedText?.Length ?? 0);
                        return generatedText ?? "Açıklama oluşturulamadı.";
                    }
                }

                // Eğer beklenen format yoksa, hata mesajını kontrol et
                if (responseJson.RootElement.TryGetProperty("error", out var error))
                {
                    var errorMessage = error.TryGetProperty("message", out var msg)
                        ? msg.GetString()
                        : "Bilinmeyen API hatası";
                    _logger.LogError("Groq API hatası: {Error}", errorMessage);
                    throw new Exception($"Groq API Hatası: {errorMessage}");
                }

                _logger.LogWarning("API yanıt formatı beklenen formatta değil. Yanıt: {Response}", responseContent);
                return "Açıklama oluşturulamadı. API yanıt formatı beklenen formatta değil.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ağ hatası oluştu");
                throw new Exception($"Ağ hatası: {ex.Message}");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parse hatası oluştu");
                throw new Exception($"JSON parse hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI açıklama oluşturulurken hata oluştu");
                throw;
            }
        }
    }
}
