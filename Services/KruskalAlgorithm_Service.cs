using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Services
{
    public class KruskalAlgorithm_Service
    {

        private const string ALGORITHM_NAME = "Kruskal's Algorithm";
        private const string TIME_COMPLEXITY = "O(E log E)";

        public AlgorithmResult FindMST(Graph graph)
        {
            var sw = Stopwatch.StartNew();
            var result = new AlgorithmResult
            {
                AlgorithmName = ALGORITHM_NAME,
                TimeComplexity = TIME_COMPLEXITY
            };

            int n = graph.Nodes.Count;
            if (n == 0) { sw.Stop(); result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds; return result; }

            // Map DB ids to compact indices
            var idToIndex = new Dictionary<int, int>();
            for (int i = 0; i < n; i++) idToIndex[graph.Nodes[i].Id] = i;

            var sortedEdges = graph.Edges.OrderBy(e => e.KhoangCach ?? double.MaxValue).ToList();
            var uf = new UnionFind(n);
            int stepNumber = 1;

            foreach (var edge in sortedEdges)
            {
                var srcIdx = idToIndex[edge.SourceId];
                var destIdx = idToIndex[edge.DestinationId];

                int rootSource = uf.Find(srcIdx);
                int rootDest = uf.Find(destIdx);

                if (rootSource == rootDest)
                {
                    result.Steps.Add(new AlgorithmStep
                    {
                        StepNumber = stepNumber++,
                        Description = $"Edge {edge.Src?.Name}-{edge.Destination?.Name} (Cost: {edge.KhoangCach}) -> REJECTED (cycle)",
                        CurrentMSTEdges = new List<Edge>(result.MSTEdges)
                    });
                    continue;
                }

                uf.Union(srcIdx, destIdx);
                result.MSTEdges.Add(edge);
                result.TotalCost += (double)(edge.Cost ?? edge.KhoangCach ?? 0);

                result.Steps.Add(new AlgorithmStep
                {
                    StepNumber = stepNumber++,
                    Description = $"Edge {edge.Src?.Name}-{edge.Destination?.Name} (Cost: {edge.Cost ?? edge.KhoangCach}) -> ACCEPTED",
                    CurrentMSTEdges = new List<Edge>(result.MSTEdges)
                });

                if (result.MSTEdges.Count == n - 1) break;
            }

            sw.Stop();
            result.ExecutionTimeMs = sw.Elapsed.TotalMilliseconds;
            result.StepCount = result.Steps.Count;
            return result;
        }
    }
}