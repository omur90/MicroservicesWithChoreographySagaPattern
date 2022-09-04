using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;
using Order.API.Models;
using Shared.Events;

namespace Order.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly AppDbContext context;
        private readonly IPublishEndpoint publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            this.context = context;
            this.publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
        {
            var newOrder = new Models.Order
            {
                BuyerId = orderCreateDto.BuyerId,
                CreatedDate = DateTime.Now,
                Address = new Address
                {
                    District = orderCreateDto.Address.District,
                    Line = orderCreateDto.Address.Line,
                    Province = orderCreateDto.Address.Province

                }
            };

            orderCreateDto.OrderItems.ForEach(x =>
            {
                newOrder.Items.Add(new OrderItem { Price = x.Price, Count = x.Count, ProductId = x.ProductId });

            });


            await context.AddAsync(newOrder);

            await context.SaveChangesAsync();

            var orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = orderCreateDto.BuyerId,
                OrderId = newOrder.Id,
                Payment = new Shared.Messages.PaymentMessage
                {
                    CardName = orderCreateDto.Payment.CardName,
                    CardNumber = orderCreateDto.Payment.CardNumber,
                    CVV = orderCreateDto.Payment.CVV,
                    Expiration = orderCreateDto.Payment.Expiration,
                    TotalPrice = orderCreateDto.OrderItems.Sum(x=> x.Price * x.Count)
                }
            };

            orderCreateDto.OrderItems.ForEach(x =>
            {
                orderCreatedEvent.OrderItems.Add(new Shared.Messages.OrderItemMessage { Count = x.Count, ProductId = x.ProductId });
            });

            await publishEndpoint.Publish(orderCreatedEvent);

            return Ok();
        }

    }
}
