using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Prim_Kruskal_Web.Models
{
    public class DataContext : DbContext
    {
        public DataContext() : base("name=DataContext")
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

        public List<KHOANG_CACH> GetAllKhoangCach()
        {
            try
            {
                if (KHOANG_CACH == null) return new List<KHOANG_CACH>();

                // Include the related TINH_THANH navigation properties so callers can access names/coords.
                return KHOANG_CACH
                    .Include(k => k.TINH_THANH)
                    .Include(k => k.TINH_THANH1)
                    .ToList();
            }
            catch (Exception)
            {
                // Do not throw from data access helper � controller expects a list and handles errors.
                return new List<KHOANG_CACH>();
            }
        }
        public List<TINH_THANH> GetAllTinhThanh()
        {
            try
            {
                if (TINH_THANH == null) return new List<TINH_THANH>();

                return TINH_THANH
                    .Include(t => t.KHOANG_CACH)
                    .Include(t => t.KHOANG_CACH1)
                    .ToList();
            }
            catch (Exception)
            {
                return new List<TINH_THANH>();
            }
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