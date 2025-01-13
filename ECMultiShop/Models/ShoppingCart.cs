using ECMultiShop.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; } 
        public string UserId { get; set; } 

       
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Billing Address
        public int? BillingAddressId { get; set; }

        public BillingAddress BillingAddress { get; set; }

        public decimal Shipping { get; set; }

        public decimal Subtotal
        {
            get
            {
                return CartItems.Sum(item => item.TotalPrice); 
            }
        }

    
        public decimal Total
        {
            get
            {
                return Subtotal + Shipping;
            }
        }
    }

}

public class CartItem
{
    public int Id { get; set; }
    public int ShoppingCartId { get; set; } 
    public ShoppingCart ShoppingCart { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; } 

  
    public int StockItemId { get; set; }
    public Stock StockItem { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal TotalPrice
    {
        get
        {
            return UnitPrice * Quantity; 
        }

    }

    public string Size { get; set; }
    public string VariantDescription { get; set; }
}
public class BillingAddress
{
    public int Id { get; set; } 

    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(20)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(20)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    public string MobileNo { get; set; }

    [Required(ErrorMessage = "Address Line 1 is required.")]
    [StringLength(20)]
    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; } 

    [Required(ErrorMessage = "City is required.")]
    [StringLength(20)]
    public string City { get; set; }

    [Required(ErrorMessage = "Municipality is required.")]
    [StringLength(20)]
    public string Municipality { get; set; }

    [Required(ErrorMessage = "Country is required.")]
    [StringLength(50)]
    public string Country { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    [StringLength(20, ErrorMessage = "Zip Code should be at most 20 characters long.")]
    public string ZipCode { get; set; }
}
