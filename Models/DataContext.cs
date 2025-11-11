using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Configuration;
using System.Data.SqlClient;

namespace Prim_Kruskal_Web.Models
{
    public class DataContext : DbContext
    {
        // Try multiple connection strings to improve out-of-the-box run experience
        public DataContext() : base(ResolveConnectionString())
        {
        }

        public virtual DbSet<KHOANG_CACH> KHOANG_CACH { get; set; }
        public virtual DbSet<TINH_THANH> TINH_THANH { get; set; }

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

        private static string ResolveConnectionString()
        {
            // Preferred order
            var names = new[]
            {
                "DataContext",                 // existing
                "DataContext_ExpressDot",     // .\\SQLEXPRESS
                "DataContext_LocalDB",        // (localdb)\\MSSQLLocalDB
                "DataContext_Dot",            // . (default instance)
                "DataContext_Localhost1433",  // localhost,1433
                "DataContext_MachineExpress"  // {MACHINE}\\SQLEXPRESS
            };

            foreach (var name in names)
            {
                var cs = ConfigurationManager.ConnectionStrings[name];
                if (cs == null) continue;

                var raw = cs.ConnectionString;
                if (CanOpen(raw))
                {
                    return raw; // Return first working connection string
                }
            }

            // Fallback to the named connection if all test fails (EF will still try)
            var defaultCs = ConfigurationManager.ConnectionStrings["DataContext"];
            return defaultCs != null ? defaultCs.ConnectionString : "name=DataContext";
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
            catch
            {
                return false;
            }
        }

        public List<KHOANG_CACH> GetAllKhoangCach()
        {
            if (KHOANG_CACH == null)
            {
                return new List<KHOANG_CACH>();
            }

            // PHẢI LÀ "KHOANG_CACH.ToList()", KHÔNG PHẢI "new List()"
            return KHOANG_CACH.ToList();
        }
        // HÀM MỚI (ĐÃ SỬA)
        public List<TINH_THANH> GetAllTinhThanh()
        {
            // BỎ try-catch để lỗi (nếu có) có thể nổi lên Controller
            if (TINH_THANH == null)
            {
                return new List<TINH_THANH>();
            }

            // Chỉ cần lấy danh sách Tỉnh, không cần Include
            return TINH_THANH.ToList();
        }

        public KHOANG_CACH GetDistance(int idTinhA, int idTinhB)
        {
            return KHOANG_CACH.FirstOrDefault(k =>
                (k.ID_TINH_A == idTinhA && k.ID_TINH_B == idTinhB) ||
                (k.ID_TINH_A == idTinhB && k.ID_TINH_B == idTinhA));
        }

        // Add this method to your existing DataContext class

    }
}