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
    public class CartItemsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: CartItems
        public ActionResult Index()
        {
            var cartItems = db.CartItems.Include(c => c.Product).Include(c => c.ShoppingCart);
            return View(cartItems.ToList());
        }

        public ActionResult AddToCart(int productId, int quantity, string size = null, string variantDescription = null)
        {
            if (quantity < 1)
            {
                return Json(new { success = false, message = "Quantity must be at least 1." });
            }

            // marrim produktet qe jan te lidhura me stockin 
            var product = db.Products.Include(p => p.StockItems).FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            // gjejm stock itemin qe pershtatet me te dhenat (size dhe variantDescription)
            var stockItem = product.StockItems.FirstOrDefault(s =>
                (size == null || s.Size == size) &&
                (variantDescription == null || s.VariantDescription == variantDescription));

            if (stockItem == null)
            {
                return Json(new { success = false, message = "Selected variant is out of stock." });
            }

            // vendosim cmimin e stock itemit, nese nuk ka ather mer cmimin baze tek product tablea 
            var unitPrice = stockItem.SalePrice ?? stockItem.Price ?? product.SalePrice ?? product.Price;

            if (User.Identity.IsAuthenticated)
            {
                // userin e marrim
                var userId = User.Identity.GetUserId();

                // retrieve shopping carten e userit
                var cart = db.ShoppingCarts.Include(c => c.CartItems)
                                            .FirstOrDefault(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                        CartItems = new List<CartItem>()
                    };
                    db.ShoppingCarts.Add(cart);
                }

                // shikojm nese produkti qe ben add ekziston ne kart already
                var existingCartItem = cart.CartItems.FirstOrDefault(item =>
                    item.ProductId == productId &&
                    item.Size == size &&
                    item.VariantDescription == variantDescription);

                if (existingCartItem != null)
                {
                    int totalQuantityTryingToAdd = existingCartItem.Quantity + quantity;
                    if (totalQuantityTryingToAdd > stockItem.Quantity)
                    {
                        int additionalCanAdd = stockItem.Quantity - existingCartItem.Quantity;
                        if (additionalCanAdd <= 0)
                        {
                            return Json(new { success = false, message = "You have reached the stock limit for this variant." });
                        }
                        return Json(new { success = false, message = $"You can only add {additionalCanAdd} more of this variant." });
                    }
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    if (quantity > stockItem.Quantity)
                    {
                        return Json(new { success = false, message = $"Only {stockItem.Quantity} left in stock for this variant." });
                    }

                    var newItem = new CartItem
                    {
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        Size = size,
                        VariantDescription = variantDescription,
                        StockItemId = stockItem.Id,
                        UnitPrice = unitPrice
                    };

                    cart.CartItems.Add(newItem);
                }

                db.SaveChanges();  // ruajm te dhenat ne baze
                return Json(new { success = true, message = "Product added to cart successfully." });
            }
            else
            {
                // per userat qe nuk logohen perdorim session
                var cartSession = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                var existingCartItem = cartSession.FirstOrDefault(item =>
      item.ProductId == productId &&
      (string.IsNullOrEmpty(size) || item.Size == size) &&
      (string.IsNullOrEmpty(variantDescription) || item.VariantDescription == variantDescription));

                if (existingCartItem != null)
                {
                    int totalQuantityTryingToAdd = existingCartItem.Quantity + quantity;
                    if (totalQuantityTryingToAdd > stockItem.Quantity)
                    {
                        int additionalCanAdd = stockItem.Quantity - existingCartItem.Quantity;
                        if (additionalCanAdd <= 0)
                        {
                            return Json(new { success = false, message = "You have reached the stock limit for this variant." });
                        }
                        return Json(new { success = false, message = $"You can only add {additionalCanAdd} more of this variant." });
                    }
                    existingCartItem.Quantity += quantity;
                }
                else
                {
                    if (quantity > stockItem.Quantity)
                    {
                        return Json(new { success = false, message = $"Only {stockItem.Quantity} left in stock for this variant." });
                    }

                    var newItem = new CartItem
                    {
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        Size = size,
                        VariantDescription = variantDescription,
                        StockItemId = stockItem.Id,
                        UnitPrice = unitPrice
                    };

                    cartSession.Add(newItem);
                }

                Session["Cart"] = cartSession;
                return Json(new { success = true, message = "Product added to cart successfully." });
            }
        }
        [HttpPost]
        public JsonResult UpdateCartItemQuantity(int productId, string size, string variantDescription, int quantity)
        {
            if (quantity < 1)
            {
                return Json(new { success = false, message = "Quantity must be at least 1." });
            }

            // disa produkte mund te mos ken size ose color per ket perdorim ' ' null 
            size = string.IsNullOrEmpty(size) ? "" : size;
            variantDescription = string.IsNullOrEmpty(variantDescription) ? "" : variantDescription;

            // marrim stock itemin bazuar ne produktin 
            var stockItem = db.Stocks.FirstOrDefault(s =>
                s.ProductId == productId &&
                (s.Size == size || string.IsNullOrEmpty(size)) &&
                (s.VariantDescription == variantDescription || string.IsNullOrEmpty(variantDescription)) &&
                s.IsActive);

            if (stockItem == null)
            {
                return Json(new { success = false, message = "Stock item not available." });
            }

            // shikojm nese e kalon quantityn me quantityn e produktit ne stock
            if (quantity > stockItem.Quantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Only {stockItem.Quantity} items available in stock.",
                    maxAvailableQuantity = stockItem.Quantity
                });
            }

            if (User.Identity.IsAuthenticated)
            {
                string userId = User.Identity.GetUserId();

                // perdorim edhe userid ne query
                var cartItem = db.CartItems.FirstOrDefault(ci =>
                    ci.ProductId == productId &&
                (ci.Size == size || string.IsNullOrEmpty(size)) &&
                (ci.VariantDescription == variantDescription || string.IsNullOrEmpty(variantDescription)) &&
                    ci.ShoppingCart.UserId == userId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Cart item not found." });
                }

                cartItem.Quantity = quantity;
                db.SaveChanges();
            }
            else
            {
                // ata qe nuk logohen perdorin session 
                var cartSession = Session["Cart"] as List<CartItem>;
                if (cartSession == null)
                {
                    return Json(new { success = false, message = "Cart is empty." });
                }

                var cartItem = cartSession.FirstOrDefault(item =>
                    item.ProductId == productId &&
                    (item.Size == size || string.IsNullOrEmpty(size)) &&
                    (item.VariantDescription == variantDescription || string.IsNullOrEmpty(variantDescription)));

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Cart item not found." });
                }

                cartItem.Quantity = quantity;
                Session["Cart"] = cartSession;
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int productId, string size = null, string variantDescription = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                // ata qe jan te loguar largoj nga baza 
                var cartItem = db.CartItems
                   .FirstOrDefault(c =>
         c.ProductId == productId &&
         (string.IsNullOrEmpty(size) || c.Size == size) &&
         (string.IsNullOrEmpty(variantDescription) || c.VariantDescription == variantDescription));

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Item not found in cart." });
                }

                db.CartItems.Remove(cartItem);
                db.SaveChanges();
            }
            else
            {
                // ata qe nuk jan te loguar session
                var cart = Session["Cart"] as List<CartItem>;

                if (cart == null)
                {
                    return Json(new { success = false, message = "Cart not found." });
                }

                var cartItem = cart.FirstOrDefault(c =>
         c.ProductId == productId &&
         (string.IsNullOrEmpty(size) || c.Size == size) &&
         (string.IsNullOrEmpty(variantDescription) || c.VariantDescription == variantDescription));

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Item not found in cart." });
                }

                cart.Remove(cartItem);
                Session["Cart"] = cart; // update sessionin
            }

            return Json(new { success = true });
        }
     

        // GET: CartItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // GET: CartItems/Create
        public ActionResult Create()
        {
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name");
            ViewBag.ShoppingCartId = new SelectList(db.ShoppingCarts, "Id", "UserId");
            return View();
        }

        // POST: CartItems/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ShoppingCartId,ProductId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.CartItems.Add(cartItem);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", cartItem.ProductId);
            ViewBag.ShoppingCartId = new SelectList(db.ShoppingCarts, "Id", "UserId", cartItem.ShoppingCartId);
            return View(cartItem);
        }

        // GET: CartItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", cartItem.ProductId);
            ViewBag.ShoppingCartId = new SelectList(db.ShoppingCarts, "Id", "UserId", cartItem.ShoppingCartId);
            return View(cartItem);
        }

        // POST: CartItems/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ShoppingCartId,ProductId,Quantity")] CartItem cartItem)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cartItem).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProductId = new SelectList(db.Products, "Id", "Name", cartItem.ProductId);
            ViewBag.ShoppingCartId = new SelectList(db.ShoppingCarts, "Id", "UserId", cartItem.ShoppingCartId);
            return View(cartItem);
        }

        // GET: CartItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CartItem cartItem = db.CartItems.Find(id);
            if (cartItem == null)
            {
                return HttpNotFound();
            }
            return View(cartItem);
        }

        // POST: CartItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CartItem cartItem = db.CartItems.Find(id);
            db.CartItems.Remove(cartItem);
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
