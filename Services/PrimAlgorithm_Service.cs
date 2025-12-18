using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prim_Kruskal_Web.Services
{
    public class PrimAlgorithm_Services : IPrimAlgorithm_Service
    {
        // =================================================================================
        // 1. PRIM STANDARD: DANH SÁCH KỀ (ADJACENCY LIST) + MẢNG (ARRAY)
        // Độ phức tạp: O(V^2) - Tốt cho đồ thị Dày (Dense)
        // =================================================================================
        public AlgorithmResult FindMST(Graph graph, int startNodeId)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = "Prim (Standard - List)",
                TimeComplexity = "O(V²)",
                Steps = new List<AlgorithmStep>(),
                MSTEdges = new List<Edge>()
            };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); return result; }

            // 1. Pre-processing: Chuyển Edge List sang Adjacency List
            var adj = new List<Edge>[n];
            for (int i = 0; i < n; i++) adj[i] = new List<Edge>();

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

            // 2. Khởi tạo
            double[] key = new double[n];   // Khoảng cách ngắn nhất
            bool[] inMST = new bool[n];     // Đánh dấu đã thăm
            int[] parent = new int[n];      // Lưu vết cha con

            for (int i = 0; i < n; i++) { key[i] = double.MaxValue; parent[i] = -1; }

            // Tìm index bắt đầu
            int startIndex = idToIndex.ContainsKey(startNodeId) ? idToIndex[startNodeId] : 0;
            key[startIndex] = 0;

            // 3. Vòng lặp chính
            for (int count = 0; count < n; count++)
            {
                // 3a. Tìm đỉnh u có key nhỏ nhất (Linear Search - O(V))
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

                if (u == -1) break;
                inMST[u] = true;

                // 3b. Cập nhật lân cận
                foreach (var edge in adj[u])
                {
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

            // 4. Tổng hợp kết quả
            ReconstructMST(result, graph, parent, adj);

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = n;
            return result;
        }

        // =================================================================================
        // 2. PRIM MATRIX: MA TRẬN KỀ (ADJACENCY MATRIX)
        // Độ phức tạp: O(V^2) - Nhanh nhất cho đồ thị siêu dày nhưng TỐN RAM (O(V^2))
        // =================================================================================
        public AlgorithmResult FindMST_Matrix(Graph graph, int startNodeId)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = "Prim (Adjacency Matrix)",
                TimeComplexity = "O(V²) - High Memory",
                Steps = new List<AlgorithmStep>(),
                MSTEdges = new List<Edge>()
            };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); return result; }

            // 1. Tạo Ma trận O(V^2) RAM
            double[,] matrix = new double[n, n];
            var idToIndex = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++) idToIndex[graph.Nodes[i].Id] = i;

            // Khởi tạo MaxValue
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    matrix[i, j] = double.MaxValue;

            // Điền trọng số
            foreach (var edge in graph.Edges)
            {
                if (idToIndex.TryGetValue(edge.SourceId, out int u) && idToIndex.TryGetValue(edge.DestinationId, out int v))
                {
                    double w = edge.KhoangCach ?? edge.Cost ?? double.MaxValue;
                    // Vô hướng nên điền 2 chiều
                    matrix[u, v] = w;
                    matrix[v, u] = w;
                }
            }

            // 2. Thuật toán Prim trên Ma trận
            double[] key = new double[n];
            bool[] inMST = new bool[n];
            int[] parent = new int[n];
            for (int i = 0; i < n; i++) { key[i] = double.MaxValue; parent[i] = -1; }

            int startIndex = idToIndex.ContainsKey(startNodeId) ? idToIndex[startNodeId] : 0;
            key[startIndex] = 0;

            for (int count = 0; count < n; count++)
            {
                // Tìm Min (O(V))
                double min = double.MaxValue;
                int u = -1;
                for (int v = 0; v < n; v++)
                    if (!inMST[v] && key[v] < min) { min = key[v]; u = v; }

                if (u == -1) break;
                inMST[u] = true;

                // Duyệt lân cận trên Ma trận (O(V))
                for (int v = 0; v < n; v++)
                {
                    if (matrix[u, v] != double.MaxValue && !inMST[v] && matrix[u, v] < key[v])
                    {
                        key[v] = matrix[u, v];
                        parent[v] = u;
                    }
                }
            }

            // Tái tạo kết quả (Cần danh sách kề tạm để tìm lại object Edge gốc)
            // (Đoạn này hơi tốn chút time nhưng cần thiết để trả về đúng Edge object)
            var tempAdj = BuildAdjList(graph, n, idToIndex);
            ReconstructMST(result, graph, parent, tempAdj);

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = n;
            return result;
        }

        // =================================================================================
        // 3. PRIM HEAP: BINARY HEAP (PRIORITY QUEUE)
        // Độ phức tạp: O(E log V) - Tối ưu cho đồ thị THƯA (Thực tế)
        // =================================================================================
        public AlgorithmResult FindMST_Heap(Graph graph, int startNodeId)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = "Prim (Binary Heap)",
                TimeComplexity = "O(E log V)",
                Steps = new List<AlgorithmStep>(),
                MSTEdges = new List<Edge>()
            };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); return result; }

            // 1. Build Adj List
            var idToIndex = new Dictionary<int, int>(n);
            for (int i = 0; i < n; i++) idToIndex[graph.Nodes[i].Id] = i;
            var adj = BuildAdjList(graph, n, idToIndex);

            // 2. Init Heap & Visited
            bool[] visited = new bool[n];
            var pq = new MinHeap(); // Sử dụng Class MinHeap bạn đã thêm

            int startIndex = idToIndex.ContainsKey(startNodeId) ? idToIndex[startNodeId] : 0;

            // Add node đầu tiên: Trọng số 0, Index, Cha = -1
            pq.Add(new HeapNode { Weight = 0, NodeIndex = startIndex, ParentIndex = -1 });

            int edgesCount = 0;

            // 3. Vòng lặp Heap
            while (pq.Count > 0)
            {
                var minNode = pq.ExtractMin();
                int u = minNode.NodeIndex;

                if (visited[u]) continue;
                visited[u] = true;

                // Nếu có cha (không phải đỉnh đầu), thêm cạnh vào MST
                if (minNode.ParentIndex != -1)
                {
                    // Tìm cạnh gốc nối (Parent -> u)
                    var edgeObj = FindEdge(adj, minNode.ParentIndex, u, idToIndex, graph.Nodes);
                    if (edgeObj != null)
                    {
                        result.MSTEdges.Add(edgeObj);
                        result.TotalCost += minNode.Weight;
                        edgesCount++;
                    }
                }

                if (edgesCount == n - 1) break;

                // Duyệt lân cận
                foreach (var edge in adj[u])
                {
                    int vDbId = (edge.SourceId == graph.Nodes[u].Id) ? edge.DestinationId : edge.SourceId;
                    int v = idToIndex[vDbId];

                    if (!visited[v])
                    {
                        double w = edge.KhoangCach ?? edge.Cost ?? 0;
                        pq.Add(new HeapNode { Weight = w, NodeIndex = v, ParentIndex = u });
                    }
                }
            }

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = edgesCount;
            return result;
        }

        // =================================================================================
        // CÁC HÀM BỔ TRỢ (HELPER METHODS)
        // =================================================================================

        private List<Edge>[] BuildAdjList(Graph graph, int n, Dictionary<int, int> idToIndex)
        {
            var adj = new List<Edge>[n];
            for (int i = 0; i < n; i++) adj[i] = new List<Edge>();
            foreach (var edge in graph.Edges)
            {
                if (idToIndex.TryGetValue(edge.SourceId, out int u) && idToIndex.TryGetValue(edge.DestinationId, out int v))
                {
                    adj[u].Add(edge);
                    adj[v].Add(edge);
                }
            }
            return adj;
        }

        private Edge FindEdge(List<Edge>[] adj, int uIndex, int vIndex, Dictionary<int, int> idToIndex, List<Node> nodes)
        {
            int uId = nodes[uIndex].Id;
            int vId = nodes[vIndex].Id;
            return adj[uIndex].FirstOrDefault(e =>
                (e.SourceId == uId && e.DestinationId == vId) ||
                (e.SourceId == vId && e.DestinationId == uId));
        }

        private void ReconstructMST(AlgorithmResult result, Graph graph, int[] parent, List<Edge>[] adj)
        {
            int n = graph.Nodes.Count;
            for (int i = 0; i < n; i++)
            {
                if (parent[i] != -1)
                {
                    int uDbId = graph.Nodes[parent[i]].Id;
                    int vDbId = graph.Nodes[i].Id;

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
        }
    }
}