using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Shipping { get; set; } = 5.00m;
        public decimal Total { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Completed;

        public string RefundReason { get; set; } 

        // Billing details
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Municipality { get; set; }
        public string ZipCode { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int StockId { get; set; }
        public Stock Stock { get; set; }

        public string ProductName { get; set; }  
        public string Size { get; set; }  
        public string VariantDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  
        public decimal TotalPrice => UnitPrice * Quantity;
    }

    public enum OrderStatus
    {
        Completed,   
        Refunded     
    }

    public class RecentOrderViewModel
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }

        public string Address1 { get; set; } 

        public string Address2 { get; set; }

        public string MobilePhone { get; set; } 




        public List<OrderItemViewModel> Items { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
    }

}