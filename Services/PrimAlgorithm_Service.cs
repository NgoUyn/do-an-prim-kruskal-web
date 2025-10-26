using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Services
{
    public class PrimAlgorithm_Services : IPrimAlgorithm_Service
    {
        private const string ALGORITHM_NAME = "Prim's Algorithm";
        private const string TIME_COMPLEXITY = "O(V²)";

        // startNodeId = index into graph.Nodes list (not DB id). If caller provides -1, we'll start at index 0.

        public AlgorithmResult FindMST(Graph graph, int startNodeIndex)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = ALGORITHM_NAME,
                TimeComplexity = TIME_COMPLEXITY
            };

            int n = graph.Nodes.Count;
            if (n == 0)
            {
                sw.Stop();
                result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
                return result;
            }

            // Khởi tạo
            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < n; i++)
                idToIndex[graph.Nodes[i].Id] = i;

            if (startNodeIndex < 0 || startNodeIndex >= n)
                startNodeIndex = 0;

            double[] key = new double[n];
            bool[] inMST = new bool[n];
            int[] parent = new int[n];

            for (int i = 0; i < n; i++)
            {
                key[i] = double.MaxValue;
                parent[i] = -1;
            }
            key[startNodeIndex] = 0;

            int stepCount = 0;

            // Lặp V lần
            for (int count = 0; count < n; count++)
            {
                int u = FindMinKeyNode(key, inMST, n);
                if (u == -1) break;

                inMST[u] = true;
                stepCount++;

                // Thêm bước
                result.Steps.Add(new AlgorithmStep
                {
                    StepNumber = stepCount,
                    Description = $"Chọn đỉnh '{graph.Nodes[u].Name}' (Chi phí: {key[u]:F2})",
                    VisitedNodes = GetVisitedNodes(inMST),
                    CurrentMSTEdges = new List<Edge>(result.MSTEdges)
                });

                // Duyệt tất cả cạnh từ u
                foreach (var edge in graph.Edges)
                {
                    int srcIdx = idToIndex[edge.SourceId];
                    int destIdx = idToIndex[edge.DestinationId];
                    int v = -1;

                    if (srcIdx == u && !inMST[destIdx])
                        v = destIdx;
                    else if (destIdx == u && !inMST[srcIdx])
                        v = srcIdx;

                    if (v != -1)
                    {
                        double weight = edge.KhoangCach ?? double.MaxValue;
                        if (weight < key[v])
                        {
                            key[v] = weight;
                            parent[v] = u;
                            stepCount++;
                        }
                    }
                }
            }

            // Xây dựng MST từ parent[]
            for (int i = 0; i < n; i++)
            {
                if (parent[i] != -1)
                {
                    int uDbId = graph.Nodes[parent[i]].Id;
                    int vDbId = graph.Nodes[i].Id;
                    var edge = FindEdgeBetween(graph, uDbId, vDbId);

                    if (edge != null)
                    {
                        result.MSTEdges.Add(edge);
                        double cost = edge.Cost ?? edge.KhoangCach ?? 0;
                        result.TotalCost += cost;
                    }
                }
            }

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = stepCount;
            return result;
        }

        private int FindMinKeyNode(double[] key, bool[] inMST, int n)
        {
            double min = double.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < n; i++)
            {
                if (!inMST[i] && key[i] < min)
                {
                    min = key[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        private List<int> GetVisitedNodes(bool[] inMST)
        {
            var visited = new List<int>();
            for (int i = 0; i < inMST.Length; i++)
                if (inMST[i])
                    visited.Add(i);
            return visited;
        }

        private Edge FindEdgeBetween(Graph graph, int uDbId, int vDbId)
        {
            return graph.Edges.FirstOrDefault(e =>
                (e.SourceId == uDbId && e.DestinationId == vDbId) ||
                (e.SourceId == vDbId && e.DestinationId == uDbId));
        }
    }
}