using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvet>
    {
        private readonly AppDbContext appDbContext;

        private readonly ILogger<PaymentFailedEventConsumer> logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<PaymentFailedEventConsumer> logger)
        {
            this.logger = logger;
            this.appDbContext = appDbContext;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvet> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                var orderItem = await appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                if (orderItem == null)
                {
                    continue;
                }

                orderItem.Count += item.Count;
                appDbContext.Update(orderItem);
                await appDbContext.SaveChangesAsync();
            }

            logger.LogInformation($"Payment failed cause stock count get back for items {context.Message.OrderId}, BuyerId : {context.Message.BuyerId}");
        }
    }
}

