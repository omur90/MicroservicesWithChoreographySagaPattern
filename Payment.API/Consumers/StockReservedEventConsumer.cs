using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> logger;

        private readonly IPublishEndpoint publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            this.logger = logger;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 3000m;

            if (balance > context.Message.Payment.TotalPrice)
            {
                logger.LogInformation($"{context.Message.Payment.TotalPrice } ₺ was witdrawn from credit card for user id = {context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentCompletedEvent { BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId });
            }
            else
            {
                logger.LogInformation($"{context.Message.Payment.TotalPrice } ₺ was NOT witdrawn from credit card for user id = {context.Message.BuyerId}");

                await publishEndpoint.Publish(new PaymentFailedEvet {  OrderItems = context.Message.OrderItems, BuyerId = context.Message.BuyerId, OrderId = context.Message.OrderId, Message = "not enough balnce" });
            }
        }
    }
}
    