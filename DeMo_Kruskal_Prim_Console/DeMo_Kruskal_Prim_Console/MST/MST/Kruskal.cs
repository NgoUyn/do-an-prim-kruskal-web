using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST
{
    internal class Kruskal
    {
        private int[] parent = new int[100];
        private int[] size = new int[100];
        private void make_set(List<Edges> edges)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                parent[i] = i;
                size[i] = 1;
            }
        }
        private int find(int v)
        {
            if (v == parent[v]) return v;
            return parent[v] = find(parent[v]);
        }
        private bool union (int a, int b)
        {
            a = find(a);
            b = find(b);
            if (a == b) return false;
            if (size[a] < size[b])
            {
                int temp = a;
                a = b;
                b = temp;
            }
            parent[b] = a;
            size[a] += size[b];
            return true;
        }
        public void RunKrusKal (List<Edges> graph, int N)
        {
            make_set(graph);
            List<Edges> MST = new List<Edges>();
            graph = graph.OrderBy(e => e.weight).ToList();
            for(int i =0; i < graph.Count; i++)
            {
                if(MST.Count == N - 1)
                {
                    break;
                }
                if (union(graph[i].startNode, graph[i].endNode))
                {
                    MST.Add(graph[i]);
                }
            }
            Console.WriteLine("Cay bao trum nho nhat theo thuat toan Kruskal la: ");
            for (int i =0; i< MST.Count; i++)
            {
                MST[i].showEdges();
            }
            Console.WriteLine("Tong trong so la: {0}", MST.Sum(w => w.weight));
        }
    }
}
