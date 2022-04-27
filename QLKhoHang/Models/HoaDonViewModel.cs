using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLKhoHang.Models
{
    public class HoaDonViewModel
    {
        public HoaDon HoaDons;
        public KhachHang KhachHang;
        public List<ChiTietHoaDon> ChiTietHoaDons;
        public List<SanPham> SanPhams;
    }
}