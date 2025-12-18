using System;
using System.Collections.Generic;

namespace Prim_Kruskal_Web.Models
{
    public class HeapNode : IComparable<HeapNode>
    {
        public double Weight { get; set; }
        public int NodeIndex { get; set; }
        public int ParentIndex { get; set; } // Để truy vết vẽ đường
        public int CompareTo(HeapNode other) => Weight.CompareTo(other.Weight);
    }

    public class MinHeap
    {
        private List<HeapNode> elements = new List<HeapNode>();
        public int Count => elements.Count;

        public void Add(HeapNode item)
        {
            elements.Add(item);
            HeapifyUp(elements.Count - 1);
        }

        public HeapNode ExtractMin()
        {
            if (elements.Count == 0) return null;
            var min = elements[0];
            var last = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            if (elements.Count > 0)
            {
                elements[0] = last;
                HeapifyDown(0);
            }
            return min;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (elements[index].CompareTo(elements[parent]) >= 0) break;
                Swap(index, parent);
                index = parent;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;
                if (left < elements.Count && elements[left].CompareTo(elements[smallest]) < 0) smallest = left;
                if (right < elements.Count && elements[right].CompareTo(elements[smallest]) < 0) smallest = right;
                if (smallest == index) break;
                Swap(index, smallest);
                index = smallest;
            }
        }
        private void Swap(int i, int j) { var temp = elements[i]; elements[i] = elements[j]; elements[j] = temp; }
    }
}