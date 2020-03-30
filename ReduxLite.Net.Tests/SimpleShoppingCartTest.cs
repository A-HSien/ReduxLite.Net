using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReduxLite.Net.Tests
{
    public class SimpleShoppingCartTest
    {
        IQueryable<Action<Guid>> _source = new List<Action<Guid>>() { }.AsQueryable();
        Store<OrderTest, Guid> _orderStore = new Store<OrderTest, Guid>();

        public SimpleShoppingCartTest()
        {
            _orderStore.Set(
                () => _source,
                action => _source = _source.AsEnumerable().Add(action).AsQueryable());

            _orderStore.Register(new Reducer<CreateOrderTestAction, Guid, OrderTest>(
                (action, order) =>
                {
                    return new OrderTest { Id = action.OrderIdToSet };
                }));

            _orderStore.Register(new Reducer<ChangeOrderStatusTestAction, Guid, OrderTest>(
                (action, order) =>
                {
                    order.OrderStatus = action.OrderStatus;
                    return order;
                }));

            _orderStore.Register(new Reducer<ModifyProductItemTestAction, Guid, OrderTest>(
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

        [Fact]
        public void StoreTest()
        {
            var orderId = Guid.NewGuid();
            _orderStore.Dispatch(new CreateOrderTestAction { OrderIdToSet = orderId });
            var createdOrder = _orderStore.GetState(orderId);
            Assert.NotNull(createdOrder);
            Assert.Equal(orderId, createdOrder.Id);


            var itemToSet = new ProductItemTest
            {
                ProductId = Guid.NewGuid(),
                Count = 5
            };
            _orderStore.Dispatch(new ModifyProductItemTestAction { TargetId = orderId, ProductItem = itemToSet });
            var afterModifyProductItemAction = _orderStore.GetState(orderId);
            Assert.NotNull(afterModifyProductItemAction);
            var savedItem = afterModifyProductItemAction.ProductItems.FirstOrDefault();
            Assert.NotNull(savedItem);
            Assert.Equal(itemToSet, savedItem);


            var statusToSet = OrderTestStatus.Paid;
            _orderStore.Dispatch(new ChangeOrderStatusTestAction { TargetId = orderId, OrderStatus = statusToSet });
            var afterChangeOrderStatusAction = _orderStore.GetState(orderId);
            Assert.NotNull(afterChangeOrderStatusAction);
            Assert.Equal(statusToSet, afterChangeOrderStatusAction.OrderStatus);
        }

        [Fact]
        public void MonitorTest()
        {
            var statusChanged = false;
            _orderStore.AttachMonitor((lastest, modified) =>
            {
                statusChanged = lastest.OrderStatus != modified.OrderStatus;
            });

            var targetId = Guid.NewGuid();
            var statusToSet = OrderTestStatus.Paid;
            _orderStore.Dispatch(new ChangeOrderStatusTestAction { TargetId = targetId, OrderStatus = statusToSet });
            Assert.True(statusChanged);
        }
    }


    enum OrderTestStatus
    {
        None, Confirmed, Paid, Shipped
    }


    class OrderTest
    {
        public Guid Id { get; set; }

        public IEnumerable<ProductItemTest> ProductItems { get; set; } = new List<ProductItemTest>();

        public OrderTestStatus OrderStatus { get; set; } = OrderTestStatus.None;

        public double DiscountRate { get; set; } = 1;
    }


    class ProductItemTest
    {
        public Guid ProductId { get; set; }
        public int Count { get; set; }
    }



    class ChangeOrderStatusTestAction : Action<Guid>
    {
        public OrderTestStatus OrderStatus { get; set; }
    }

    class ModifyProductItemTestAction : Action<Guid>
    {
        public ProductItemTest ProductItem { get; set; }
    }

    class CreateOrderTestAction : Action<Guid>
    {
        public override Guid TargetId => this.OrderIdToSet;
        public Guid OrderIdToSet { get; set; }
    }

    static class CollectionExtensions
    {
        public static IEnumerable<T> Add<T>(this IEnumerable<T> items, T toAdd)
        {
            return items.Concat(new[] { toAdd });
        }
    }
}
