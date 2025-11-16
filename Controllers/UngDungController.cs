using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Data.Entity; // Cần thêm namespace này cho .ToListAsync() và .FirstOrDefaultAsync()

namespace Prim_Kruskal_Web.Controllers
{
    public class UngDungController : Controller
    {
        // 1. Sửa lỗi DataContext: Sử dụng Dependency Injection
        private readonly DataContext _db;

        // 2. Sửa lỗi HttpClient: Sử dụng instance static, thread-safe
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(12);
        private static readonly string UserAgent = "PrimKruskalWeb/1.0 (+contact: app@example.com)";
        private static readonly HttpClient _httpClient = CreateStaticHttpClient();

        // Constructor để nhận DataContext được tiêm vào
        public UngDungController(DataContext dataContext)
        {
            _db = dataContext;
        }

        static UngDungController()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        // Đổi tên thành CreateStaticHttpClient và chỉ được gọi một lần
        private static HttpClient CreateStaticHttpClient()
        {
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            var client = new HttpClient(handler) { Timeout = HttpTimeout };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        [HttpGet] public ActionResult NoiTinh() => View();
        [HttpGet] public ActionResult LienTinh() => View();

        [HttpGet]
        public ActionResult GetProvinces()
        {
            try
            {
                // Giả sử GetAllTinhThanh() là phương thức tùy chỉnh của bạn.
                // Nếu nó chỉ là db.TINH_THANH.ToList(), hãy cân nhắc chuyển action này sang async
                // và gọi await _db.TINH_THANH.ToListAsync();
                var list = _db.GetAllTinhThanh();
                if (list == null || !list.Any())
                    return Json(new { success = false, message = "Không có dữ liệu tỉnh" }, JsonRequestBehavior.AllowGet);
                return Json(new { success = true, data = list.Select(p => new { id = p.ID, name = p.TEN_TINH }) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ===== Helpers =====
        private double[] GetBBox(int provinceID, int index)
        {
            // Cân nhắc chuyển logic "hard-code" này vào database trong bảng TINH_THANH
            switch (provinceID)
            {
                case 1: return index == 0 ? new[] { 106.33, 10.36 } : new[] { 107.03, 11.19 }; // HCM
                // ... các case khác ...
                default: return new[] { 0.0, 0.0 }; // unknown
            }
        }
        private bool IsZero(double[] p) => p == null || p.Length < 2 || (p[0] == 0 && p[1] == 0);
        private string SanitizeProvinceName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var name = raw.Trim();
            name = Regex.Replace(name, "^(Tỉnh|Thanh Pho|Thành phố)\\s+", "", RegexOptions.IgnoreCase);
            return name;
        }
        private async Task<double[][]> ResolveBBoxAsync(int provinceID)
        {
            var min = GetBBox(provinceID, 0);
            var max = GetBBox(provinceID, 1);
            if (!IsZero(min) && !IsZero(max)) return new[] { min, max };

            // 3. Sửa lỗi Async/Await: Sử dụng 'await' và '...Async'
            var province = await _db.TINH_THANH.FirstOrDefaultAsync(t => t.ID == provinceID);
            if (province == null) return null;

            var query = province.TEN_TINH + ", Vietnam";
            try
            {
                // 2. Sửa lỗi HttpClient: Không dùng 'using', dùng instance static
                var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&q=" + HttpUtility.UrlEncode(query);
                var json = await _httpClient.GetStringAsync(url);
                var arr = JArray.Parse(json);
                if (arr.Count == 0) return null;
                var bb = arr[0]["boundingbox"] as JArray; if (bb == null || bb.Count < 4) return null;
                double south = double.Parse(bb[0].ToString(), CultureInfo.InvariantCulture);
                double north = double.Parse(bb[1].ToString(), CultureInfo.InvariantCulture);
                double west = double.Parse(bb[2].ToString(), CultureInfo.InvariantCulture);
                double east = double.Parse(bb[3].ToString(), CultureInfo.InvariantCulture);
                return new[] { new[] { west, south }, new[] { east, north } };
            }
            // 6. Sửa lỗi Logging: Ghi log lỗi thay vì "nuốt" lỗi
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] ResolveBBoxAsync Nominatim call failed: {ex.Message}");
                return null;
            }
        }
        private async Task<double[]> GetProvinceCenterViaNominatimAsync(TINH_THANH province)
        {
            if (province == null) return null;
            try
            {
                // 2. Sửa lỗi HttpClient: Không dùng 'using', dùng instance static
                var name = SanitizeProvinceName(province.TEN_TINH) + ", Vietnam";
                var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&q=" + HttpUtility.UrlEncode(name);
                var json = await _httpClient.GetStringAsync(url);
                var arr = JArray.Parse(json);
                if (arr.Count == 0) return null;
                double lat = double.Parse(arr[0]["lat"].ToString(), CultureInfo.InvariantCulture);
                double lon = double.Parse(arr[0]["lon"].ToString(), CultureInfo.InvariantCulture);
                return new[] { lon, lat }; // lon,lat
            }
            catch (Exception ex)
            {
                // 6. Sửa lỗi Logging
                Debug.WriteLine($"[ERROR] GetProvinceCenterViaNominatimAsync failed: {ex.Message}");
                return null;
            }
        }

        // ===== Overpass helpers =====
        private string BuildOverpassQuery(double south, double west, double north, double east, bool broad)
        {
            // ... logic không đổi ...
            if (!broad)
                return $"[out:json][timeout:25];(node[\"tourism\"]({south},{west},{north},{east});node[\"leisure\"]({south},{west},{north},{east});node[\"historic\"]({south},{west},{north},{east});node[\"natural\"]({south},{west},{north},{east});node[\"amenity\"]({south},{west},{north},{east}););out center 200;";
            return $"[out:json][timeout:25];(node[\"tourism\"]({south},{west},{north},{east});node[\"leisure\"]({south},{west},{north},{east});node[\"historic\"]({south},{west},{north},{east});node[\"natural\"]({south},{west},{north},{east});node[\"amenity\"]({south},{west},{north},{east});node[\"shop\"]({south},{west},{north},{east}););out center 300;";
        }
        private async Task<List<LocationDTO>> ExecuteOverpass(string endpoint, string query, int provinceID)
        {
            var list = new List<LocationDTO>();
            // 2. Sửa lỗi HttpClient: Không dùng 'using', dùng instance static
            try
            {
                var form = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
                var resp = await _httpClient.PostAsync(endpoint, form);
                var body = await resp.Content.ReadAsStringAsync();
                var root = JArray.Parse(JObject.Parse(body)["elements"].ToString());
                foreach (JObject el in root)
                {
                    // ... logic không đổi ...
                    var tags = (JObject)el["tags"]; if (tags == null) continue;
                    var name = (string)tags["name"]; if (string.IsNullOrWhiteSpace(name)) continue;
                    double lat = (double?)el["lat"] ?? (double?)el["center"]?["lat"] ?? 0;
                    double lon = (double?)el["lon"] ?? (double?)el["center"]?["lon"] ?? 0;
                    if (lat == 0 && lon == 0) continue;
                    long oid = (long?)el["id"] ?? 0;
                    list.Add(new LocationDTO { Id = (int)(oid % int.MaxValue), ProvinceId = provinceID, Name = name, Latitude = lat, Longitude = lon });
                    if (list.Count >= 200) break;
                }
            }
            catch (Exception ex) { Debug.WriteLine("Overpass error: " + ex.Message); }
            return list;
        }
        private async Task<List<LocationDTO>> TryOverpass(string endpoint, double[] min, double[] max, int provinceID, bool broad)
        {
            // ... logic không đổi ...
            double south = min[1], west = min[0], north = max[1], east = max[0];
            double pad = 0.05; south -= pad; west -= pad; north += pad; east += pad;
            var q1 = BuildOverpassQuery(south, west, north, east, false);
            var res = await ExecuteOverpass(endpoint, q1, provinceID);
            if (!res.Any() && broad)
            {
                var q2 = BuildOverpassQuery(south, west, north, east, true);
                res = await ExecuteOverpass(endpoint, q2, provinceID);
            }
            return res;
        }
        private async Task<List<LocationDTO>> OverpassFallbackMulti(double[] min, double[] max, int provinceID, bool broad)
        {
            // ... logic không đổi ...
            var endpoints = new[]
            {"https://overpass-api.de/api/interpreter","https://overpass.osm.ch/api/interpreter","https://overpass.kumi.systems/api/interpreter"};
            foreach (var ep in endpoints)
            {
                try
                {
                    await Task.Delay(400); // Thêm delay nhỏ để tránh rate-limit
                    var data = await TryOverpass(ep, min, max, provinceID, broad);
                    if (data.Any()) return data;
                }
                catch (Exception ex) { Debug.WriteLine("Endpoint fail: " + ex.Message); }
            }
            return new List<LocationDTO>();
        }
        private async Task<List<LocationDTO>> OverpassAroundCenter(double lon, double lat, int provinceID, bool broad)
        {
            // ... logic không đổi ...
            int radius = 30000;
            var query = $"[out:json][timeout:25];(node(around:{radius},{lat},{lon})[tourism];node(around:{radius},{lat},{lon})[leisure];node(around:{radius},{lat},{lon})[historic];node(around:{radius},{lat},{lon})[natural];node(around:{radius},{lat},{lon})[amenity];{(broad ? "node(around:" + radius + "," + lat + "," + lon + ")[shop];" : "")});out 300;";
            return await ExecuteOverpass("https://overpass-api.de/api/interpreter", query, provinceID);
        }

        private List<LocationDTO> GetSeedLocationsForProvince(int provinceId)
        {
            // ... logic không đổi ...
            var seed = new List<LocationDTO>();
            for (int i = 1; i <= 50; i++)
            {
                seed.Add(new LocationDTO { Id = provinceId * 1000 + i, ProvinceId = provinceId, Name = $"Điểm {i} - Tỉnh {provinceId}", Latitude = 10.0 + i * 0.01, Longitude = 106.0 + i * 0.01 });
            }
            return seed;
        }

        [HttpGet]
        public async Task<ActionResult> GetLocationsByProvinceID_V2(int provinceID, bool useOverpass = false, bool broad = false)
        {
            try
            {
                var bbox = await ResolveBBoxAsync(provinceID);
                var final = new List<LocationDTO>();
                if (bbox != null)
                {
                    var min = bbox[0]; var max = bbox[1];
                    if (useOverpass)
                    {
                        final = await OverpassFallbackMulti(min, max, provinceID, broad);
                        if (!final.Any())
                        {
                            // 3. Sửa lỗi Async/Await: Sử dụng 'await' và '...Async'
                            var province = await _db.TINH_THANH.FirstOrDefaultAsync(t => t.ID == provinceID);
                            var center = await GetProvinceCenterViaNominatimAsync(province);
                            if (center != null)
                            {
                                var around = await OverpassAroundCenter(center[0], center[1], provinceID, broad);
                                if (around.Any()) final = around;
                            }
                        }
                    }
                }
                if (!final.Any())
                {
                    // 3. Sửa lỗi Async/Await: Sử dụng 'await' và '...Async'
                    var dbLocs = await _db.LOCATION.Where(l => l.ProvinceId == provinceID).ToListAsync();
                    if (!dbLocs.Any()) dbLocs = GetSeedLocationsForProvince(provinceID).Select(s => new LOCATION { ID = s.Id, ProvinceId = s.ProvinceId, Name = s.Name, Latitude = s.Latitude, Longitude = s.Longitude, Source = "Seed" }).ToList();
                    final = dbLocs.Select(l => new LocationDTO { Id = l.ID, ProvinceId = l.ProvinceId, Name = l.Name, Latitude = l.Latitude, Longitude = l.Longitude }).ToList();
                }
                return Json(new { success = final.Any(), data = final }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // ===== Liên Tỉnh =====
        public class LienTinhRequest { public List<int> selectedProvinces { get; set; } }
        private class AlgoSummary { public double totalCost { get; set; } public double totalDistance { get; set; } public double executionTime { get; set; } public int edgeCount { get; set; } public int operationCount { get; set; } }
        private Tuple<List<Edge>, AlgoSummary> RunAlgo(Func<List<Edge>> f)
        {
            var sw = Stopwatch.StartNew();
            var edges = f();
            sw.Stop();
            return Tuple.Create(edges, new AlgoSummary { totalCost = edges.Sum(e => e.Weight), totalDistance = edges.Sum(e => e.Weight), executionTime = sw.Elapsed.TotalMilliseconds, edgeCount = edges.Count, operationCount = edges.Count });
        }

        [HttpPost]
        public async Task<ActionResult> CalculateOptimalRoute(LienTinhRequest req)
        {
            try
            {
                if (req?.selectedProvinces == null || req.selectedProvinces.Count < 2)
                    return Json(new { success = false, message = "Thiếu tỉnh" });

                var ids = req.selectedProvinces.Distinct().ToList();
                var provinces = await _db.TINH_THANH.Where(t => ids.Contains(t.ID)).ToListAsync();
                if (provinces.Count < 2) return Json(new { success = false, message = "Không đủ tỉnh hợp lệ" });

                var graph = new Graph();
                var map = new Dictionary<int, Node>();
                foreach (var p in provinces) { var n = new Node(p.ID, p.TEN_TINH); graph.AddNode(n); map[p.ID] = n; }

                // === SỬA LỖI: Đổi ID_TINH_1/2 thành ID_TINH_A/B ===
                var allDistances = await _db.KHOANG_CACH
                    .Where(kc => ids.Contains(kc.ID_TINH_A) && ids.Contains(kc.ID_TINH_B))
                    .ToListAsync();

                foreach (var kc in allDistances)
                {
                    // Đảm bảo cả hai node đều tồn tại trong map trước khi thêm cạnh
                    if (map.ContainsKey(kc.ID_TINH_A) && map.ContainsKey(kc.ID_TINH_B))
                    {
                        graph.AddEdge(map[kc.ID_TINH_A], map[kc.ID_TINH_B], kc.KHOANG_CACH_VALUE);
                    }
                }
                // === KẾT THÚC SỬA LỖI ===

                if (!graph.Edges.Any()) return Json(new { success = false, message = "Không có khoảng cách" });

                // Đẩy việc chạy thuật toán sang luồng khác
                var (prim, krus, usePrim, bestEdges, bestSummary) = await Task.Run(() =>
                {
                    var primResult = RunAlgo(() => Prim.FindMST(graph));
                    var krusResult = RunAlgo(() => Kruskal.FindMST(graph));
                    bool primIsBetter = primResult.Item2.totalCost <= krusResult.Item2.totalCost;
                    var edges = primIsBetter ? primResult.Item1 : krusResult.Item1;
                    var summary = primIsBetter ? primResult.Item2 : krusResult.Item2;
                    return (primResult, krusResult, primIsBetter, edges, summary);
                });

                var routeNames = new List<string>();
                foreach (var e in bestEdges) { if (e.Src != null && !routeNames.Contains(e.Src.Name)) routeNames.Add(e.Src.Name); if (e.Destination != null && !routeNames.Contains(e.Destination.Name)) routeNames.Add(e.Destination.Name); }

                return Json(new
                {
                    success = true,
                    primResult = prim.Item2,
                    kruskalResult = krus.Item2,
                    optimalRoute = new { algorithmName = usePrim ? "Prim" : "Kruskal", bestSummary.totalCost, bestSummary.totalDistance, bestSummary.executionTime, routeNames, edges = bestEdges.Select(e => new { fromName = e.Src?.Name, toName = e.Destination?.Name, distance = e.Weight, cost = e.Weight }) },
                    comparison = new { costDifference = Math.Abs(prim.Item2.totalCost - krus.Item2.totalCost), timeDifference = Math.Abs(prim.Item2.executionTime - krus.Item2.executionTime), operationDifference = Math.Abs(prim.Item2.operationCount - krus.Item2.operationCount) }
                });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.GetBaseException().Message }); }
        }
        // ===== Nội Tỉnh =====
        public class NoiTinhRequest { public List<LocationDTO> selectedLocations { get; set; } }
        [HttpPost]
        public async Task<ActionResult> CalculateNoiTinhRoute(NoiTinhRequest req)
        {
            try
            {
                if (req?.selectedLocations == null || req.selectedLocations.Count < 2)
                    return Json(new { success = false, message = "Thiếu địa điểm" });

                var locs = req.selectedLocations.Where(l => !string.IsNullOrWhiteSpace(l.Name)).ToList();
                if (locs.Count < 2) return Json(new { success = false, message = "Không đủ địa điểm hợp lệ" });

                // 5. Sửa lỗi Offload CPU: Đẩy toàn bộ công việc nặng (O(N^2) + O(E log V)) sang luồng khác
                var resultJson = await Task.Run(() =>
                {
                    var graph = new Graph(); int id = 1; var nodes = new List<Node>();
                    foreach (var l in locs) { var n = new Node(id++, l.Name) { Latitude = l.Latitude, Longitude = l.Longitude }; graph.AddNode(n); nodes.Add(n); }

                    // O(N^2)
                    for (int i = 0; i < nodes.Count; i++)
                        for (int j = i + 1; j < nodes.Count; j++)
                        {
                            var a = nodes[i]; var b = nodes[j];
                            var dist = HaversineKm(a.Latitude, a.Longitude, b.Latitude, b.Longitude);
                            graph.AddEdge(a, b, dist);
                        }

                    // O(E log V)
                    var prim = RunAlgo(() => Prim.FindMST(graph));
                    var krus = RunAlgo(() => Kruskal.FindMST(graph));
                    bool usePrim = prim.Item2.totalCost <= krus.Item2.totalCost;
                    var bestEdges = usePrim ? prim.Item1 : krus.Item1;
                    var bestSummary = usePrim ? prim.Item2 : krus.Item2;

                    var routeNames = new List<string>();
                    foreach (var e in bestEdges)
                    {
                        if (e.Src != null && !routeNames.Contains(e.Src.Name)) routeNames.Add(e.Src.Name);
                        if (e.Destination != null && !routeNames.Contains(e.Destination.Name)) routeNames.Add(e.Destination.Name);
                    }

                    // Trả về một object để serialize
                    return new
                    {
                        success = true,
                        primResult = prim.Item2,
                        kruskalResult = krus.Item2,
                        optimalRoute = new { algorithmName = usePrim ? "Prim" : "Kruskal", bestSummary.totalCost, bestSummary.totalDistance, bestSummary.executionTime, routeNames, edges = bestEdges.Select(e => new { fromName = e.Src?.Name, toName = e.Destination?.Name, distance = e.Weight, cost = e.Weight }) },
                        comparison = new { costDifference = Math.Abs(prim.Item2.totalCost - krus.Item2.totalCost), timeDifference = Math.Abs(prim.Item2.executionTime - krus.Item2.executionTime), operationDifference = Math.Abs(prim.Item2.operationCount - krus.Item2.operationCount) }
                    };
                }); // Kết thúc Task.Run

                return Json(resultJson);
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.GetBaseException().Message }); }
        }
        private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371.0; double dLat = (lat2 - lat1) * Math.PI / 180.0; double dLon = (lon2 - lon1) * Math.PI / 180.0; lat1 *= Math.PI / 180.0; lat2 *= Math.PI / 180.0; double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2); double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)); return Math.Round(R * c, 3);
        }
    }
}