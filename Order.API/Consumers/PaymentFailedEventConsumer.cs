using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvet>
    {
        private readonly ILogger<PaymentFailedEventConsumer> logger;
        private readonly AppDbContext appDbContext;

        public PaymentFailedEventConsumer(ILogger<PaymentFailedEventConsumer> logger, AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvet> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order ==null)
            {
                logger.LogError($"Order (Id : {context.Message.OrderId}) not found !");
                return;
            }

            order.Status = Enums.OrderStatusEnum.Fail;
            appDbContext.Update(order);

            logger.LogError(context.Message.Message);
            logger.LogInformation($"Order (Id : {context.Message.OrderId}) status changed : {order.Status}");

            await appDbContext.SaveChangesAsync();
        }
    }
}

