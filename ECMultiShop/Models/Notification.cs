using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } 
        public string Message { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false; 
    }

}