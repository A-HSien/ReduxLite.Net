using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReduxLite.Net.Tests.SimpleShoppingCart
{
    internal class OrderStore : Store<Order, Guid>
    { 
        internal OrderStore()
        { 
            Register(new Reducer<CreateOrderAction, Guid, Order>(
                   (action, order) =>
                   {
                       return new Order { Id = action.OrderIdToSet };
                   }));

            Register(new Reducer<ChangeOrderStatusAction, Guid, Order>(
                (action, order) =>
                {
                    order.OrderStatus = action.OrderStatus;
                    return order;
                }));

            Register(new Reducer<ModifyProductItemAction, Guid, Order>(
                (action, order) =>
                {
                    var productItem = action.ProductItem;
                    if (productItem.Count <= 0)
                    {
                        order.ProductItems = order.ProductItems.Where(i => i.ProductId != productItem.ProductId);
                        return order;
                    }

                    var toModify = order.ProductItems
                        .Where(i => i.ProductId == action.ProductItem.ProductId)
                        .FirstOrDefault();


                    if (toModify != null) toModify.Count = productItem.Count;
                    else if (toModify == null && productItem.Count > 0)
                    {
                        var productItems = order.ProductItems.ToList();
                        productItems.Add(productItem);
                        order.ProductItems = productItems;
                    }
                    return order;
                }));
        }
    }
}
