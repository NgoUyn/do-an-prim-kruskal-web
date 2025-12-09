using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prim_Kruskal_Web.Services
{
    public class DijkstraAlgorithm_Service
    {
        // Dijkstra tìm đường đi ngắn nhất từ nguồn (Shortest Path Tree)
        // Nó có cơ chế "loang" khá giống Prim nhưng dựa trên tổng khoảng cách tích lũy
        public AlgorithmResult FindShortestPathTree(Graph graph, int startNodeIndex = 0)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult { AlgorithmName = "Dijkstra", TimeComplexity = "O(E + V log V)" };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); return result; }

            // Map ID
            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < n; i++) idToIndex[graph.Nodes[i].Id] = i;

            double[] dist = new double[n];
            bool[] visited = new bool[n];
            int[] parent = new int[n];

            for (int i = 0; i < n; i++) { dist[i] = double.MaxValue; parent[i] = -1; }
            dist[startNodeIndex] = 0;

            for (int count = 0; count < n - 1; count++)
            {
                int u = MinDistance(dist, visited, n);
                if (u == -1) break;
                visited[u] = true;

                // Tìm các cạnh nối với u
                var edges = graph.Edges.Where(e =>
                    (idToIndex[e.SourceId] == u) || (idToIndex[e.DestinationId] == u)
                ).ToList();

                foreach (var edge in edges)
                {
                    int uIdx = idToIndex[edge.SourceId] == u ? idToIndex[edge.SourceId] : idToIndex[edge.DestinationId];
                    int vIdx = idToIndex[edge.SourceId] == u ? idToIndex[edge.DestinationId] : idToIndex[edge.SourceId];

                    if (!visited[vIdx] && dist[uIdx] != double.MaxValue
                        && dist[uIdx] + edge.Weight < dist[vIdx])
                    {
                        dist[vIdx] = dist[uIdx] + edge.Weight;
                        parent[vIdx] = uIdx;
                    }
                }
            }

            // Reconstruct Tree
            for (int i = 0; i < n; i++)
            {
                if (parent[i] != -1)
                {
                    var uNode = graph.Nodes[parent[i]];
                    var vNode = graph.Nodes[i];
                    result.MSTEdges.Add(new Edge(uNode, vNode, dist[i] - dist[parent[i]])); // Store edge
                    result.TotalCost += (dist[i] - dist[parent[i]]);
                }
            }

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            return result;
        }

        private int MinDistance(double[] dist, bool[] visited, int n)
        {
            double min = double.MaxValue;
            int minIndex = -1;
            for (int v = 0; v < n; v++)
                if (!visited[v] && dist[v] <= min) { min = dist[v]; minIndex = v; }
            return minIndex;
        }
    }
}