﻿
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using Stock.API.Consumers;
using Stock.API.Models;

namespace Stock.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<OrderCreatedEventConsumer>();
                x.AddConsumer<PaymentFailedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(Configuration.GetConnectionString("RabbitMQ"));

                    cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockOrderCreatedEventQueueName, e => {
                        e.ConfigureConsumer<OrderCreatedEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint(RabbitMQSettingsConst.StockPaymentFailedEventQueueName, e =>
                    {
                        e.ConfigureConsumer<PaymentFailedEventConsumer>(context);
                    });
                });
            });
            
            services.AddMassTransitHostedService();

            services.AddDbContext<AppDbContext>(options => {
                options.UseInMemoryDatabase("StockDb");
            });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
