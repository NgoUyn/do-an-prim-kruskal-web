using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class DatabaseContext // Lấy dữ liệu từ databse và API, quản lí dữ liệu
    {
        static string strcon = "Data Source = MSI; database = PRIM_KRUSKAL_Tour; User ID = sa;Password = 123456";
        SqlConnection con = new SqlConnection(strcon);
        public List<Graph> dsGraph = new List<Graph>();

        public Graph LoadGraph()
        {
            var graph = new Graph();

            using (SqlConnection con = new SqlConnection(strcon))
            {
                con.Open();

                // Lấy danh sách Node
                SqlDataAdapter daNode = new SqlDataAdapter("SELECT * FROM TINH_THANH", con);
                DataTable dtNode = new DataTable();
                daNode.Fill(dtNode);

                foreach (DataRow dr in dtNode.Rows)
                {
                    var node = new Node(Convert.ToInt32(dr["ID_TINH"]), dr["TEN_TINH"].ToString());
                    graph.AddNode(node);
                }

                // Lấy danh sách Edge
                SqlDataAdapter daEdge = new SqlDataAdapter("SELECT * FROM KHOANG_CACH", con);
                DataTable dtEdge = new DataTable();
                daEdge.Fill(dtEdge);

                foreach (DataRow dr in dtEdge.Rows)
                {
                    int srcId = Convert.ToInt32(dr["ID_TINH_A"]);
                    int desId = Convert.ToInt32(dr["ID_TINH_B"]);
                    int weight = Convert.ToInt32(dr["KHOANG_CACH_KM"]);

                    var src = graph.Nodes.FirstOrDefault(n => n.Id == srcId);
                    var des = graph.Nodes.FirstOrDefault(n => n.Id == desId);

                    if (src != null && des != null)
                        graph.AddEdge(src, des, weight);
                }
            }

            return graph;
        }
    }
}
