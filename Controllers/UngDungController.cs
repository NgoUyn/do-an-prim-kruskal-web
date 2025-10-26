using Prim_Kruskal_Web.Models;
using Prim_Kruskal_Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class UngDungController : Controller
    {
        private readonly DataContext db = new DataContext();


        public ActionResult LienTinh()
        {
            try
            {
                var tinhThanhs = db.GetAllTinhThanh();
                return View(tinhThanhs);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu: " + ex.Message;
                return View(new List<object>());
            }
        }


        [HttpGet]
        public ActionResult GetProvinces()
        {
            try
            {
                var tinhThanhs = db.GetAllTinhThanh();
                var provinces = tinhThanhs.Select(t => new
                {
                    id = t.ID,
                    name = t.TEN_TINH,
                    latitude = 0.0,   // FIX: TINH_THANH does not have Latitude
                    longitude = 0.0   // FIX: TINH_THANH does not have Longitude
                }).ToList();

                return Json(new
                {
                    success = true,
                    data = provinces
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi tải danh sách tỉnh: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public ActionResult GetGraphData()
        {
            try
            {
                var allProvinces = db.GetAllTinhThanh();
                var allDistances = db.GetAllKhoangCach();

                var nodes = allProvinces.Select(p => new
                {
                    id = p.ID,
                    name = p.TEN_TINH,
                    latitude = 0.0,   // FIX: TINH_THANH does not have Latitude
                    longitude = 0.0   // FIX: TINH_THANH does not have Longitude
                }).ToList();

                var edges = allDistances.Select(d => new
                {
                    source = d.ID_TINH_A,
                    target = d.ID_TINH_B,
                    distance = d.KHOANG_CACH_VALUE, // FIX: Use correct property name
                    cost = d.KHOANG_CACH_VALUE,     // FIX: Use KHOANG_CACH_VALUE for cost if no COST property exists
                    sourceName = d.TINH_THANH?.TEN_TINH ?? "Unknown",
                    targetName = d.TINH_THANH1?.TEN_TINH ?? "Unknown"
                }).ToList();

                return Json(new
                {
                    success = true,
                    nodes = nodes,
                    edges = edges
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi tải dữ liệu đồ thị: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CalculateOptimalRoute()
        {
            try
            {
                // Đọc JSON body thay vì dùng parameter trực tiếp (tránh lỗi model binding)
                string body;
                using (var reader = new System.IO.StreamReader(Request.InputStream))
                {
                    body = reader.ReadToEnd();
                }

                // Parse JSON
                var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<CalculateRequestDto>(body);
                var selectedProvinces = payload?.selectedProvinces ?? new List<int>();

                if (selectedProvinces.Count < 2)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng chọn ít nhất 2 tỉnh thành"
                    });
                }

                // Load graph từ DB
                var graph = LoadGraph(selectedProvinces);

                if (graph.Nodes.Count < 2)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không đủ dữ liệu để tính toán"
                    });
                }

                // Chạy thuật toán
                var kruskalSvc = new KruskalAlgorithm_Service();
                var primSvc = new PrimAlgorithm_Services();

                var kruskalResult = kruskalSvc.FindMST(graph);
                var primResult = primSvc.FindMST(graph, 0);

                // Kiểm tra tính liên thông
                int expectedEdges = graph.Nodes.Count - 1;
                if (primResult.MSTEdges.Count < expectedEdges || kruskalResult.MSTEdges.Count < expectedEdges)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Đồ thị không liên thông - một số tỉnh không có đường kết nối"
                    });
                }

                // Xác định route tối ưu
                var optimalRoute = DetermineOptimalRoute(primResult, kruskalResult, graph);

                // Tạo DTO
                var primEdgesDto = ConvertEdgesToDto(primResult.MSTEdges);
                var kruskalEdgesDto = ConvertEdgesToDto(kruskalResult.MSTEdges);
                var optimalEdgesDto = ConvertEdgesToDto(optimalRoute.Edges);

                // Tính comparison metrics
                var comparison = new
                {
                    costDifference = Math.Abs(primResult.TotalCost - kruskalResult.TotalCost),
                    timeDifference = Math.Abs(primResult.ExecutionTimeMs - kruskalResult.ExecutionTimeMs),
                    operationDifference = Math.Abs(primResult.StepCount - kruskalResult.StepCount)
                };

                return Json(new
                {
                    success = true,
                    primResult = new
                    {
                        totalCost = primResult.TotalCost,
                        executionTime = primResult.ExecutionTimeMs,
                        operationCount = primResult.StepCount,
                        edgeCount = primResult.MSTEdges.Count,
                        edges = primEdgesDto
                    },
                    kruskalResult = new
                    {
                        totalCost = kruskalResult.TotalCost,
                        executionTime = kruskalResult.ExecutionTimeMs,
                        operationCount = kruskalResult.StepCount,
                        edgeCount = kruskalResult.MSTEdges.Count,
                        edges = kruskalEdgesDto
                    },
                    optimalRoute = new
                    {
                        algorithmName = optimalRoute.AlgorithmName,
                        totalCost = optimalRoute.TotalCost,
                        totalDistance = optimalRoute.TotalDistance,
                        executionTime = optimalRoute.ExecutionTimeMs,
                        routeNames = optimalRoute.RouteNames,
                        edges = optimalEdgesDto,
                        routeVertices = optimalRoute.RouteVertices
                    },
                    comparison = comparison
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }


        private List<object> ConvertEdgesToDto(List<Edge> edges)
        {
            return edges.Select(e => new
            {
                fromId = e.SourceId,
                fromName = e.Src?.Name ?? "Unknown",
                toId = e.DestinationId,
                toName = e.Destination?.Name ?? "Unknown",
                distance = e.KhoangCach ?? 0.0,
                cost = e.Cost ?? 0.0
            }).Cast<object>().ToList();
        }


        private Graph LoadGraph(List<int> selectedProvinces)
        {
            var graph = new Graph();
            var allTinhThanhs = db.GetAllTinhThanh();
            var allDistances = db.GetAllKhoangCach();

            // Thêm nodes
            foreach (var provinceId in selectedProvinces)
            {
                var tinh = allTinhThanhs.FirstOrDefault(t => t.ID == provinceId); // FIX: Use t.ID
                if (tinh != null)
                {
                    var node = new Node
                    {
                        Id = tinh.ID,
                        Name = tinh.TEN_TINH,
                        Latitude = 0.0,   // FIX: TINH_THANH does not have Latitude
                        Longitude = 0.0   // FIX: TINH_THANH does not have Longitude
                    };
                    graph.AddNode(node);
                }
            }

            // Thêm edges giữa các nodes đã chọn
            foreach (var distance in allDistances)
            {
                if (selectedProvinces.Contains(distance.ID_TINH_A) &&
                    selectedProvinces.Contains(distance.ID_TINH_B))
                {
                    graph.AddEdge(
                        distance.ID_TINH_A,
                        distance.ID_TINH_B,
                        distance.KHOANG_CACH_VALUE, // FIX: Use KHOANG_CACH_VALUE for both distance and cost
                        distance.KHOANG_CACH_VALUE  // FIX: Use KHOANG_CACH_VALUE for cost since COST does not exist
                    );
                }
            }

            return graph;
        }

        /// <summary>
        /// Xác định route tối ưu dựa trên so sánh Prim vs Kruskal
        /// FIX: Bổ sung đầy đủ các thuộc tính cho OptimalRoute
        /// </summary>
        private OptimalRoute DetermineOptimalRoute(
            AlgorithmResult primResult,
            AlgorithmResult kruskalResult,
            Graph graph)
        {
            bool primIsOptimal = primResult.TotalCost <= kruskalResult.TotalCost;
            var selectedResult = primIsOptimal ? primResult : kruskalResult;

            var routeVertices = GetRouteVertices(selectedResult.MSTEdges);
            var routeNames = routeVertices
                .Select(id => graph.Nodes.FirstOrDefault(n => n.Id == id)?.Name ?? "N/A")
                .ToList();

            // Tính tổng khoảng cách
            double totalDistance = selectedResult.MSTEdges.Sum(e => e.KhoangCach ?? 0.0);

            return new OptimalRoute
            {
                AlgorithmName = primIsOptimal ? "Prim" : "Kruskal",
                Edges = selectedResult.MSTEdges,
                TotalCost = selectedResult.TotalCost,
                TotalDistance = totalDistance,
                ExecutionTimeMs = selectedResult.ExecutionTimeMs,
                OperationCount = selectedResult.StepCount,
                RouteVertices = routeVertices,
                RouteNames = routeNames
            };
        }

        /// <summary>
        /// Lấy danh sách vertex IDs từ danh sách edges
        /// </summary>
        private List<int> GetRouteVertices(List<Edge> edges)
        {
            var vertices = new HashSet<int>();
            foreach (var edge in edges)
            {
                vertices.Add(edge.SourceId);
                vertices.Add(edge.DestinationId);
            }
            return vertices.OrderBy(v => v).ToList();
        }

        public ActionResult NoiTinh()
        {
            return View();
        }
    }

    /// <summary>
    /// DTO để nhận request JSON từ frontend
    /// </summary>
    public class CalculateRequestDto
    {
        public List<int> selectedProvinces { get; set; }
    }


    internal class OptimalRoute
    {
        public string AlgorithmName { get; set; }
        public List<Edge> Edges { get; set; }
        public double TotalCost { get; set; }
        public double TotalDistance { get; set; }
        public double ExecutionTimeMs { get; set; }
        public int OperationCount { get; set; }
        public List<int> RouteVertices { get; set; }
        public List<string> RouteNames { get; set; }

        public OptimalRoute()
        {
            Edges = new List<Edge>();
            RouteVertices = new List<int>();
            RouteNames = new List<string>();
        }
    }
}