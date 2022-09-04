using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<PaymentCompletedEventConsumer> logger;

        public PaymentCompletedEventConsumer(AppDbContext appDbContext, ILogger<PaymentCompletedEventConsumer> logger)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order != null)
            {
                order.Status = Enums.OrderStatusEnum.Completed;
                appDbContext.Update(order);

                await appDbContext.SaveChangesAsync();

                logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed => {order.Status}");
            }
            else
            {
                logger.LogError($"Order (Id={context.Message.OrderId}) not found !");
            }
        }
    }
}

