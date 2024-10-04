
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Azure;
using Azure.AI.Language.Conversations;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using System.IO;

namespace ProfileBot.Dialogs
{
    public class FaqDialog : ComponentDialog
    {
        private readonly ConversationAnalysisClient _cluClient;
        private readonly string _projectName;
        private readonly string _deploymentName;

        public FaqDialog(IConfiguration configuration)
            : base(nameof(FaqDialog))
        {
            // Initialize CLU settings from configuration
            var cluEndpoint = configuration["CLUSettings:Endpoint"];
            var cluKey = configuration["CLUSettings:Key"];
            _projectName = configuration["CLUSettings:ProjectName"];
            _deploymentName = configuration["CLUSettings:DeploymentName"];

            _cluClient = new ConversationAnalysisClient(new Uri(cluEndpoint), new AzureKeyCredential(cluKey));

            var waterfallSteps = new WaterfallStep[]
            {
                AskQuestionStepAsync,
                GetCLUResponseStepAsync
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
        }

        private async Task<DialogTurnResult> AskQuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt),
                new PromptOptions { Prompt = MessageFactory.Text("Please ask your FAQ.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetCLUResponseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userQuestion = (string)stepContext.Result;

            // Create the request object for the CLU service
            var request = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = userQuestion,
                        id = "1",
                        participantId = "1"
                    }
                },
                parameters = new
                {
                    projectName = _projectName,
                    deploymentName = _deploymentName,
                    stringIndexType = "Utf16CodeUnit"
                },
                kind = "Conversation"
            };

            // Call the AnalyzeConversationAsync method
            Response response = await _cluClient.AnalyzeConversationAsync(RequestContent.Create(request));

            // Extract the top intent from the response
            string topIntent = "None";

            if (response.ContentStream != null)
            {
                using (StreamReader reader = new StreamReader(response.ContentStream))
                {
                    string jsonResponse = await reader.ReadToEndAsync();
                    var jsonDocument = System.Text.Json.JsonDocument.Parse(jsonResponse);
                    var result = jsonDocument.RootElement.GetProperty("result");
                    var prediction = result.GetProperty("prediction");
                    topIntent = prediction.GetProperty("topIntent").GetString();
                }
            }

            // Send back the detected intent
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Detected intent: {topIntent}"), cancellationToken);

            // Handle different intents
            switch (topIntent)
            {
                case "Get Company Overview":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Our company is a leading software development firm specializing in AI and machine learning solutions. Founded in 2010, we have grown to over 500 employees worldwide."), cancellationToken);
                    break;
                case "Leave Policy":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Our leave policy includes 20 days of paid time off per year, 10 sick days, and additional leave for special circumstances. We also offer parental leave and sabbaticals for long-term employees."), cancellationToken);
                    break;
                case "Work From Home Policy":
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("We offer flexible work-from-home options. Employees can work remotely up to 3 days a week, with the option for full-time remote work for certain roles. We provide equipment and support for comfortable home office setups."), cancellationToken);
                    break;
                case "None":
                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I'm sorry, I couldn't find a specific answer for that question. Is there something else I can help you with regarding our company policies or information?"), cancellationToken);
                    break;
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
