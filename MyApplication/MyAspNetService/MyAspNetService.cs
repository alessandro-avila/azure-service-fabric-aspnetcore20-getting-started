using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;
using System.IO;

namespace MyAspNetService
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class MyAspNetService : StatelessService
    {
        public MyAspNetService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                    new ServiceInstanceListener(serviceContext =>
                        new WebListenerCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                        {
                                ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting WebListener on {url}");

                                return new WebHostBuilder()
                                    .UseHttpSys(
                                        options =>
                                            {
                                                options.Authentication.Schemes = AuthenticationSchemes.Negotiate; // Microsoft.AspNetCore.Server.HttpSys
                                                options.Authentication.AllowAnonymous = false;
                                                /* Additional options */
                                                //options.MaxConnections = 100;
                                                //options.MaxRequestBodySize = 30000000;
                                                //options.UrlPrefixes.Add("http://localhost:5000");
                                            }
                                    )
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}

