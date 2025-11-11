using Prim_Kruskal_Web.Models;
using Prim_Kruskal_Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Prim_Kruskal_Web.Controllers
{
    public partial class UngDungController : Controller
    {
        private readonly DataContext db = new DataContext();
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(12);
        private static readonly string UserAgent = "PrimKruskalWeb/1.0 (+contact: app@example.com)";

        // Ensure TLS1.2 for external APIs
        static UngDungController()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var client = new HttpClient(handler) { Timeout = HttpTimeout };
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // Serve NoiTinh view
        public ActionResult NoiTinh()
        {
            return View();
        }

        // Provinces list for dropdown
        [HttpGet]
        public ActionResult GetProvinces()
        {
            try
            {
                var tinhThanhs = db.GetAllTinhThanh();
                if (tinhThanhs == null || !tinhThanhs.Any())
                {
                    return Json(new { success = false, message = "Không có dữ liệu tỉnh thành" }, JsonRequestBehavior.AllowGet);
                }
                var provinces = tinhThanhs.Select(t => new { id = t.ID, name = t.TEN_TINH, latitude = 0.0, longitude = 0.0 }).ToList();
                return Json(new { success = true, data = provinces }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải danh sách tỉnh: " + ex.GetBaseException().Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Resolve BBox for any province ID: try static map, else Nominatim
        private async Task<double[][]> ResolveBBoxAsync(int provinceID)
        {
            // 1) Fast static mapping for a few known IDs
            var mapped = GetBBox(provinceID, 0);
            var mappedMax = GetBBox(provinceID, 1);
            if (!IsZero(mapped) && !IsZero(mappedMax))
                return new[] { mapped, mappedMax };

            // 2) Try Nominatim by province name
            var province = db.TINH_THANH.FirstOrDefault(t => t.ID == provinceID);
            if (province == null || string.IsNullOrWhiteSpace(province.TEN_TINH)) return null;
            var query = province.TEN_TINH + ", Vietnam";

            try
            {
                using (var client = CreateHttpClient())
                {
                    var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&q=" + HttpUtility.UrlEncode(query);
                    var json = await client.GetStringAsync(url);
                    var arr = JArray.Parse(json);
                    if (arr.Count == 0) return null;
                    var first = (JObject)arr[0];
                    var bb = first["boundingbox"] as JArray;
                    if (bb == null || bb.Count < 4) return null;
                    // Nominatim boundingbox: [south, north, west, east]
                    double south = double.Parse(bb[0].ToString(), CultureInfo.InvariantCulture);
                    double north = double.Parse(bb[1].ToString(), CultureInfo.InvariantCulture);
                    double west = double.Parse(bb[2].ToString(), CultureInfo.InvariantCulture);
                    double east = double.Parse(bb[3].ToString(), CultureInfo.InvariantCulture);
                    return new[] { new[] { west, south }, new[] { east, north } };
                }
            }
            catch
            {
                return null;
            }
        }

        private bool IsZero(double[] p) => p == null || (p.Length >= 2 && p[0] == 0 && p[1] == 0);

        // Helper: sanitize province name for queries (remove prefixes like 'Tỉnh', 'Thành phố')
        private string SanitizeProvinceName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            var name = raw.Trim();
            name = Regex.Replace(name, "^(Tỉnh|Thanh Pho|Thành phố)\\s+", "", RegexOptions.IgnoreCase);
            return name;
        }

        [HttpGet]
        public async Task<ActionResult> GetLocationsByProvinceID_V2(int provinceID, bool useOverpass = false, bool broad = false, bool debug = false)
        {
            try
            {
                var apiKey = (ConfigurationManager.AppSettings["OpenRouteServiceApiKey"] ?? string.Empty).Trim();
                var province = db.TINH_THANH.FirstOrDefault(t => t.ID == provinceID);
                var bbox = await ResolveBBoxAsync(provinceID);
                if (bbox == null)
                    return Json(new { success = false, message = "Không tìm thấy BBox cho tỉnh" }, JsonRequestBehavior.AllowGet);
                var min = bbox[0]; var max = bbox[1];

                if (useOverpass || string.IsNullOrWhiteSpace(apiKey))
                {
                    var over = await OverpassFallbackMulti(min, max, provinceID, broad);
                    string usedStrategy = "bbox";

                    if (!over.Any())
                    {
                        // Second strategy: query around province center (from Nominatim) with radius 30km
                        var center = await GetProvinceCenterViaNominatimAsync(province);
                        if (center != null)
                        {
                            var around = await OverpassAroundCenter(center[0], center[1], provinceID, broad);
                            if (around.Any())
                            {
                                over = around;
                                usedStrategy = "center-radius";
                            }
                        }
                    }

                    if (over.Any())
                    {
                        return Json(new { success = true, strategy = usedStrategy, count = over.Count, data = over }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = "Overpass không trả về dữ liệu dù đã thử mở rộng (bbox & radius).", strategy = usedStrategy }, JsonRequestBehavior.AllowGet);
                }

                using (var client = CreateHttpClient())
                {
                    client.BaseAddress = new Uri("https://api.openrouteservice.org");
                    client.DefaultRequestHeaders.Remove("Authorization");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", apiKey);

                    var payload = new
                    {
                        geometry = new { bbox = new double[][] { min, max } },
                        filters = new { category_group_ids = new int[] { 560 } },
                        limit = 200
                    };
                    var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync("/pois", content);
                    var body = await resp.Content.ReadAsStringAsync();
                    if (resp.IsSuccessStatusCode)
                    {
                        var ors = JsonConvert.DeserializeObject<OrsPoiResponse>(body);
                        var list = (ors?.Features ?? new List<OrsFeature>()).Where(f => f?.Geometry?.Coordinates?.Count >= 2 && !string.IsNullOrWhiteSpace(f.Properties?.Name))
                            .Select(f => new LocationDTO
                            {
                                Id = (int)(f.Properties.OsmId % int.MaxValue),
                                ProvinceId = provinceID,
                                Name = f.Properties.Name,
                                Longitude = f.Geometry.Coordinates[0],
                                Latitude = f.Geometry.Coordinates[1]
                            }).ToList();
                        return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
                    }
                    if ((int)resp.StatusCode == 403)
                    {
                        return Json(new { success = false, message = "API key gói free không có quyền POIs. Dùng tham số useOverpass=true." }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { success = false, message = $"Lỗi API ORS: {(int)resp.StatusCode} {resp.ReasonPhrase}. Chi tiết: {body}" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.GetBaseException()?.Message ?? ex.Message;
                return Json(new { success = false, message = "Lỗi máy chủ: " + msg }, JsonRequestBehavior.AllowGet);
            }
        }

        private async Task<double[]> GetProvinceCenterViaNominatimAsync(TINH_THANH province)
        {
            if (province == null) return null;
            try
            {
                using (var client = CreateHttpClient())
                {
                    var name = SanitizeProvinceName(province.TEN_TINH) + ", Vietnam";
                    var url = "https://nominatim.openstreetmap.org/search?format=json&limit=1&polygon_geojson=0&q=" + HttpUtility.UrlEncode(name);
                    var json = await client.GetStringAsync(url);
                    var arr = JArray.Parse(json);
                    if (arr.Count == 0) return null;
                    var first = (JObject)arr[0];
                    double lat = double.Parse(first["lat"].ToString(), CultureInfo.InvariantCulture);
                    double lon = double.Parse(first["lon"].ToString(), CultureInfo.InvariantCulture);
                    return new[] { lon, lat }; // consistent lon,lat ordering
                }
            }
            catch { return null; }
        }

        private async Task<List<LocationDTO>> OverpassAroundCenter(double lon, double lat, int provinceID, bool broad)
        {
            // Use radius 30000m (30km) search around center
            var radius = 30000;
            var list = new List<LocationDTO>();
            var baseFilter = "tourism|leisure|historic|natural|amenity";
            var extraFilter = broad ? "|shop" : "";
            var query = $@"[out:json][timeout:25];
(
  node(around:{radius},{lat},{lon})[tourism];
  node(around:{radius},{lat},{lon})[leisure];
  node(around:{radius},{lat},{lon})[historic];
  node(around:{radius},{lat},{lon})[natural];
  node(around:{radius},{lat},{lon})[amenity];
  {(broad ? "node(around:" + radius + "," + lat + "," + lon + ")[shop];" : string.Empty)}
);
out 300;";
            using (var client = CreateHttpClient())
            {
                try
                {
                    var form = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
                    var resp = await client.PostAsync("https://overpass-api.de/api/interpreter", form);
                    var body = await resp.Content.ReadAsStringAsync();
                    try
                    {
                        var root = JObject.Parse(body); var elements = (JArray)root["elements"] ?? new JArray();
                        foreach (JObject el in elements)
                        {
                            var tags = (JObject)el["tags"]; if (tags == null) continue; var name = (string)tags["name"]; if (string.IsNullOrWhiteSpace(name)) continue;
                            double nlat = (double?)el["lat"] ?? 0; double nlon = (double?)el["lon"] ?? 0; if (nlat == 0 && nlon == 0) continue; long oid = (long?)el["id"] ?? 0;
                            list.Add(new LocationDTO { Id = (int)(oid % int.MaxValue), ProvinceId = provinceID, Name = name, Latitude = nlat, Longitude = nlon });
                            if (list.Count >= 200) break;
                        }
                    }
                    catch { }
                }
                catch { }
            }
            return list;
        }

        private async Task<List<LocationDTO>> OverpassFallbackMulti(double[] min, double[] max, int provinceID, bool broad)
        {
            var endpoints = new[]
            {
                "https://overpass-api.de/api/interpreter",
                "https://overpass.osm.ch/api/interpreter",
                "https://overpass.kumi.systems/api/interpreter"
            };

            foreach (var ep in endpoints)
            {
                try
                {
                    // gentle rate limit
                    await Task.Delay(800);
                    var data = await TryOverpass(ep, min, max, provinceID, broad);
                    if (data.Any()) return data;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Overpass endpoint failed: " + ep + " => " + ex.Message);
                }
            }
            return new List<LocationDTO>();
        }

        private string BuildOverpassQuery(double south, double west, double north, double east, bool broad)
        {
            var baseQuery = $@"[out:json][timeout:25];
(
  node[""tourism""~""attraction|museum|gallery|viewpoint|theme_park|zoo|information""]({south},{west},{north},{east});
  way[""tourism""~""attraction|museum|gallery|viewpoint|theme_park|zoo|information""]({south},{west},{north},{east});
  node[""leisure""~""park|garden|nature_reserve""]({south},{west},{north},{east});
  way[""leisure""~""park|garden|nature_reserve""]({south},{west},{north},{east});
  node[""historic""~""castle|ruins|monument|memorial|archaeological_site""]({south},{west},{north},{east});
  node[""natural""~""peak|spring|cave_entrance|tree""]({south},{west},{north},{east});
  node[""amenity""~""place_of_worship|theatre|arts_centre|fountain|marketplace""]({south},{west},{north},{east});
);out center 200;";

            if (!broad) return baseQuery;

            // Extended query adds food, shops, education, transport
            var extended = $@"[out:json][timeout:25];
(
  node[""tourism""~""attraction|museum|gallery|viewpoint|theme_park|zoo|information|artwork|hotel""]({south},{west},{north},{east});
  way[""tourism""~""attraction|museum|gallery|viewpoint|theme_park|zoo|information|artwork|hotel""]({south},{west},{north},{east});
  node[""amenity""~""restaurant|cafe|fast_food|bar|pub|theatre|arts_centre|library|marketplace""]({south},{west},{north},{east});
  way[""amenity""~""restaurant|cafe|fast_food|bar|pub|theatre|arts_centre|library|marketplace""]({south},{west},{north},{east});
  node[""shop""~""supermarket|convenience|mall|bakery|gift|department_store""]({south},{west},{north},{east});
  way[""shop""~""supermarket|convenience|mall|bakery|gift|department_store""]({south},{west},{north},{east});
  node[""leisure""~""park|garden|nature_reserve|sports_centre|pitch|stadium""]({south},{west},{north},{east});
  way[""leisure""~""park|garden|nature_reserve|sports_centre|pitch|stadium""]({south},{west},{north},{east});
  node[""historic""~""castle|ruins|monument|memorial|archaeological_site|fort""]({south},{west},{north},{east});
  node[""natural""~""peak|spring|cave_entrance|tree|waterfall""]({south},{west},{north},{east});
  node[""amenity""~""school|university|bus_station|ferry_terminal|parking""]({south},{west},{north},{east});
);out center 300;";
            return extended;
        }

        private async Task<List<LocationDTO>> TryOverpass(string endpoint, double[] min, double[] max, int provinceID, bool broad)
        {
            var south = min[1]; var west = min[0]; var north = max[1]; var east = max[0];
            double pad = 0.05; south -= pad; west -= pad; north += pad; east += pad;
            var ql = BuildOverpassQuery(south, west, north, east, false);
            var qlBroad = broad ? BuildOverpassQuery(south, west, north, east, true) : null;

            var results = await ExecuteOverpass(endpoint, ql, provinceID, south, west, north, east);
            if (!results.Any() && qlBroad != null)
            {
                // second attempt with extended tags
                await Task.Delay(500);
                results = await ExecuteOverpass(endpoint, qlBroad, provinceID, south, west, north, east);
            }
            return results;
        }

        private async Task<List<LocationDTO>> ExecuteOverpass(string endpoint, string query, int provinceID, double south, double west, double north, double east)
        {
            var list = new List<LocationDTO>();
            using (var client = CreateHttpClient())
            {
                try
                {
                    var form = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("data", query) });
                    var resp = await client.PostAsync(endpoint, form);
                    var body = await resp.Content.ReadAsStringAsync();
                    try
                    {
                        var root = JObject.Parse(body); var elements = (JArray)root["elements"] ?? new JArray();
                        foreach (JObject el in elements)
                        {
                            var tags = (JObject)el["tags"]; if (tags == null) continue; var name = (string)tags["name"]; if (string.IsNullOrWhiteSpace(name)) continue;
                            double lat = 0, lon = 0;
                            if (el["type"]?.ToString() == "node") { lat = (double?)el["lat"] ?? 0; lon = (double?)el["lon"] ?? 0; }
                            else { var center = (JObject)el["center"]; if (center != null) { lat = (double?)center["lat"] ?? 0; lon = (double?)center["lon"] ?? 0; } }
                            if (lat == 0 && lon == 0) continue; long osmId = (long?)el["id"] ?? 0;
                            list.Add(new LocationDTO { Id = (int)(osmId % int.MaxValue), ProvinceId = provinceID, Name = name, Latitude = lat, Longitude = lon });
                            if (list.Count >= 200) break;
                        }
                    }
                    catch (Exception pex)
                    {
                        Debug.WriteLine("Parse Overpass JSON error: " + pex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ExecuteOverpass error: " + ex.Message);
                }
            }
            return list;
        }

        private double[] GetBBox(int provinceID, int index)
        {
            switch (provinceID)
            {
                case 1: return (index == 0) ? new double[] { 106.33, 10.36 } : new double[] { 107.03, 11.19 }; // HCM
                case 2: return (index == 0) ? new double[] { 106.37, 10.99 } : new double[] { 106.92, 11.53 }; // Binh Duong
                case 3: return (index == 0) ? new double[] { 106.79, 10.51 } : new double[] { 107.59, 11.60 }; // Dong Nai
                case 4: return (index == 0) ? new double[] { 106.96, 10.25 } : new double[] { 107.58, 10.88 }; // Ba Ria - Vung Tau
                case 5: return (index == 0) ? new double[] { 105.74, 11.05 } : new double[] { 106.57, 11.70 }; // Tay Ninh
                case 6: return (index == 0) ? new double[] { 105.52, 10.37 } : new double[] { 106.81, 11.17 }; // Long An
                case 7: return (index == 0) ? new double[] { 106.23, 9.81 } : new double[] { 106.80, 10.36 }; // Ben Tre
                case 8: return (index == 0) ? new double[] { 105.71, 9.92 } : new double[] { 106.25, 10.35 }; // Vinh Long
                case 9: return (index == 0) ? new double[] { 105.53, 9.89 } : new double[] { 105.90, 10.19 }; // Can Tho
                case 10: return (index == 0) ? new double[] { 104.75, 10.22 } : new double[] { 105.76, 11.11 }; // An Giang
                default: return new double[] { 0, 0 };
            }
        }

        public class OrsPoiResponse { [JsonProperty("features")] public List<OrsFeature> Features { get; set; } }
        public class OrsFeature { [JsonProperty("geometry")] public OrsGeometry Geometry { get; set; } [JsonProperty("properties")] public OrsProperties Properties { get; set; } }
        public class OrsGeometry { [JsonProperty("coordinates")] public List<double> Coordinates { get; set; } }
        public class OrsProperties { [JsonProperty("osm_id")] public long OsmId { get; set; } [JsonProperty("name")] public string Name { get; set; } }

        [HttpGet]
        public async Task<ActionResult> Diag(int provinceID, bool broad = true)
        {
            var result = new Dictionary<string, object>();
            try
            {
                var province = db.TINH_THANH.FirstOrDefault(t => t.ID == provinceID);
                result["province"] = province?.TEN_TINH ?? "unknown";

                // 1) BBox
                var bbox = await ResolveBBoxAsync(provinceID);
                if (bbox != null)
                {
                    result["bbox"] = new { min = new { lon = bbox[0][0], lat = bbox[0][1] }, max = new { lon = bbox[1][0], lat = bbox[1][1] } };
                }
                else
                {
                    result["bbox"] = "null (ResolveBBoxAsync failed)";
                }

                // 2) Nominatim center
                double[] center = null; string nominatimErr = null;
                try
                {
                    center = await GetProvinceCenterViaNominatimAsync(province);
                }
                catch (Exception ex)
                {
                    nominatimErr = ex.GetBaseException().Message;
                }
                result["nominatim"] = new { ok = center != null, centerLon = center != null ? (double?)center[0] : null, centerLat = center != null ? (double?)center[1] : null, error = nominatimErr };

                // 3) Overpass per endpoint
                var endpoints = new[]
                {
                    "https://overpass-api.de/api/interpreter",
                    "https://overpass.osm.ch/api/interpreter",
                    "https://overpass.kumi.systems/api/interpreter"
                };
                var overItems = new List<object>();
                foreach (var ep in endpoints)
                {
                    var epItem = new Dictionary<string, object> { { "endpoint", ep } };
                    try
                    {
                        int bboxCount = 0, radiusCount = 0; string err1 = null, err2 = null;
                        // BBox query
                        if (bbox != null)
                        {
                            try
                            {
                                var tmp = await TryOverpass(ep, bbox[0], bbox[1], provinceID, broad);
                                bboxCount = tmp.Count;
                            }
                            catch (Exception ex1) { err1 = ex1.GetBaseException().Message; }
                        }
                        // Center-radius query
                        if (center != null)
                        {
                            try
                            {
                                var tmp2 = await OverpassAroundCenter(center[0], center[1], provinceID, broad);
                                radiusCount = tmp2.Count;
                            }
                            catch (Exception ex2) { err2 = ex2.GetBaseException().Message; }
                        }
                        epItem["bboxCount"] = bboxCount;
                        epItem["radiusCount"] = radiusCount;
                        epItem["errorBBox"] = err1;
                        epItem["errorRadius"] = err2;
                    }
                    catch (Exception ex)
                    {
                        epItem["error"] = ex.GetBaseException().Message;
                    }
                    overItems.Add(epItem);
                }
                result["overpass"] = overItems;

                // 4) ORS availability
                var apiKey = (ConfigurationManager.AppSettings["OpenRouteServiceApiKey"] ?? string.Empty).Trim();
                result["orsConfigured"] = !string.IsNullOrWhiteSpace(apiKey);

                return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.GetBaseException().Message, data = result }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}