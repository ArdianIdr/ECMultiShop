using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class CategoryViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<SubCategory> SubCategories { get; set; }

        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Stock> Stocks { get; set; } = new List<Stock>(); 
        public Contact Contact { get; set; }
         
        public IEnumerable<Team> Teams { get; set; }
        public int? SelectedCategoryId { get; set; }
        public int? SelectedSubCategoryId { get; set; }
        public IEnumerable<Product> RelatedProducts { get; set; }  
        public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
        public IEnumerable<CartItem> CartItems { get; set; } = new List<CartItem>();  

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalProducts { get; set; }

  
        public int TotalPages => (int)Math.Ceiling((decimal)TotalProducts / PageSize);
        public BillingAddress BillingAddress { get; set; }
        public IEnumerable<Order> Orders{ get; set; }

        public List<Offer> Offers { get; set; } 
        public List<Carousel> CarouselItems { get; set; } 








    }

}