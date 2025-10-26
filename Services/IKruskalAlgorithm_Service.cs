using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim_Kruskal_Web.Services
{
    internal interface IKruskalAlgorithm_Service
    {
        AlgorithmResult FindMST(Graph graph);
    }
}
