using Warehouse_25DH110249.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Warehouse_25DH110249.Controllers
{
    public class ProductsController : Controller
    {
        private Warehouse_25DH110249Entities1 db = new Warehouse_25DH110249Entities1();

        // Helper: Phân loại mức giá theo đề bài
        public static string GetPriceLevel(decimal price)
        {
            if (price < 100000) return "Giá thấp";
            if (price < 500000) return "Giá trung bình";
            return "Giá cao";
        }

        // ==========================================
        // CÂU 2: HIỂN THỊ VÀ CHỈNH SỬA SẢN PHẨM
        // ==========================================
        public ActionResult Index()
        {
            var products = db.Products.ToList();
            return View(products);
        }

        // GET: Sửa sản phẩm
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int ProductID, string ProductName, string Category, decimal Price)
        {
            var product = db.Products.Find(ProductID);
            if (product == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được rỗng!");
            if (string.IsNullOrWhiteSpace(Category))
                ModelState.AddModelError("Category", "Danh mục không được rỗng!");
            if (Price <= 0)
                ModelState.AddModelError("Price", "Đơn giá mới phải lớn hơn 0!");

            if (ModelState.IsValid)
            {
                // Chỉ cho phép sửa ProductName, Category, Price; giữ nguyên Quantity
                product.ProductName = ProductName.Trim();
                product.Category = Category.Trim();
                product.Price = Price;
                db.SaveChanges();

                TempData["Success"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // ==========================================
        // CÂU 3: LỌC THEO KHOẢNG GIÁ VÀ SẮP XẾP
        // ==========================================
        public ActionResult Filter(decimal? fromPrice, decimal? toPrice, string category, string sortOrder)
        {
            ViewBag.Categories = db.Products.Select(p => p.Category).Distinct().ToList();
            ViewBag.SelectedCategory = category;
            ViewBag.FromPrice = fromPrice;
            ViewBag.ToPrice = toPrice;
            ViewBag.SortOrder = sortOrder;

            // Kiểm tra Từ giá > Đến giá
            if (fromPrice.HasValue && toPrice.HasValue && fromPrice.Value > toPrice.Value)
            {
                ViewBag.ErrorMessage = "Lỗi: 'Từ giá' không được lớn hơn 'Đến giá'!";
                return View(Enumerable.Empty<Product>());
            }

            var query = db.Products.AsQueryable();

            if (fromPrice.HasValue) query = query.Where(p => p.Price >= fromPrice.Value);
            if (toPrice.HasValue) query = query.Where(p => p.Price <= toPrice.Value);
            if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);

            if (sortOrder == "asc")
                query = query.OrderBy(p => p.Price);
            else if (sortOrder == "desc")
                query = query.OrderByDescending(p => p.Price);

            return View(query.ToList());
        }

        // ==========================================
        // CÂU 4: ĐIỀU CHỈNH GIÁ BÁN SẢN PHẨM
        // ==========================================
        public ActionResult AdjustPrice()
        {
            ViewBag.Products = db.Products.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncreasePrice(int productId, decimal percent)
        {
            var p = db.Products.Find(productId);
            if (p == null) return HttpNotFound();

            if (percent <= 0 || percent > 100)
            {
                TempData["Error"] = "Tỷ lệ tăng phải lớn hơn 0 và không vượt quá 100%!";
                return RedirectToAction("AdjustPrice");
            }

            decimal newPrice = Math.Round(p.Price * (1 + (percent / 100)), 2);
            p.Price = newPrice;
            db.SaveChanges();

            TempData["Success"] = $"Đã tăng giá sản phẩm '{p.ProductName}' thêm {percent}%. Giá mới: {newPrice:N0} đ";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DecreasePrice(int productId, decimal percent)
        {
            var p = db.Products.Find(productId);
            if (p == null) return HttpNotFound();

            if (percent <= 0 || percent >= 100)
            {
                TempData["Error"] = "Tỷ lệ giảm phải lớn hơn 0 và nhỏ hơn 100%!";
                return RedirectToAction("AdjustPrice");
            }

            decimal newPrice = Math.Round(p.Price * (1 - (percent / 100)), 2);
            if (newPrice < 1000)
            {
                TempData["Error"] = $"Từ chối lưu: Giá sau khi giảm ({newPrice:N0} đ) nhỏ hơn 1.000 đồng!";
                return RedirectToAction("AdjustPrice");
            }

            p.Price = newPrice;
            db.SaveChanges();

            TempData["Success"] = $"Đã giảm giá sản phẩm '{p.ProductName}' {percent}%. Giá mới: {newPrice:N0} đ";
            return RedirectToAction("Index");
        }

        // ==========================================
        // CÂU 5: ĐIỀU CHỈNH GIÁ HAI SẢN PHẨM TRONG GIAO DỊCH
        // ==========================================
        public ActionResult DiscountTwo()
        {
            ViewBag.Products = db.Products.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DiscountTwo(int id1, decimal rate1, int id2, decimal rate2)
        {
            ViewBag.Products = db.Products.ToList();

            if (id1 == id2)
            {
                ViewBag.Error = "Vui lòng chọn 2 sản phẩm khác nhau!";
                return View();
            }

            if (rate1 <= 0 || rate1 >= 100 || rate2 <= 0 || rate2 >= 100)
            {
                ViewBag.Error = "Tỷ lệ giảm của cả 2 sản phẩm phải lớn hơn 0 và nhỏ hơn 100%!";
                return View();
            }

            var p1 = db.Products.Find(id1);
            var p2 = db.Products.Find(id2);

            decimal newPrice1 = Math.Round(p1.Price * (1 - (rate1 / 100)), 2);
            decimal newPrice2 = Math.Round(p2.Price * (1 - (rate2 / 100)), 2);

            // Kiểm tra điều kiện >= 1000
            if (newPrice1 < 1000 || newPrice2 < 1000)
            {
                ViewBag.Error = $"Giao dịch bị từ chối: Sản phẩm có giá sau giảm < 1.000đ " +
                                $"(SP1 dự kiến: {newPrice1:N0}đ, SP2 dự kiến: {newPrice2:N0}đ). Giá cả hai sản phẩm được giữ nguyên!";
                return View();
            }

            // Thực hiện giao dịch an toàn (Transaction)
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    p1.Price = newPrice1;
                    p2.Price = newPrice2;
                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = $"Cập nhật thành công 2 sản phẩm: '{p1.ProductName}' còn {newPrice1:N0} đ và '{p2.ProductName}' còn {newPrice2:N0} đ.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Error = "Lỗi trong quá trình thực hiện giao dịch: " + ex.Message;
                    return View();
                }
            }
        }

        // ==========================================
        // CÂU 6: THÊM SẢN PHẨM MỚI
        // ==========================================
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductName,Category,Price,Quantity")] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống!");
            if (string.IsNullOrWhiteSpace(product.Category))
                ModelState.AddModelError("Category", "Danh mục không được để trống!");
            if (product.Price <= 0)
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0!");
            if (product.Quantity < 0)
                ModelState.AddModelError("Quantity", "Số lượng phải là số nguyên không âm!");

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm mới sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // ==========================================
        // CÂU 7: THỐNG KÊ GIÁ VÀ GIÁ TRỊ HÀNG HOÁ
        // ==========================================
        public ActionResult Statistics()
        {
            int totalProducts = db.Products.Count();
            int highPriceCount = db.Products.Count(p => p.Price >= 500000);
            decimal totalInventoryValue = db.Products.Any()
                ? db.Products.Sum(p => (decimal?)(p.Price * p.Quantity)) ?? 0
                : 0;

            ViewBag.TotalProducts = totalProducts;
            ViewBag.HighPriceCount = highPriceCount;
            ViewBag.TotalInventoryValue = totalInventoryValue;

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}