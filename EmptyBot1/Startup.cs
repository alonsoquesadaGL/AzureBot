﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmptyBot1
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {

        private ILoggerFactory _loggerFactory;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> specifies the contract for a collection of service descriptors.</param>
        /// <seealso cref="IStatePropertyAccessor{T}"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/dependency-injection"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0"/>
        public void ConfigureServices(IServiceCollection services)
        {
            // The Memory Storage used here is for local bot debugging only. When the bot
            // is restarted, everything stored in memory will be gone.
            IStorage dataStore = new MemoryStorage();

            // Create Conversation State object.
            // The Conversation State object is where we persist anything at the conversation-scope.
            var conversationState = new ConversationState(dataStore);
            services.AddSingleton(conversationState);

            services.AddBot<EmptyBot1Bot>(options =>
           {
               // Creates a logger for the application to use.
               ILogger logger = _loggerFactory.CreateLogger<EmptyBot1Bot>();
               var secretKey = Configuration.GetSection("botFileSecret")?.Value;

               // Loads .bot configuration file and adds a singleton that your Bot can access through dependency injection.
               var botConfig = BotConfiguration.Load(@".\EmptyBot1.bot", secretKey);
               services.AddSingleton(sp => botConfig);

               // Retrieve current endpoint.
               var service = botConfig.Services.Where(s => s.Type == "endpoint" && s.Name == "development").FirstOrDefault();
               if (!(service is EndpointService endpointService))
               {
                   throw new InvalidOperationException($"The .bot file does not contain a development endpoint.");
               }

               options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

               // Catches any errors that occur during a conversation turn and logs them.
               options.OnTurnError = async (context, exception) =>
              {
                  await context.SendActivityAsync("Sorry, it looks like something went wrong.");
              };


           });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
