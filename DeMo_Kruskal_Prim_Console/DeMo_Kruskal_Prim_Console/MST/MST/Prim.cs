using ConcurrentPriorityQueue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MST
{
    public class PriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new List<(T item, int priority)>();
        public int Count => elements.Count;
        public void EQueue(T item, int priority)
        {
            elements.Add((item, priority));
            elements = elements.OrderBy(q => q.priority).ToList();
        }
        public T Dqueue()
        {
            var item = elements[0].item;
            elements.RemoveAt(0);
            return item;
        }
    }
    internal class Prim
    {
        public void RunPrim (List<Edges> graph, int N, int startN)
        {
            List<Edges>[] adj = new List<Edges> [N + 1];
            for (int i = 1; i <= N; i++)
                adj[i] = new List<Edges>();
            foreach (var e in graph)
            {
                adj[e.startNode].Add(e);
                adj[e.endNode].Add(e);
            }
            List<Edges> MST = new List<Edges>();
            var pq = new PriorityQueue<(int w, int eN)>();
            pq.EQueue((0, startN), 0);
            int[] key = new int[1000];
            bool[] visited = new bool[1000];
            int[] parent = new int[100];
            for(int i = 1; i <= N; i++)
            {
                key[i] = int.MaxValue;
                visited[i] = false;
                parent[i] = 0;
            }
            key[startN] = 0;
            while (pq.Count > 0)
            {
                var (currentW, u) = pq.Dqueue();
                if (visited[u]) continue;
                visited[u] = true;
                if(parent[u] != 0)
                {
                    Edges e = new Edges(parent[u], u, key[u]);
                    MST.Add(e);
                }
                foreach (var edge in adj[u])
                {
                    int v = edge.startNode == u ? edge.endNode : edge.startNode;
                    if (visited[v]) continue;
                    if (!visited[v] && edge.weight < key[v])
                    {
                        key[v] = edge.weight;
                        pq.EQueue((key[v], v), key[v]);
                        parent[v] = u;
                    }
                }
            }
            Console.WriteLine("Cay bao trum nho nhat la: ");
            for (int i = 0; i < MST.Count; i++)
            {
                MST[i].showEdges();
            }   
            Console.WriteLine("Tong trong so la: {0}", MST.Sum(w => w.weight));
        }
        public void RunPrimDSKe (List<Edges> graph, int N, int start)
        {
            List<Edges>[] adj = new List<Edges>[N + 1];
            for (int i = 1; i <= N; i++)
                adj[i] = new List<Edges>();
            foreach (var e in graph)
            {
                adj[e.startNode].Add(e);
                adj[e.endNode].Add(e);
            }
            List<Edges> MST = new List<Edges>();
            bool[] visited = new bool[100];
            visited[start] = true;
            while (MST.Count < N - 1)
            {
                int min_w = int.MaxValue;
                int X = 0, Y = 0;
                for(int i = 1; i <= N; i++)
                {
                    if (visited[i])
                    {
                        foreach (var edge in adj[i])
                        {
                            int v = edge.startNode == i ? edge.endNode : edge.startNode;
                            if (!visited[v] && edge.weight < min_w)
                            {
                                min_w = edge.weight;
                                X = i;
                                Y = v;
                            }
                        }
                    }
                }
                visited[Y] = true;
                Edges e = new Edges(X, Y, min_w);
                MST.Add(e);
            }
            Console.WriteLine("Cay bao trum nho nhat theo thuật toán Prim la: ");
            for (int i = 0; i < MST.Count; i++)
            {
                MST[i].showEdges();
            }
            Console.WriteLine("Tong trong so la: {0}", MST.Sum(w => w.weight));
        }
    }
}
