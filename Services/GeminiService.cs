using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Prim_Kruskal_Web.Services
{
    public class GeminiService
    {
        // Lấy API Key từ Web.config (sẽ tự động đọc từ Secrets.config)
        private readonly string _apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];

        private readonly string _endpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";
        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            _httpClient = new HttpClient();
            // Timeout 30 giây để tránh treo ứng dụng nếu mạng chậm
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // =================================================================================
        // 1. TƯ VẤN DU LỊCH (Dùng cho UngDungController)
        // =================================================================================
        public async Task<string> GetTourAdviceAsync(string routeDescription, double totalCost)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "Lỗi: Chưa cấu hình API Key (Kiểm tra file Secrets.config).";

            string prompt = $@"
                Bạn là hướng dẫn viên du lịch chuyên nghiệp.
                Lộ trình: {routeDescription}
                Tổng chi phí: {totalCost:N0} VNĐ.
                
                Yêu cầu:
                1. Nhận xét ngắn gọn về lộ trình này (có hợp lý không?).
                2. Gợi ý 1 món ăn đặc sản tại điểm đến.
                3. Trả lời ngắn gọn, thân thiện.";

            return await CallGeminiApi(prompt);
        }

        // =================================================================================
        // 2. PHÂN TÍCH MÔ PHỎNG (PRIM vs KRUSKAL - Chạy 1 lần)
        // =================================================================================
        public async Task<string> AnalyzeSimulation(string primName, double primTime, int primSteps, long primTheory, double kruskalTime, int kruskalSteps, long kruskalTheory, int V, int E)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "⚠️ Chưa cấu hình API Key.";

            string winner = Math.Abs(primTime - kruskalTime) < 0.1 ? "Hòa" : (primTime < kruskalTime ? "Prim thắng" : "Kruskal thắng");

            // Tính mật độ đồ thị để AI phân tích chính xác hơn
            double density = (V > 1) ? (double)E / (V * (V - 1) / 2) * 100 : 0;

            string prompt = $@"
                Đóng vai chuyên gia giải thuật (Algorithm Expert). Phân tích kết quả chạy thực tế:
                - Đồ thị: {V} đỉnh, {E} cạnh (Mật độ {density:F1}%).
                - Prim ({primName}): {primTime:F3}ms (Score {primTheory}).
                - Kruskal: {kruskalTime:F3}ms (Score {kruskalTheory}).
                -> Kết quả thực tế: {winner}.
                
                Yêu cầu (Markdown ngắn gọn): 
                1. Giải thích tại sao {winner} trong trường hợp cụ thể này? (Gợi ý: Dựa vào số cạnh E và cấu trúc dữ liệu).
                2. Nhận xét về sự tương quan giữa điểm lý thuyết (Big-O) và thời gian thực tế.
            ";
            return await CallGeminiApi(prompt);
        }

        // =================================================================================
        // 3. PHÂN TÍCH BENCHMARK (So sánh hiệu năng khi N tăng)
        // =================================================================================
        public async Task<string> AnalyzeBenchmark(int startN, int endN, string mode, List<double> primTimes, List<double> kruskalTimes)
        {
            if (string.IsNullOrEmpty(_apiKey)) return "⚠️ Chưa cấu hình API Key.";

            double avgP = primTimes.Count > 0 ? primTimes.Average() : 0;
            double avgK = kruskalTimes.Count > 0 ? kruskalTimes.Average() : 0;

            string prompt = $@"
                Đóng vai kỹ sư hiệu năng (Performance Engineer). Phân tích Stress Test từ N={startN} đến {endN} ({mode}):
                - TB Prim: {avgP:F2}ms.
                - TB Kruskal: {avgK:F2}ms.
                
                Yêu cầu (Markdown): 
                1. Phân tích xu hướng tăng trưởng thời gian khi N tăng (Scalability).
                2. Thuật toán nào phù hợp hơn cho Big Data trong trường hợp này? Tại sao?
            ";
            return await CallGeminiApi(prompt);
        }

        // =================================================================================
        // HELPER: GỌI API GEMINI (Xử lý lỗi & JSON)
        // =================================================================================
        private async Task<string> CallGeminiApi(string textPrompt)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = textPrompt } } }
                    }
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                // Gọi API Google
                var response = await _httpClient.PostAsync($"{_endpoint}?key={_apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    return $"⚠️ **Lỗi API Gemini ({response.StatusCode})**: {errorMsg}. Vui lòng kiểm tra lại Key hoặc Model.";
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(responseString);
                var resultText = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                return resultText ?? "🤖 AI đang suy nghĩ nhưng không có phản hồi.";
            }
            catch (Exception ex)
            {
                return $"⚠️ **Lỗi Hệ Thống**: {ex.Message}";
            }
        }
    }
}