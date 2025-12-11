using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prim_Kruskal_Web.Services
{
    public class PrimAlgorithm_Services : IPrimAlgorithm_Service
    {
        private const string ALGORITHM_NAME = "Prim's Algorithm (Optimized)";
        private const string TIME_COMPLEXITY = "O(V²)";

        public AlgorithmResult FindMST(Graph graph, int startNodeIndex)
        {
            // 1. Khởi động đồng hồ đo
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = ALGORITHM_NAME,
                TimeComplexity = TIME_COMPLEXITY,
                Steps = new List<AlgorithmStep>(),
                MSTEdges = new List<Edge>()
            };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); return result; }

            // 2. Pre-processing: Chuyển Edge List sang Adjacency List (Mảng các List)
            // Việc này giúp truy xuất O(1) thay vì O(E)
            var adj = new List<Edge>[n];
            for (int i = 0; i < n; i++) adj[i] = new List<Edge>();

            // Map ID -> Index để dùng mảng (nhanh hơn Dictionary)
            var idToIndex = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++) idToIndex[graph.Nodes[i].Id] = i;

            foreach (var edge in graph.Edges)
            {
                if (idToIndex.TryGetValue(edge.SourceId, out int u) && idToIndex.TryGetValue(edge.DestinationId, out int v))
                {
                    adj[u].Add(edge);
                    adj[v].Add(edge);
                }
            }

            // 3. Khởi tạo cấu trúc dữ liệu Prim
            if (startNodeIndex < 0 || startNodeIndex >= n) startNodeIndex = 0;

            double[] key = new double[n];   // Khoảng cách ngắn nhất
            bool[] inMST = new bool[n];     // Đánh dấu đã thăm
            int[] parent = new int[n];      // Lưu vết cha con

            // Khởi tạo giá trị (Dùng vòng lặp for thay vì Enumerable để nhanh hơn)
            for (int i = 0; i < n; i++)
            {
                key[i] = double.MaxValue;
                parent[i] = -1;
            }
            key[startNodeIndex] = 0;

            // --- QUYẾT ĐỊNH CHẾ ĐỘ CHẠY ---
            // Nếu N > 100: Chạy chế độ "Đua tốc độ" (Không ghi Log)
            // Nếu N <= 100: Chạy chế độ "Mô phỏng" (Ghi Log đầy đủ để vẽ hình)
            bool isBenchmarkMode = n > 100;

            // 4. VÒNG LẶP CHÍNH O(V^2)
            for (int count = 0; count < n; count++)
            {
                // Bước 4a: Tìm đỉnh u có key nhỏ nhất chưa vào MST
                // Đây là đoạn O(V) thuần túy
                double minVal = double.MaxValue;
                int u = -1;

                for (int v = 0; v < n; v++)
                {
                    if (!inMST[v] && key[v] < minVal)
                    {
                        minVal = key[v];
                        u = v;
                    }
                }

                if (u == -1) break; // Đồ thị ngắt quãng hoặc xong

                inMST[u] = true;

                // LOGGING (Chỉ chạy khi N nhỏ)
                if (!isBenchmarkMode)
                {
                    // Lưu ý: Đoạn code này rất chậm vì tạo Object mới liên tục
                    result.Steps.Add(new AlgorithmStep
                    {
                        StepNumber = count + 1,
                        Description = $"Chọn đỉnh {u} (W: {minVal:F1})",
                        // Copy danh sách chỉ để hiển thị (Rất tốn RAM/CPU)
                        CurrentMSTEdges = new List<Edge>(result.MSTEdges)
                    });
                }

                // Bước 4b: Cập nhật trọng số cho các đỉnh kề
                // Duyệt qua danh sách kề adj[u] thay vì toàn bộ Edges
                var neighbors = adj[u];
                int neighborsCount = neighbors.Count;

                for (int k = 0; k < neighborsCount; k++)
                {
                    var edge = neighbors[k];
                    // Tìm index của đỉnh kia
                    int vDbId = (edge.SourceId == graph.Nodes[u].Id) ? edge.DestinationId : edge.SourceId;
                    int v = idToIndex[vDbId];

                    if (inMST[v]) continue;

                    double weight = edge.KhoangCach ?? edge.Cost ?? double.MaxValue;

                    if (weight < key[v])
                    {
                        key[v] = weight;
                        parent[v] = u;
                    }
                }
            }

            // 5. Tổng hợp kết quả (Reconstruct MST)
            for (int i = 0; i < n; i++)
            {
                if (parent[i] != -1)
                {
                    int uDbId = graph.Nodes[parent[i]].Id;
                    int vDbId = graph.Nodes[i].Id;

                    // Tìm lại reference cạnh gốc
                    var edge = adj[i].FirstOrDefault(e =>
                        (e.SourceId == uDbId && e.DestinationId == vDbId) ||
                        (e.SourceId == vDbId && e.DestinationId == uDbId));

                    if (edge != null)
                    {
                        result.MSTEdges.Add(edge);
                        result.TotalCost += (edge.Cost ?? edge.KhoangCach ?? 0);
                    }
                }
            }

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = n; // Tổng số bước = số đỉnh
            return result;
        }
    }
}