using Azure;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProfileBot.Dialogs
{
    public class ChitChatDialog : ComponentDialog
    {
        private readonly QuestionAnsweringClient _questionAnsweringClient;
        private readonly IConfiguration _configuration;
        private readonly string projectName = "UserProfile"; 
        private readonly string deploymentName = "production";

        public ChitChatDialog(IConfiguration configuration)
            : base(nameof(ChitChatDialog))
        {
            _configuration = configuration;
            var languageEndPoint = _configuration["ChitChatSettings:LanguageEndPoint"];
            var languageKey = _configuration["ChitChatSettings:LanguageKey"];
            if (string.IsNullOrEmpty(languageEndPoint) || string.IsNullOrEmpty(languageKey))
            {
                throw new InvalidOperationException("Environment variables for endpoint or key are not set.");
            }

            Uri endpoint = new Uri(languageEndPoint);
            AzureKeyCredential credential = new AzureKeyCredential(languageKey);

            _questionAnsweringClient = new QuestionAnsweringClient(endpoint, credential);

            var waterfallSteps = new WaterfallStep[]
            {
                AskQuestionStepAsync,
                GetQnAResponseStepAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));


            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("What would you like to ask? Type 'Exit' to go back to the menu.") },
                cancellationToken);
        }

        private async Task<DialogTurnResult> GetQnAResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string userQuestion = (string)stepContext.Result;

            
            if (string.Equals(userQuestion, "Exit", StringComparison.OrdinalIgnoreCase))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Exiting Chit Chat. Returning to the main menu."), cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
                
            }
            //var projectName = _configuration["ChitChatSettings:ProjectName"];
            //var deploymentName = _configuration["ChitChatSettings:DeploymentName"];


            
            var project = new QuestionAnsweringProject(projectName, deploymentName);
            var options = new AnswersOptions { ConfidenceThreshold = 0.5 }; 

            Response<AnswersResult> response = await _questionAnsweringClient.GetAnswersAsync(userQuestion, project, options, cancellationToken);

           
            string replyMessage = response.Value.Answers[0].Answer;
            

            // Send the response back to the user
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyMessage), cancellationToken);

            // Re-prompt for another question
            return await stepContext.ReplaceDialogAsync(nameof(ChitChatDialog), null, cancellationToken);
        }
    }
}
