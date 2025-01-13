using ECMultiShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Security.Claims; 
using Microsoft.AspNet.Identity;

namespace ECMultiShop.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            List<CartItem> cartItems = new List<CartItem>(); // inicializim i 1 liste bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);
                
                // numro order historyt
                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems") 
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

              
                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }

               
                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null; 
            }
            Session.Remove("DiscountPercentage"); // nese nga karta kthehemi pas te index.cshtml ather harro kuponin per lirim tek cart.cshtml

            // viewmodel per kategorine , subkategorin dhe produktet e fundit
            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                Products = db.Products
                 .Include(p => p.Photos)
                 .Include(p => p.StockItems) 
                 .OrderByDescending(p => p.Id) // te fundit
                 .Take(4)
                 .ToList(),
                Offers = db.Offers.Where(o => o.IsActive).ToList(), // fotografite e offertave ui/ux
                CarouselItems = db.Carousels.Where(c => c.IsActive).ToList() // njejt per caruselin

            };

            return View(viewModel);
        }


        public ActionResult Shop(int? categoryId, int? subCategoryId, decimal? minPrice, decimal? maxPrice, string colors, string sizes, string sortOrder, string searchQuery, int page = 1, int pageSize = 9)
        {
            List<CartItem> cartItems = new List<CartItem>(); // inicializojme 1 list bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);



                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            
                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }


                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0;  // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null; 
            }

            var products = db.Products.Include(p => p.Photos).AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchQuery.ToLower()));
            }

            // Filter kategorit dhe subkategorit
            if (subCategoryId.HasValue)
            {
                products = products.Where(p => p.SubCategoryId == subCategoryId.Value);
                ViewBag.SubCategoryId = subCategoryId.Value;
            }
            else if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
                ViewBag.CategoryId = categoryId.Value;
            }

            // filter
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
            }

            //  filterojm ato produkte qe kan color apo size 
            if (!string.IsNullOrEmpty(colors))
            {
                var colorList = colors.Split(',');
                products = products.Where(p => p.StockItems.Any(s => colorList.Contains(s.VariantDescription)));
            }

            if (!string.IsNullOrEmpty(sizes))
            {
                var sizeList = sizes.Split(',');
                products = products.Where(p => p.StockItems.Any(s => sizeList.Contains(s.Size)));
            }

            // Sortimi 
            switch (sortOrder)
            {
                case "latest":
                    products = products.OrderByDescending(p => p.Id);
                    break;
                case "sales":
                    products = products.Where(p => p.SalePrice.HasValue).OrderByDescending(p => p.SalePrice);
                    break;
                default:
                    products = products.OrderBy(p => p.Name); // default 
                    break;
            }



            // tre dictionary per te numruar produkte bazuar n madhesi, ngjyre, dhe interval çmimi nga baza
            var sizeCounts = new Dictionary<string, int>
{
    { "XS", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.Size == "XS").Select(s => s.ProductId).Distinct().Count() },
    { "S", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.Size == "S").Select(s => s.ProductId).Distinct().Count() },
    { "M", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.Size == "M").Select(s => s.ProductId).Distinct().Count() },
    { "L", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.Size == "L").Select(s => s.ProductId).Distinct().Count() },
    { "XL", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.Size == "XL").Select(s => s.ProductId).Distinct().Count() }
};

            var colorCount = new Dictionary<string, int>
{
    { "Black", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.VariantDescription == "Black").Select(s => s.ProductId).Distinct().Count() },
    { "White", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.VariantDescription == "White").Select(s => s.ProductId).Distinct().Count() },
    { "Red", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.VariantDescription == "Red").Select(s => s.ProductId).Distinct().Count() },
    { "Blue", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.VariantDescription == "Blue").Select(s => s.ProductId).Distinct().Count() },
    { "Green", db.Stocks.Where(s => s.IsActive && s.Quantity > 0 && s.VariantDescription == "Green").Select(s => s.ProductId).Distinct().Count() }
};

            var priceRanges = new Dictionary<string, int>
{
    { "0-100", db.Products.Where(p => p.Price >= 0 && p.Price <= 100).Count() },
    { "100-200", db.Products.Where(p => p.Price > 100 && p.Price <= 200).Count() },
    { "200-300", db.Products.Where(p => p.Price > 200 && p.Price <= 300).Count() },
    { "300-400", db.Products.Where(p => p.Price > 300 && p.Price <= 400).Count() },
    { "400-500", db.Products.Where(p => p.Price > 400 && p.Price <= 500).Count() }
};

            ViewBag.PriceRanges = priceRanges;
            ViewBag.ColorCount = colorCount;
            ViewBag.SizeCounts = sizeCounts;

           //
            var totalProducts = products.Count(); // numrohen produktet nese > 9 ather new page 
            var pagedProducts = products.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            Session.Remove("DiscountPercentage");

            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                Stocks = db.Stocks.ToList(),
                Products = pagedProducts,  // 9 produktet 
                CurrentPage = page,
                PageSize = pageSize,
                TotalProducts = totalProducts
            };
           


            return View(viewModel);
        }



        public ActionResult About()
        {

            List<CartItem> cartItems = new List<CartItem>(); // inicializim i 1 liste bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                // numro order historyt
                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();


                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }


                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null;
            }
            Session.Remove("DiscountPercentage");

            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                Teams = db.Teams.Take(4).ToList() // vetem 4

            };

            return View(viewModel);
        }
        public ActionResult Contact()
        {
            List<CartItem> cartItems = new List<CartItem>(); // inicializim i 1 liste bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                // numro order historyt
                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();


                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }


                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null;
            }
            Session.Remove("DiscountPercentage");

            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
            };



            return View(viewModel);
        }
        public ActionResult Cart()
        {
            List<CartItem> cartItems = new List<CartItem>(); // inicializojm nje list boshe
            var userId = User.Identity.GetUserId(); // marrim id e userit

            if (User.Identity.IsAuthenticated)
            {
                // ata qe jan te loguar te marrin informatet e kartes nga databaza 
                cartItems = db.ShoppingCarts
                              .Include(s => s.CartItems.Select(c => c.Product)) // produktin
                              .Include(s => s.CartItems.Select(c => c.Product.Photos)) // fotografite e produktit
                              .FirstOrDefault(s => s.UserId == userId)?
                              .CartItems?.ToList() ?? new List<CartItem>(); // null cases
            }
            else
            {
                // ata qe nuk jan te loguar perdor sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                foreach (var item in cartItems)
                {
                    item.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == item.ProductId);
                }
            }

            // ata qe jan te loguar merr numrin e produkteve qe ka ne wishlist, order histori, dhe shopping cart
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
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
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
                // ata qe nuk jan te loguar perodrin sessionin cart
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

          
                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }

         
                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; 
                ViewBag.ShoppingCart = null; 
            }
            Session.Remove("DiscountPercentage");

            // Prepare the view model
            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                CartItems = cartItems 
            };


            return View(viewModel); 
        }


        public ActionResult Thanks()
        {

            List<CartItem> cartItems = new List<CartItem>(); // inicializim i 1 liste bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                // numro order historyt
                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();


                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }


                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null;
            }
            Session.Remove("DiscountPercentage");

            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList()
            };


            return View(viewModel);
        }

        public ActionResult Wishlist()
        {
            List<CartItem> cartItems = new List<CartItem>(); // inicializim i 1 liste bosh
            var userId = User.Identity.GetUserId(); // marrim useridn

            // per userat e logum merr listen e wishlistit dhe sh.cartes nga baza 
            if (User.Identity.IsAuthenticated)
            {
                // numro produktet qe ka useri ne wishlist
                ViewBag.WishlistCount = db.Wishlists
                                          .Count(w => w.UserId == userId);

                // numro order historyt
                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                // per secilen shporte te ngarkohen automatikisht te gjithe artikujt e lidhur nga tabela CartItems
                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    // shfaqim detajet e produkteve per secilin artikul ne shopping cart
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Find(cartItem.ProductId);
                    }

                    // kalkulojme numrin e produkteve ne sh.cart (sum of all quantities)
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
                // ata qe nuk jan te loguar perdorin sesion
                cartItems = Session["Cart"] as List<CartItem> ?? new List<CartItem>();


                foreach (var cartItem in cartItems)
                {
                    cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                }


                ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);

                ViewBag.WishlistCount = 0; // ata qe nuk jan te loguar gjithmon 0 wishlista
                ViewBag.ShoppingCart = null;
            }



            // shfaqim produktet qe userat kan ne wishlist
            var wishlistItems = db.Wishlists
          .Where(w => w.UserId == userId)
          .Select(w => w.Product)
          .Include(p => p.Photos) // Include fotografite
          .Include(p => p.StockItems) // stock item , per detajet 
          .ToList();
            Session.Remove("DiscountPercentage");

            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                Products = wishlistItems // shfaqim produktet

            };

            return View(viewModel);
        }
        [HttpPost]
        public ActionResult StoreDiscount(decimal discountPercentage)
        {
            Session["DiscountPercentage"] = discountPercentage;
            return Json(new { success = true });
        }

        [Authorize]
        public ActionResult Checkout()
        {
            List<CartItem> cartItems;
            var userId = User.Identity.GetUserId();
            var userEmail = User.Identity.GetUserName();  // marrim emailin e userit
            decimal shipping = 5.0m;

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.WishlistCount = db.Wishlists.Count(w => w.UserId == userId);

                int totalOrders = db.Orders.Count(o => o.UserId == userId);
                ViewBag.TotalOrders = totalOrders;

                var shoppingCart = db.ShoppingCarts
                                     .Include("CartItems")
                                     .FirstOrDefault(sc => sc.UserId == userId);

                if (shoppingCart != null)
                {
                    foreach (var cartItem in shoppingCart.CartItems)
                    {
                        cartItem.Product = db.Products.Include(p => p.Photos).FirstOrDefault(p => p.Id == cartItem.ProductId);
                    }
                    cartItems = shoppingCart.CartItems.ToList();
                    ViewBag.TotalProducts = cartItems.Sum(ci => ci.Quantity);
                }
                else
                {
                    cartItems = new List<CartItem>();
                    ViewBag.TotalProducts = 0;
                }
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
            }

            // kalkulojm totali
            decimal subtotal = cartItems.Sum(ci => ci.UnitPrice * ci.Quantity);

            // shikojm nese useri ka perdorer kuponin apo perqindjen e lirimit , perndryshe 0$
            decimal discountPercentage = Session["DiscountPercentage"] != null ? (decimal)Session["DiscountPercentage"] : 0;

            // kalkulojm me kuponin (nese)
            decimal discountAmount = subtotal * (discountPercentage / 100);
            decimal total = subtotal + shipping - discountAmount;

            // dergojm ne view te dhenat
            ViewBag.Subtotal = subtotal;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.Total = total;
            



            /*Session.Remove("DiscountPercentage");*/
            var viewModel = new CategoryViewModel
            {
                Categories = db.Categories.ToList(),
                SubCategories = db.SubCategories.ToList(),
                CartItems = cartItems.ToList(),
                BillingAddress = new BillingAddress
                {
                    Email = userEmail // shfaqim emailin e userit ne view
                }

            };

            return View(viewModel);
        }

        public ActionResult Success()
        {

            string userId = User.Identity.GetUserId();

            // marrim shopping karten
            var shoppingCart = db.ShoppingCarts
                .Include(sc => sc.CartItems.Select(ci => ci.StockItem))
                .Include(sc => sc.BillingAddress)
                .FirstOrDefault(sc => sc.UserId == userId);

            if (shoppingCart == null || !shoppingCart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty or the payment failed.";
                return RedirectToAction("Index", "Cart");
            }

            // marrim discount perqindjen nga sesioni perndyshe 0$
            decimal discountPercentage = Session["DiscountPercentage"] != null ? (decimal)Session["DiscountPercentage"] : 0;

            // kalkulimi i fundit
            decimal discountAmount = shoppingCart.Subtotal * (discountPercentage / 100);
            decimal totalAmount = shoppingCart.Subtotal - discountAmount + shoppingCart.Shipping;

            var cartItems = db.CartItems
    .Include(ci => ci.StockItem.Product)
    .ToList();



            // order objekti
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Subtotal = shoppingCart.Subtotal,
                Discount = discountAmount,
                Shipping = shoppingCart.Shipping = 5,
                Total = totalAmount + shoppingCart.Shipping,

                // adresa
                FirstName = shoppingCart.BillingAddress.FirstName,
                LastName = shoppingCart.BillingAddress.LastName,
                Email = shoppingCart.BillingAddress.Email,
                MobileNo = shoppingCart.BillingAddress.MobileNo,
                AddressLine1 = shoppingCart.BillingAddress.AddressLine1,
                AddressLine2 = shoppingCart.BillingAddress.AddressLine2,
                Country = shoppingCart.BillingAddress.Country,
                City = shoppingCart.BillingAddress.City,
                Municipality = shoppingCart.BillingAddress.Municipality,
                ZipCode = shoppingCart.BillingAddress.ZipCode,
            };

            // Mapimi i artikujve ne orderitem 
            foreach (var cartItem in shoppingCart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    StockId = cartItem.StockItemId,
                    /*          ProductName = cartItem.StockItem.ProductId.ToString(),*/
                    ProductName = cartItem.StockItem.Product.Name,
                    Size = cartItem.Size,
                    VariantDescription = cartItem.VariantDescription,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    Order = order
                };

                // e minusojme stock levelin
                cartItem.StockItem.Quantity -= cartItem.Quantity;

                order.OrderItems.Add(orderItem);
            }

            // ruajm porosine ne baze
            db.Orders.Add(order);

            // fshijme karten per at user dhe ruajme te dhenat
            db.CartItems.RemoveRange(shoppingCart.CartItems);
            db.ShoppingCarts.Remove(shoppingCart);

            db.SaveChanges();
            Session.Remove("DiscountPercentage");


            return View();
        }
        [Authorize]
        public ActionResult OrderHistory()
        {
            string userId = User.Identity.GetUserId();

            // marrim orders prej bazes
            var orders = db.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Total,
                    o.Status,
                    Items = o.OrderItems.Select(oi => new
                    {
                        oi.ProductName,
                        oi.Size,
                        oi.VariantDescription,
                        oi.Quantity,
                        oi.UnitPrice
                    }).ToList()
                })
                .ToList() 
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.Id,
                    OrderDate = o.OrderDate,
                    Total = o.Total,
                    Status = o.Status,
                    Items = o.Items.Select(oi => new OrderHistoryItemViewModel
                    {
                        ProductName = oi.ProductName,
                        Size = oi.Size,
                        Color = oi.VariantDescription,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.UnitPrice * oi.Quantity 
                    }).ToList()
                })
                .ToList();

         


            return View(orders);
        }

        public ActionResult Refund(int orderId, string reason)
        {
            // marrim orderin nga id-ja
            var order = db.Orders
                .Include(o => o.OrderItems.Select(oi => oi.Stock))
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                ViewBag.ErrorMessage = "Order not found.";
                return View("Index"); 
            }

            // shikojm nese orderi eshte kthyer me heret
            if (order.Status == OrderStatus.Refunded)
            {
                ViewBag.ErrorMessage = "This order has already been refunded.";
                return View("Details", order); 
            }

            // Restore stock levels per secilin order item
            foreach (var orderItem in order.OrderItems)
            {
                var stockItem = db.Stocks.FirstOrDefault(si => si.Id == orderItem.StockId);
                if (stockItem != null)
                {
                    stockItem.Quantity += orderItem.Quantity; // Restore stock
                }
            }

            // ruajm refund reason
            order.RefundReason = reason;
            order.Status = OrderStatus.Refunded;
            db.SaveChanges();

            TempData["SuccessMessage"] = $"Refund for order #{orderId} has been processed successfully.";


            return RedirectToAction("OrderHistory");

        }


        [Authorize(Roles = "Admin")]
        public ActionResult Admin()
        {
            var userId = User.Identity.GetUserId();

            var user = db.Users.Find(userId);

            if (user != null)
            {
                // shfaqim emailin e userit ne view
                ViewBag.UserName = user.UserName; 
            }
            else
            {
                ViewBag.UserName = "Unknown User";
            }

            using (var db = new ApplicationDbContext())
            {
                var toDoTasks = db.ToDoTasks.OrderByDescending(t => t.CreatedAt).ToList();

                // marrim total sales 
                var totalSales = db.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .Sum(o => o.Total);

                // marrim te dhena
                var totalOrders = db.Orders.Count();
                var refundedOrders = db.Orders.Count(o => o.Status == OrderStatus.Refunded);

                var completedOrdersByCountry = db.Orders
                    .Where(o => o.Status == OrderStatus.Completed) // filtrojm per ordera qe jan kryer me sukses
                    .GroupBy(o => o.Country) // i grupojm shtetet
                    .Select(g => new
                    {
                        Country = g.Key, // emrin e shtetit
                        OrderCount = g.Count() // i numrojm nga ai shtet orderat
                    })
                    .OrderByDescending(g => g.OrderCount) 
                    .ToList();

                // marrim daten e sotme dhe llogarisim kohen
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // marrim today's sales 
                var todaySales = db.Orders
                    .Where(o => o.Status == OrderStatus.Completed && o.OrderDate >= today && o.OrderDate < tomorrow)
                    .Sum(o => (decimal?)o.Total) ?? 0; // null = 0

                // marrim total sales 
                var totalShitje = db.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .Sum(o => (decimal?)o.Total) ?? 0;

                // marrim recent orders
                var recentOrders = db.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .Select(o => new RecentOrderViewModel
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        CustomerName = o.FirstName + " " + o.LastName,
                        Total = o.Total,
                        Status = o.Status.ToString(), // Convert enum to string if necessary
                        Items = o.OrderItems.Select(i => new OrderItemViewModel
                        {
                            ProductName = i.ProductName,
                            Size = i.Size,
                            Quantity = i.Quantity
                        }).ToList()
                    })
                    .ToList();


                // marrim userat qe na kan kontaktuar (5)
                var latestMessages = db.Contacts
       .OrderByDescending(c => c.ContactID)
       .Take(5)
       .ToList()
       .Select(c => new MessageViewModel
       {
           Name = c.Name,
           Email = c.Email,
           Message = c.Message,
           MinutesAgo = GetTimeAgo(c.CreatedAt)
       })
       .ToList();

                var unreadNotifications = db.Notifications
           .Where(n => !n.IsRead)
           .OrderByDescending(n => n.CreatedAt)
           .Take(10) // 10 njofrimet e fundit (descending id)
           .ToList();

                // marrim low stock informatet
                var lowStockProducts = db.Stocks
                    .Where(s => s.Quantity <= 5 && s.IsActive) 
                    .Select(s => new LowStockProductViewModel
                    {
                        ProductName = s.Product.Name,
                        StockQuantity = s.Quantity
                    })
                    .OrderBy(s => s.StockQuantity)
                    .ToList();


                // i pasojme te dhenat ne viewbag qe ti perdorim ne view
                ViewBag.ToDoTasks = toDoTasks;
                ViewBag.TodaySales = todaySales;
                ViewBag.TotalSales = totalShitje;
                ViewBag.TotalOrders = totalOrders;
                ViewBag.RefundedOrders = refundedOrders;
                ViewBag.CompletedOrdersByCountry = completedOrdersByCountry;
                ViewBag.RecentOrders = recentOrders;
                ViewBag.LatestMessages = latestMessages; 
                ViewBag.UnreadNotifications = unreadNotifications;
                ViewBag.LowStockProducts = lowStockProducts; 


            }

            return View();
        }
        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";

            return $"{(int)(timeSpan.TotalDays / 30)} months ago";
        }


    }
}