using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ECMultiShop.Models;
using Microsoft.AspNet.Identity;
using Stripe;
using Stripe.Checkout;

namespace ECMultiShop.Controllers
{
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        // GET: Orders
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
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

            // merr gjitha te dhenat e bazes
            var allOrders = db.Orders 
                .Select(order => new RecentOrderViewModel
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    CustomerName = order.FirstName, 
                    Total = order.Total,
                    Status = order.Status.ToString(),
                    Address1 = order.AddressLine1,
                    Address2 = order.AddressLine2,
                    MobilePhone = order.MobileNo,
                    Items = order.OrderItems.Select(item => new OrderItemViewModel
                    {
                        ProductName = item.ProductName,
                        Size = item.Size,
                        Quantity = item.Quantity
                    }).ToList()
                })
                .ToList();


            var totalEarnings = allOrders.Where(o => o.Status == "Completed").Sum(o => o.Total);
            var totalLosses = allOrders.Where(o => o.Status == "Refunded").Sum(o => o.Total);

            // dergoj ne view
            ViewBag.TotalEarnings = totalEarnings;
            ViewBag.TotalLosses = totalLosses;



            // dergoj ne view informatat
            return View(allOrders);

        }

        public ActionResult CreateCheckoutSession()
        {
            string userId = User.Identity.GetUserId();

            // marrim shopping carten
            var shoppingCart = db.ShoppingCarts
                .Include(sc => sc.CartItems.Select(ci => ci.Product))
                .FirstOrDefault(sc => sc.UserId == userId);

            if (shoppingCart == null || !shoppingCart.CartItems.Any())
            {
                return Json(new { error = "Your cart is empty." });
            }

            // marrim discount % nga sessioni nese nuk ka = 0
            decimal discountPercentage = Session["DiscountPercentage"] != null ? (decimal)Session["DiscountPercentage"] : 0;

            // kalkulimi i perqindjes se discountit
            decimal discountFactor = 1 - (discountPercentage / 100);

            // pregaditet stripe
            var domain = "https://localhost:44327/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = shoppingCart.CartItems.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)((item.UnitPrice * discountFactor) * 100), // aplikojm discountin
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Quantity,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = domain + "/Home/Success/",
                CancelUrl = domain + "/Home/Cancel/",
            };

            // shtojm shipping fee ne nje rresht te vecant 
            decimal shipping = 5.0m;
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(shipping * 100), // cent
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = "Shipping Fee",
                    },
                },
                Quantity = 1,
            });

            options.Metadata = new Dictionary<string, string>
    {
        { "DiscountPercentage", discountPercentage.ToString("F2") }
    };

            var client = new Stripe.StripeClient("sk_test_51PEcZsJjOnxsjQ4OiEG1hpelzb0f2HJdKrSG2XFnkBpIoKQvmQp01KA5mgsD2naMeq7iRxrtKrkeVVF2l4l7UfLw00702mj9QP"); // secret key
            var service = new SessionService(client);

            // krijojm sesionin
            Session session = service.Create(options);
            return Json(new { id = session.Id });
        }





        /*  // Create Checkout Session
          [HttpPost]
          public async Task<ActionResult> CreateCheckoutSession()
          {
              var userId = User.Identity.GetUserId();  // Get the authenticated user's ID
              var cartItems = db.ShoppingCarts
                              .Where(cart => cart.UserId == userId)
                     .SelectMany(cart => cart.CartItems)
                     .Include(item => item.Product) // Ensure Product is loaded
                     .ToList();

              if (cartItems == null || !cartItems.Any())
              {
                  return Json(new { error = "No items in cart" });  // Handle empty cart case
              }

              var options = new SessionCreateOptions
              {
                  PaymentMethodTypes = new List<string> { "card" },
                  LineItems = GetLineItemsFromCart(cartItems),
                  Mode = "payment",
                  SuccessUrl = Url.Action("Success", "Orders", null, Request.Url.Scheme),
                  CancelUrl = Url.Action("Cancel", "Orders", null, Request.Url.Scheme),
              };

              var service = new SessionService();
              var session = await service.CreateAsync(options);

              return Json(new { id = session.Id });
          }

          // Helper to prepare Line Items from cart retrieved from DB
          private List<SessionLineItemOptions> GetLineItemsFromCart(List<CartItem> cartItems)
          {
              return cartItems.Select(item => new SessionLineItemOptions
              {
                  PriceData = new SessionLineItemPriceDataOptions
                  {
                      UnitAmountDecimal = item.UnitPrice * 100, // Amount in cents
                      Currency = "usd",
                      ProductData = new SessionLineItemPriceDataProductDataOptions
                      {
                          Name = item.Product?.Name ?? "Unknown Product", // Handle null Product
                      },
                  },
                  Quantity = item.Quantity,
              }).ToList();
          }

          // Success callback   
          public ActionResult Success()
          {
              return View();
          }

          // Cancel callback
          public ActionResult Cancel()
          {
              return View("Cancel");
          }

          // Helper to prepare Line Items from the cart
          private List<SessionLineItemOptions> GetLineItemsFromCart()
          {
              var cartItems = Session["CartItems"] as List<CartItem>; // Replace with your actual cart retrieval logic
              return cartItems.Select(item => new SessionLineItemOptions
              {
                  PriceData = new SessionLineItemPriceDataOptions
                  {
                      UnitAmountDecimal = item.UnitPrice * 100, // Amount in cents
                      Currency = "usd",
                      ProductData = new SessionLineItemPriceDataProductDataOptions
                      {
                          Name = item.Product.Name,
                      },
                  },
                  Quantity = item.Quantity,
              }).ToList();
          }

          [HttpPost]
          public async Task<ActionResult> PlaceOrder(Order order)
          {
              if (ModelState.IsValid)
              {
                  var userId = User.Identity.GetUserId(); // Get the authenticated user's ID
                  var cart = db.ShoppingCarts.Include(c => c.CartItems)
                                             .FirstOrDefault(c => c.UserId == userId);
                  if (cart == null || !cart.CartItems.Any())
                  {
                      return RedirectToAction("CartEmpty", "Orders"); // Handle empty cart case
                  }

                  // Save the order
                  db.Orders.Add(order);

                  // Save order items
                  foreach (var item in cart.CartItems)
                  {
                      var stock = db.Stocks.Find(item.StockItemId); // Assuming you track stock
                      if (stock != null)
                      {
                          stock.Quantity -= item.Quantity; // Deduct stock
                      }

                      var orderItem = new OrderItem
                      {
                          Order = order,
                          StockId = item.StockItemId,
                          ProductName = item.Product.Name,
                          Size = item.Size,
                          VariantDescription = item.VariantDescription,
                          Quantity = item.Quantity,
                          UnitPrice = item.UnitPrice
                      };
                      db.OrderItems.Add(orderItem);
                  }

                  await db.SaveChangesAsync();

                  // Clear the cart after order placement
                  db.ShoppingCarts.Remove(cart);
                  await db.SaveChangesAsync();

                  return RedirectToAction("Success", "Orders");
              }

              return View("Checkout", order);
          }*/

        // GET: Orders/Details/5
        [Authorize(Roles = "Admin")]
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

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,OrderDate,Subtotal,Discount,Shipping,Total,Status,RefundReason,FirstName,LastName,Email,MobileNo,AddressLine1,AddressLine2,Country,City,Municipality,ZipCode")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,OrderDate,Subtotal,Discount,Shipping,Total,Status,RefundReason,FirstName,LastName,Email,MobileNo,AddressLine1,AddressLine2,Country,City,Municipality,ZipCode")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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

        public ActionResult OrderConfirmation(int orderId)
        {
            var order = db.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Stock))
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null || order.UserId != User.Identity.GetUserId())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }
    }
}
