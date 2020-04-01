using System;
using System.Collections.Generic;
using System.Text;

namespace ReduxLite.Net.Tests.SimpleShoppingCart
{
   internal class Order
    {
        public Guid Id { get; set; }

        public IEnumerable<ProductItem> ProductItems { get; set; } = new List<ProductItem>();

        public OrderStatus OrderStatus { get; set; } = OrderStatus.None;

        public double DiscountRate { get; set; } = 1;
    }

    internal enum OrderStatus
    {
        None, Confirmed, Paid, Shipped
    }

    internal class ProductItem
    {
        public Guid ProductId { get; set; }
        public int Count { get; set; }
    }
}
