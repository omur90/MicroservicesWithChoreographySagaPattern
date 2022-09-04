using System;
using System.Collections.Generic;
using Shared.Messages;

namespace Shared.Events
{
    public class StockReservedEvent
    {
        public int OrderId { get; set; }
        public  string BuyerId { get; set; }

        public PaymentMessage Payment { get; set; }

        public List<OrderItemMessage> OrderItems { get; set; }
    }
}
