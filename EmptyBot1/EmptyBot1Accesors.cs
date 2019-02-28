using Microsoft.Bot.Builder;
using System;

namespace EmptyBot1
{
    public class EmptyBot1Accesors
    {
        public ConversationState ConversationState { get; }
        public IStatePropertyAccessor<CounterState> CounterState { get; set; }
        public static string CounterStateName { get; } = $"{nameof(EmptyBot1Accesors)}.CounterState";
        public EmptyBot1Accesors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }
    }
}
