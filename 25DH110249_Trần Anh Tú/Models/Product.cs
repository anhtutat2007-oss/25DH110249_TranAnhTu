namespace Warehouse_25DH110249.Models
{
    using System;
    using System.Collections.Generic;

    public partial class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}