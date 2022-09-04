using System;
using System.Threading.Tasks;
using MassTransit;
using Shared.Events;
using Order.API.Models;
using Microsoft.Extensions.Logging;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer : IConsumer<StockNotReservedEvent>
    {
        private readonly AppDbContext appDbContext;
        private readonly ILogger<StockNotReservedEventConsumer> logger;

        public StockNotReservedEventConsumer(AppDbContext appDbContext, ILogger<StockNotReservedEventConsumer> logger)
        {
            this.logger = logger;
            this.appDbContext = appDbContext;
        }

        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var order = await appDbContext.Orders.FindAsync(context.Message.OrderId);

            if (order == null)
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

