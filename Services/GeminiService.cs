using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Prim_Kruskal_Web.Services
{
    public class GeminiService
    {
        // ⚠️ QUAN TRỌNG: Thay API Key của bạn vào đây
        private readonly string _apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
        // Sử dụng model Gemini 1.5 Flash cho tốc độ phản hồi nhanh nhất
        private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Gửi thông tin lộ trình và chi phí để xin tư vấn từ AI
        /// </summary>
        public async Task<string> GetTourAdviceAsync(string routeDescription, double totalCost)
        {
            // 1. Tạo Prompt (Câu lệnh nhắc)
            string prompt = $@"
                Đóng vai là một hướng dẫn viên du lịch chuyên nghiệp và hài hước.
                Hãy nhận xét về lộ trình du lịch sau đây:
                - Các điểm đến: {routeDescription}
                - Tổng chi phí dự kiến: {totalCost:N0} VNĐ.

                Yêu cầu:
                1. Nhận xét xem mức giá này có hợp lý không (Rẻ/Đắt/Trung bình).
                2. Gợi ý thêm 1 món ăn đặc sản hoặc 1 hoạt động thú vị nên thử trên cung đường này.
                3. Trả lời ngắn gọn dưới 150 từ, dùng emotion icon vui vẻ.";

            return await CallGeminiApi(prompt);
        }

        /// <summary>
        /// Hàm xử lý gọi API gốc
        /// </summary>
        private async Task<string> CallGeminiApi(string textPrompt)
        {
            try
            {
                // Cấu trúc JSON body theo chuẩn Google AI Studio
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

                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Gửi Request POST
                var response = await _httpClient.PostAsync($"{_endpoint}?key={_apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Lỗi API ({response.StatusCode}): Không thể kết nối đến Gemini.";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);

                // Parse kết quả trả về
                // Cấu trúc: candidates[0].content.parts[0].text
                var resultText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                return resultText ?? "AI không trả lời được lúc này.";
            }
            catch (Exception ex)
            {
                return $"Lỗi hệ thống: {ex.Message}";
            }
        }
    }
}