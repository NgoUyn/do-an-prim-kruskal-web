using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Edges> edges = new List<Edges>();
            Console.Write("Nhap so dinh: ");
            int n = int.Parse(Console.ReadLine());
            Console.Write("Nhap so canh: ");
            int m = int.Parse(Console.ReadLine());
            Console.WriteLine("Nhap lan luot thong tin cac canh (dinh dau, dinh cuoi, trong so): ");
            for (int i =0; i < m; i++)
            {
                Edges e = new Edges();
                e.insertEdges();
                edges.Add(e);
            }
            Console.WriteLine("Do thi la:");
            foreach (var e in edges)
            {
                e.showEdges();
                Console.WriteLine();
            }
            Kruskal kruskal = new Kruskal();
            kruskal.RunKrusKal(edges, n);
            Console.WriteLine();
            Prim prim = new Prim();
            prim.RunPrimDSKe(edges, n, 5);
        }
    }
}
