// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace EmptyBot1
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service. Transient lifetime services are created
    /// each time they're requested. Objects that are expensive to construct, or have a lifetime
    /// beyond a single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class EmptyBot1Bot : IBot
    {
        private readonly EmptyBot1Accesors _accessors;
        private readonly ILogger _logger;
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>                        
        public EmptyBot1Bot(ConversationState conversationState, ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _accessors = new EmptyBot1Accesors(conversationState)
            {
                CounterState = conversationState.CreateProperty<CounterState>(EmptyBot1Accesors.CounterStateName),
            };

            _logger = loggerFactory.CreateLogger<EmptyBot1Bot>();
            _logger.LogTrace("EmptyBot1 turn start.");
        }

        /// <summary>
        /// Every conversation turn calls this method.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // Handle Message activity type, which is the main activity type for shown within a conversational interface
            // Message activities may contain text, speech, interactive cards, and binary or unknown attachments.
            // see https://aka.ms/about-bot-activity-message to learn more about the message and other activity types

            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                await turnContext.SendActivityAsync("Hello World Update", cancellationToken: cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                // Get the conversation state from the turn context.
                var oldState = await _accessors.CounterState.GetAsync(turnContext, () => new CounterState());

                // Bump the turn count for this conversation.
                var newState = new CounterState { TurnCount = oldState.TurnCount + 1 };

                // Set the property using the accessor.
                await _accessors.CounterState.SetAsync(turnContext, newState);

                // Save the new turn count into the conversation state.
                await _accessors.ConversationState.SaveChangesAsync(turnContext);

                if (turnContext.Activity.Text == "Colors")
                {
                    var reply = turnContext.Activity.CreateReply("What is your favorite color?");

                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                            {
                              new CardAction() { Title = "Red", Type = ActionTypes.ImBack, Value = "Red" },
                              new CardAction() { Title = "Yellow", Type = ActionTypes.ImBack, Value = "Yellow" },
                              new CardAction() { Title = "Blue", Type = ActionTypes.ImBack, Value = "Blue" },
                             },
                    };
                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                else
                {
                    // Send Hello World back to the user      
                    await turnContext.SendActivityAsync("Current Count: " + newState.TurnCount, cancellationToken: cancellationToken);
                }
            }
        }
    }
}
