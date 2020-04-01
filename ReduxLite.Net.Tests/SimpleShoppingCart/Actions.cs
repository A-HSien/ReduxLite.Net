using System;
using System.Collections.Generic;
using System.Text;

namespace ReduxLite.Net.Tests.SimpleShoppingCart
{ 
    internal class ChangeOrderStatusAction : Action<Guid>
    {
        public OrderStatus OrderStatus { get; set; }
    }

    internal class ModifyProductItemAction : Action<Guid>
    {
        public ProductItem ProductItem { get; set; }
    }

    internal class CreateOrderAction : Action<Guid>
    {
        public override Guid TargetId => this.OrderIdToSet;
        public Guid OrderIdToSet { get; set; }
    }

}
