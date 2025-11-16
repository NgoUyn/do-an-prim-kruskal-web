using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics; // Thêm cái này để debug

namespace Prim_Kruskal_Web.Models
{
    public class DataContext : DbContext
    {
        // 1. Biến static để lưu chuỗi kết nối đã được giải quyết
        private static readonly string _resolvedConnectionString;

        // 2. Static Constructor - Chỉ chạy MỘT LẦN khi ứng dụng khởi động
        static DataContext()
        {
            Debug.WriteLine("=== DataContext STATIC Constructor: Resolving Connection String ===");
            _resolvedConnectionString = ResolveConnectionString();
            Debug.WriteLine($"=== DataContext: Connection String set to: {_resolvedConnectionString} ===");
        }

        // 3. Instance Constructor - Được gọi MỖI REQUEST (rất nhanh)
        // Nó chỉ đơn giản là sử dụng chuỗi kết nối đã được tìm thấy
        public DataContext() : base(_resolvedConnectionString)
        {
            // Constructor này giờ đây trống rỗng và siêu nhanh
        }

        public virtual DbSet<KHOANG_CACH> KHOANG_CACH { get; set; }
        public virtual DbSet<TINH_THANH> TINH_THANH { get; set; }
        public virtual DbSet<LOCATION> LOCATION { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TINH_THANH>()
                .HasMany(e => e.KHOANG_CACH)
                .WithRequired(e => e.TINH_THANH)
                .HasForeignKey(e => e.ID_TINH_A)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TINH_THANH>()
                .HasMany(e => e.KHOANG_CACH1)
                .WithRequired(e => e.TINH_THANH1)
                .HasForeignKey(e => e.ID_TINH_B)
                .WillCascadeOnDelete(false);
        }

        // 4. Toàn bộ logic tìm kiếm của bạn giờ là một phần của static constructor
        private static string ResolveConnectionString()
        {
            var names = new[]
            {
                "DataContext",
                "DataContext_ExpressDot",
                "DataContext_LocalDB",
                "DataContext_Dot",
                "DataContext_Localhost1433",
                "DataContext_MachineExpress"
            };

            foreach (var name in names)
            {
                var cs = ConfigurationManager.ConnectionStrings[name];
                if (cs == null)
                {
                    Debug.WriteLine($"[DB_RESOLVER] Skipping '{name}': Not found in web.config.");
                    continue;
                }

                var raw = cs.ConnectionString;
                if (CanOpen(raw))
                {
                    Debug.WriteLine($"[DB_RESOLVER] SUCCESS: Using '{name}'.");
                    return raw; // Trả về chuỗi kết nối ĐẦU TIÊN hoạt động
                }
                else
                {
                    Debug.WriteLine($"[DB_RESOLVER] FAILED: Could not open '{name}'.");
                }
            }

            // Fallback
            var defaultCs = ConfigurationManager.ConnectionStrings["DataContext"];
            var fallback = defaultCs != null ? defaultCs.ConnectionString : "name=DataContext";
            Debug.WriteLine($"[DB_RESOLVER] WARNING: No working connection found. Falling back to '{fallback}'.");
            return fallback;
        }

        private static bool CanOpen(string connectionString)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    return conn.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để bạn biết TẠI SAO nó không kết nối được
                Debug.WriteLine($"[DB_RESOLVER_CAN_OPEN] Error: {ex.Message}");
                return false;
            }
        }

        // --- Các hàm helper của bạn (không đổi) ---
        public List<KHOANG_CACH> GetAllKhoangCach()
        {
            if (KHOANG_CACH == null) return new List<KHOANG_CACH>();
            return KHOANG_CACH.ToList();
        }

        public List<TINH_THANH> GetAllTinhThanh()
        {
            if (TINH_THANH == null) return new List<TINH_THANH>();
            return TINH_THANH.ToList();
        }

        public KHOANG_CACH GetDistance(int idTinhA, int idTinhB)
        {
            return KHOANG_CACH.FirstOrDefault(k =>
                (k.ID_TINH_A == idTinhA && k.ID_TINH_B == idTinhB) ||
                (k.ID_TINH_A == idTinhB && k.ID_TINH_B == idTinhA));
        }
    }
}