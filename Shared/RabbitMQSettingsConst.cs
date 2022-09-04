using System;
namespace Shared
{
    public class RabbitMQSettingsConst
    {
        public const string StockReservedEventQueueName = "stock-reserved-queue";
        public const string StockNotReservedEventQueueName = "stock-not-reserved-queue";
        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string OrderPaymentCompletedQueueName = "order-completed-queue";
        public const string OrderPaymentFailedEventQueueName = "order-failed-queue";
        public const string StockPaymentFailedEventQueueName = "stock-payment-failed-queue";
    }
}
