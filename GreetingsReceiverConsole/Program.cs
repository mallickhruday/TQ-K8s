#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using System.Threading.Tasks;
using Greetings.Adapters.ServiceHost;
using Greetings.Ports.CommandHandlers;
using Greetings.Ports.Commands;
using Greetings.Ports.Mappers;
using Greetings.TinyIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ;
using Paramore.Brighter.MessagingGateway.RMQ.MessagingGatewayConfiguration;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection;
using Paramore.Brighter.ServiceActivator.Extensions.Hosting;
using Polly;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

namespace GreetingsReceiverConsole
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            var host = new HostBuilder()
                .ConfigureServices((hostContext, services) =>

                {
                    var connections = new Connection[]
                    {
                        new Connection<GreetingEvent>(
                            new ConnectionName("paramore.example.greeting"),
                            new ChannelName("greeting.event"),
                            new RoutingKey("greeting.event"),
                            timeoutInMilliseconds: 200,
                            isDurable: true,
                            highAvailability: true)
                    };

                    var rmqConnnection = new RmqMessagingGatewayConnection
                    {
                        //AmpqUri  = new AmqpUriSpecification(new Uri("amqp://myuser:mypass@localhost:5672/%2f")),
                        AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@rabbitmq:5672/%2f")),
                        Exchange = new Exchange("paramore.brighter.exchange"),
                    };

                    var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnnection);

                    services.AddServiceActivator(options =>
                    {
                        options.Connections = connections;
                        options.ChannelFactory = new InputChannelFactory(rmqMessageConsumerFactory);
                    })
                    .MapperRegistryFromAssemblies(typeof(GreetingEventHandler).Assembly)
                    .HandlersFromAssemblies(typeof(GreetingEventHandler).Assembly);

                    services.AddSingleton<ILoggerFactory>(x => new SerilogLoggerFactory());
                    services.AddHostedService<ServiceActivatorHostedService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
