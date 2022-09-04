using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext appDbContext;
        private ILogger<OrderCreatedEventConsumer> logger;
        private ISendEndpointProvider sendEndpointProvider;
        private IPublishEndpoint publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            this.appDbContext = appDbContext;
            this.logger = logger;
            this.sendEndpointProvider = sendEndpointProvider;
            this.publishEndpoint = publishEndpoint;
        }
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stockResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stockResult.Add(await appDbContext.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }

            if (stockResult.All(x => x.Equals(true))) //stock tarafında hepsi ok
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await appDbContext.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);

                    if (stock != null)
                    {
                        stock.Count -= item.Count;
                    }

                    await appDbContext.SaveChangesAsync();
                }

                logger.LogInformation($"Stock was reserved for Buyer Id : {context.Message.BuyerId}");

                var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    OrderItems = context.Message.OrderItems,
                    Payment = context.Message.Payment
                };

                await sendEndpoint.Send(stockReservedEvent);
            }
            else
            {
                await publishEndpoint.Publish(new StockNotReservedEvent
                {

                    OrderId = context.Message.OrderId,
                    Message = "Not enough stock"
                });

                logger.LogInformation($"Stock was not reserved for Buyer Id : {context.Message.BuyerId}");
            }
        }
    }
}
