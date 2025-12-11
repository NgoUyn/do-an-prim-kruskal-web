using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Prim_Kruskal_Web.Services
{
    public class GeminiService
    {
        private readonly string _apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
        private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            // Bắt buộc dùng TLS 1.2 trở lên cho Google API
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetTourAdviceAsync(string routeDescription, double totalCost)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Lỗi: Chưa cấu hình API Key trong Web.config";

            string prompt = $@"
                Bạn là hướng dẫn viên du lịch vui tính.
                Lộ trình: {routeDescription}
                Tổng chi phí di chuyển: {totalCost:N0} VNĐ.
                
                Yêu cầu:
                1. Nhận xét ngắn gọn về lộ trình và chi phí (Rẻ/Đắt/Hợp lý).
                2. Gợi ý 1 món ăn đặc sản tại điểm đến cuối cùng.
                3. Dùng emoji vui vẻ. Trả lời dưới 100 từ.";

            return await CallGeminiApi(prompt);
        }

        private async Task<string> CallGeminiApi(string textPrompt)
        {
            try
            {
                // Cấu trúc JSON chuẩn của Gemini 1.5
                var requestBody = new
                {
                    contents = new[]
                    {
                        new {
                            parts = new[]
                            {
                                new { text = textPrompt }
                            }
                        }
                    }
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_endpoint}?key={_apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    return $"Lỗi API ({response.StatusCode}): {errorMsg}";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                // Lấy kết quả an toàn
                var resultText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                return resultText ?? "AI không có câu trả lời.";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }
    }
}