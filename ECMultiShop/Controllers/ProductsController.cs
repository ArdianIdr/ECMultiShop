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
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Products
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category).Include(p => p.SubCategory);
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
         .Take(10) // Limit to 10 recent notifications
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
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            List<CartItem> cartItems = new List<CartItem>(); 
            var userId = User.Identity.GetUserId(); 

        
            if (User.Identity.IsAuthenticated)
            {
              
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;


           
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems") 
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

               
                    ViewBag.TotalProducts = shoppingCart.CartItems.Sum(ci => ci.Quantity);
                }
                else
                {
                    ViewBag.TotalProducts = 0;
                }

                ViewBag.ShoppingCart = shoppingCart;
            }
            else
            {
                
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

              
                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }

              
                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; 
                ViewBag.ShoppingCart = null; 
            }
            var productcountid = db.Products.Find(id);

      
            int reviewsCount = db.Reviews.Count(review => review.ProductId == id);

            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = db.Products
                               .Include(p => p.Photos)
                               .Include(p => p.StockItems)
                               .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // produktet qe gjinden ne te njejten kategori
            var relatedProducts = db.Products
          .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
          .Include(p => p.Photos)
          .Include(p => p.StockItems) // Include StockItems for availability check
          .ToList();

            Session.Remove("DiscountPercentage");


            var viewModel = new CategoryViewModel
            {
                Products = new List<Product> { product },
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                RelatedProducts = relatedProducts, 
                Reviews = db.Reviews.Where(r => r.ProductId == id).ToList()
            };


            ViewBag.ReviewsCount = reviewsCount;


            return View(viewModel);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult PrDetails(int? id)
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        public ActionResult GetProductDetails(int id)
        {

            var product = db.Products.Include(p => p.StockItems)
                                     .Include(p => p.Photos)
                                     .FirstOrDefault(p => p.Id == id);


           
            var viewModel = new CategoryViewModel
            {
                Products = new List<Product> { product },  
                CartItems = db.CartItems.Where(c => c.ProductId == id).ToList(),  
            };

         
            return PartialView("_partialview", viewModel);
        }




        // GET: Products/Create
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

            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Price,SalePrice,CategoryId,SubCategoryId,Gender,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Price,SalePrice,CategoryId,SubCategoryId,Gender,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "Name", product.CategoryId);
            ViewBag.SubCategoryId = new SelectList(db.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
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
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
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
