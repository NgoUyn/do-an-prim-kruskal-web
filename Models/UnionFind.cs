using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class UnionFind
    {
        private int[] parent;
        private int[] rank;
        private int components;

        public UnionFind(int n)
        {
            parent = new int[n];
            rank = new int[n];
            components = n;

            // Initialize: each element is its own parent
            for (int i = 0; i < n; i++)
            {
                parent[i] = i;
                rank[i] = 0;
            }
        }

        /// <summary>
        /// Find the representative (root) of the set containing x
        /// Uses path compression for optimization
        /// </summary>
        public int Find(int x)
        {
            if (parent[x] != x)
            {
                parent[x] = Find(parent[x]); // Path compression
            }
            return parent[x];
        }

        /// <summary>
        /// Union two sets containing x and y
        /// Returns true if union was performed (elements were in different sets)
        /// Returns false if elements already in same set (cycle detected)
        /// Uses union by rank for optimization
        /// </summary>
        public bool Union(int x, int y)
        {
            int rootX = Find(x);
            int rootY = Find(y);

            if (rootX == rootY)
                return false; // Already in same set - cycle detected

            // Union by rank: attach smaller rank tree under larger rank tree
            if (rank[rootX] < rank[rootY])
            {
                parent[rootX] = rootY;
            }
            else if (rank[rootX] > rank[rootY])
            {
                parent[rootY] = rootX;
            }
            else
            {
                parent[rootY] = rootX;
                rank[rootX]++;
            }

            components--;
            return true;
        }

        public int GetComponentCount() => components;
    }
}