using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MST
{
    internal class Edges
    {
        public int startNode { get; set; }
        public int endNode { get; set; }
        public int weight { get; set; }
        public Edges()
        {
            startNode = 0;
            endNode = 0;
            weight = 0;
        }
        public Edges(int startNode, int endNode, int weight)
        {
            this.startNode = startNode;
            this.endNode = endNode;
            this.weight = weight;
        }
        public void insertEdges()
        {
            Console.Write("Nhap dinh dau: ");
            startNode = int.Parse(Console.ReadLine());
            Console.Write("Nhap dinh cuoi: ");
            endNode = int.Parse(Console.ReadLine());
            Console.Write("Nhap trong so: ");
            weight = int.Parse(Console.ReadLine());
        }
        public void showEdges()
        {
            Console.WriteLine("Dinh dau: {0}, Dinh cuoi: {1}, Trong so: {2}", startNode, endNode, weight);
        }
    }
}
