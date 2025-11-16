using System;

namespace Prim_Kruskal_Web.Models
{
    // DTO đại diện một địa điểm dùng cho tính năng Ứng dụng (UngDung)
    public class LocationDTO
    {
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}