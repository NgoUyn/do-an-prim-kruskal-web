using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class DisjoinSet // Cây tập hợp rời rạc {tài liệu nói vậy :))} = Check 2 Node có nằm trong cùng 1 Cây con chưa

    {
        private Dictionary<int, int> parent = new Dictionary<int, int>(); // tạo map vùng nhớ cho nó : {key = Id // value = cha} của node trong cây  
        public void MakeSet(List<Node> nodes) // khởi tạo ban đầu mỗi node là riêng biệt để hiểu mỗi đỉnh là 1 cây riêng lẻ
        {
            foreach (var node in nodes)
            {
                parent[node.Id] = node.Id;
            }
        }

        private int Find(int nodeId) // tìm rồi chả về cha gốc của node đó
        {
            if (parent[nodeId] != nodeId)
            {
                parent[nodeId] = Find(parent[nodeId]); // Path compression : nen duong
                
            }return parent[nodeId];
        }

        public void Union(int nodeA, int nodeB) // Gộp nhóm 2 tập lại với nhau
        {
            int rootA = Find(nodeA);
            int rootB = Find(nodeB);
            if (rootA != rootB)
            {
                parent[rootB] = rootA; // Gop nhom
            }
        }

        public bool Connected(int nodeA, int nodeB) // check 2 đỉnh cùng 1 tập (kiểu là 1 cạnh rồi á)
        {
            return Find(nodeA) == Find(nodeB);
        }
    }
}