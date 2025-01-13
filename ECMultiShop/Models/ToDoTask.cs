using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ECMultiShop.Models
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public string Task { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}