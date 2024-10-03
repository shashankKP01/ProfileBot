// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.22.0

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using ProfileBot.Cards;

namespace ProfileBot.Bots
{
    public class EchoBot<T> : ActivityHandler where T: Dialog
    {
        protected readonly Dialog Dialog;
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;
        protected readonly ILogger Logger;
        
        public EchoBot(ConversationState conversationState,UserState userState,T dialog,ILogger<EchoBot<T>> logger)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog= dialog;
            Logger= logger;
        }
        public override async Task OnTurnAsync(ITurnContext turnContext,CancellationToken cancellationToken = default)
        {
            try
            {
                await base.OnTurnAsync(turnContext, cancellationToken);
                await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex,"");
                throw;
            }
           
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Running dialog with Message Activity");
            await Dialog.RunAsync(turnContext,ConversationState.CreateProperty<DialogState>(nameof(DialogState)),cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    var welcomeCard = WelcomeCard.CreateCard();
                    var reply=MessageFactory.Attachment(welcomeCard);

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                }
                
            }
        }
    }
}
