using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int SubCategoryId { get; set; }
        public SubCategory SubCategory { get; set; }

        public ICollection<ProductPhoto> Photos { get; set; } = new List<ProductPhoto>();
        public ICollection<Stock> StockItems { get; set; } = new List<Stock>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public Gender Gender { get; set; }
    }

    public class Stock
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string Size { get; set; }

   
        [Display(Name = "Color")]
        public string VariantDescription { get; set; } 

        public int Quantity { get; set; }

        public decimal? Price { get; set; } 
        public decimal? SalePrice { get; set; } 
        public bool IsActive { get; set; }


    }

    public class ProductPhoto
    {
        public int Id { get; set; }
        public string ProductImage { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
    public class LowStockProductViewModel
    {
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
    }

}
public enum Gender
{
    Male,
    Female,
    Unisex,
    NULL
}
