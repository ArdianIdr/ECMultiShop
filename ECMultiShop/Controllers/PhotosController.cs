using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ECMultiShop.Models;
using Microsoft.AspNet.Identity;

namespace ECMultiShop.Controllers
{
    public class PhotosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Photos
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
                // shfaqim emailin e userit
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) // limiti ne 10
         .ToList();

            // marrim lowstock only
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) 
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 
            var productPhotoes = db.ProductPhotoes.Include(p => p.Product);
            return View(productPhotoes.ToList());
        }
        [Authorize(Roles = "Admin")]

        // GET: Photos/Details/5
        public ActionResult Details(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) 
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive)
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            var photo = db.ProductPhotoes
    .Include(p => p.Product) 
    .FirstOrDefault(p => p.Id == id);


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductPhoto productPhoto = db.ProductPhotoes.Find(id);
            if (productPhoto == null)
            {
                return HttpNotFound();
            }
            return View(productPhoto);
        }

        // GET: Photos/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10)
         .ToList();

         
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) 
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts;

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            return View();
        }

        // POST: Photos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int ProductId, IEnumerable<HttpPostedFileBase> ProductImages)
        {
            if (ProductImages != null && ProductImages.Any())
            {
                foreach (var file in ProductImages)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        // gjenerojme nje id per fotot
                        var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                        var path = System.IO.Path.Combine(Server.MapPath("~/Content/ProductImages"), fileName);
                        file.SaveAs(path);

                     
                        var productPhoto = new ProductPhoto
                        {
                            ProductId = ProductId,
                            ProductImage = "~/Content/ProductImages/" + fileName
                        };

                    
                        db.ProductPhotoes.Add(productPhoto);
                    }
                }

            
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", ProductId);
            return View();
        }

        // GET: Photos/Edit/1
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
               
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) 
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) 
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts;

            var photo = db.ProductPhotoes
.Include(p => p.Product) 
.FirstOrDefault(p => p.Id == id);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ProductPhoto productPhoto = db.ProductPhotoes.Find(id);
            if (productPhoto == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", productPhoto.ProductId);
            return View(productPhoto);
        }

        // POST: Photos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, [Bind(Include = "Id,ProductId")] ProductPhoto productPhoto, HttpPostedFileBase NewProductImage)
        {
            if (ModelState.IsValid)
            {
                // marrim fotot ekzistuese ne baze
                var existingProductPhoto = db.ProductPhotoes.Find(id);
                if (existingProductPhoto == null)
                {
                    return HttpNotFound();
                }

                // shikojm nese foto e re eshte uploudu
                if (NewProductImage != null && NewProductImage.ContentLength > 0)
                {
                    // fshijm te vjetren
                    var oldImagePath = Server.MapPath(existingProductPhoto.ProductImage);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                    // ruajm te rejen
                    var fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(NewProductImage.FileName);
                    var path = System.IO.Path.Combine(Server.MapPath("~/Content/ProductImages/"), fileName);
                    NewProductImage.SaveAs(path);

                    // Update ne baze
                    existingProductPhoto.ProductImage = "~/Content/ProductImages/" + fileName;
                }

                // Update other properties
                existingProductPhoto.ProductId = productPhoto.ProductId;

                db.Entry(existingProductPhoto).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", productPhoto.ProductId);
            return View(productPhoto);
        }

        [Authorize(Roles = "Admin")]
        // GET: Photos/Delete/5
        public ActionResult Delete(int? id)
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
              
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name");
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name");
            var unreadNotifications = db.Notifications
         .Where(n => !n.IsRead)
         .OrderByDescending(n => n.CreatedAt)
         .Take(10) 
         .ToList();

            // Fetch low stock products
            var lowStockProducts = db.Stocks
                .Where(s => s.Quantity <= 5 && s.IsActive) 
                .Select(s => new LowStockProductViewModel
                {
                    ProductName = s.Product.Name,
                    StockQuantity = s.Quantity
                })
                .OrderBy(s => s.StockQuantity)
                .ToList();

            ViewBag.UnreadNotifications = unreadNotifications; 
            ViewBag.LowStockProducts = lowStockProducts; 

            var photo = db.ProductPhotoes
.Include(p => p.Product) 
.FirstOrDefault(p => p.Id == id);

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductPhoto productPhoto = db.ProductPhotoes.Find(id);
            if (productPhoto == null)
            {
                return HttpNotFound();
            }
            return View(productPhoto);
        }

        // POST: Photos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductPhoto productPhoto = db.ProductPhotoes.Find(id);
            db.ProductPhotoes.Remove(productPhoto);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
