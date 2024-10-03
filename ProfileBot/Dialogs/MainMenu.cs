using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using ProfileBot.Dialogs;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Extensions.Configuration;




namespace ProfileBot.Bots
{
    public class MainMenu : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        private readonly IConfiguration _configuration;
        private readonly QuestionAnsweringClient _questionAnsweringClient;

        private readonly AzureTableHelper _tableHelper;

        public MainMenu(UserState userState, AzureTableHelper tableHelper)
    : base(nameof(MainMenu))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _tableHelper = tableHelper;

            var waterfallSteps = new WaterfallStep[]
            {
                ShowOptionsStepAsync,
                HandleOptionSelectionStepAsync,
                RestartMainMenuStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new UserProfileDialog(userState, tableHelper)); // Pass userState and tableHelper
            AddDialog(new ChitChatDialog());
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowOptionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var options = new List<Choice>
            {
                new Choice{Value="Create Profile"},
                new Choice{Value="Chit Chat"}
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What would you like to do?"),
                Choices = options
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> HandleOptionSelectionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selectedOption = ((FoundChoice)stepContext.Result).Value;

            if (selectedOption == "Create Profile")
            {
                // Begin the UserProfileDialog and wait for it to complete
                return await stepContext.BeginDialogAsync(nameof(UserProfileDialog), null, cancellationToken);
            }
            else if (selectedOption == "Chit Chat")
            {
                // Begin the ChitChatDialog and wait for it to complete
                return await stepContext.BeginDialogAsync(nameof(ChitChatDialog), null, cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
        private async Task<DialogTurnResult> RestartMainMenuStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.ReplaceDialogAsync(InitialDialogId,null,cancellationToken);
        }
    }
}
