using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ReduxLite.Net.Tests.SimpleShoppingCart
{
    public class Tests
    {
        IQueryable<Action<Guid>> _source = new List<Action<Guid>>() { }.AsQueryable();
        OrderStore _orderStore = new OrderStore();

        public Tests()
        {
            _orderStore.Set(
                () => _source,
                action => _source = _source.AsEnumerable().Add(action).AsQueryable());
        }

        [Fact]
        public void StoreTest()
        {
            var orderId = Guid.NewGuid();
            _orderStore.Dispatch(new CreateOrderAction { OrderIdToSet = orderId });
            var createdOrder = _orderStore.GetState(orderId);
            Assert.NotNull(createdOrder);
            Assert.Equal(orderId, createdOrder.Id);


            var itemToSet = new ProductItem
            {
                ProductId = Guid.NewGuid(),
                Count = 5
            };
            _orderStore.Dispatch(new ModifyProductItemAction { TargetId = orderId, ProductItem = itemToSet });
            var afterModifyProductItemAction = _orderStore.GetState(orderId);
            Assert.NotNull(afterModifyProductItemAction);
            var savedItem = afterModifyProductItemAction.ProductItems.FirstOrDefault();
            Assert.NotNull(savedItem);
            Assert.Equal(itemToSet, savedItem);


            var statusToSet = OrderStatus.Paid;
            _orderStore.Dispatch(new ChangeOrderStatusAction { TargetId = orderId, OrderStatus = statusToSet });
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
            var statusToSet = OrderStatus.Paid;
            _orderStore.Dispatch(new ChangeOrderStatusAction { TargetId = targetId, OrderStatus = statusToSet });
            Assert.True(statusChanged);
        }
    }

 
    static class CollectionExtensions
    {
        public static IEnumerable<T> Add<T>(this IEnumerable<T> items, T toAdd)
        {
            return items.Concat(new[] { toAdd });
        }
    }
}
