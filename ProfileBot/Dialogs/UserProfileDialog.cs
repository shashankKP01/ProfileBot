using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Builder;
using System.Linq;
using System;
using ProfileBot.Cards;

namespace ProfileBot.Bots
{
    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly AzureTableHelper _tableHelper;

        public UserProfileDialog(UserState userState, AzureTableHelper tableHelper)
            : base(nameof(UserProfileDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");
            _tableHelper = tableHelper;

            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                AgeStepAsync,
                CityStepAsync,
                PhoneNoStepAsync,
                EmailStepAsync,
                GenderStepAsync,
                ImageStepAsync,
                DisplayProfileStepAsync,
                ProcessCardResponseAsync,
                SaveProfileAndEndDialogAsync
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new TextPrompt("PhoneNoPrompt", PhoneNumberValidatorAsync));
            AddDialog(new TextPrompt("EmailPrompt", EmailValidatorAsync));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> AgeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["name"] = (string)stepContext.Result;
            return await stepContext.PromptAsync(nameof(NumberPrompt<int>), new PromptOptions { Prompt = MessageFactory.Text("Please enter your age.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> CityStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your city.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PhoneNoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["city"] = (string)stepContext.Result;
            return await stepContext.PromptAsync("PhoneNoPrompt", new PromptOptions { Prompt = MessageFactory.Text("Please enter your phone number.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phone"] = (string)stepContext.Result;
            return await stepContext.PromptAsync("EmailPrompt", new PromptOptions { Prompt = MessageFactory.Text("Please enter your email address.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GenderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["email"] = (string)stepContext.Result;
            var choices = new List<Choice>
            {
                new Choice { Value = "Male" },
                new Choice { Value = "Female" },
                new Choice { Value = "Other" }
            };
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please select your gender."),
                Choices = choices
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ImageStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var genderChoice = (FoundChoice)stepContext.Result;
            stepContext.Values["gender"] = genderChoice.Value;

            var promptOptions = new PromptOptions
            {
                Prompt = MessageFactory.Text("Please upload your profile image.")
            };
            return await stepContext.PromptAsync(nameof(AttachmentPrompt), promptOptions, cancellationToken);
        }

        private async Task<DialogTurnResult> DisplayProfileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);
            userProfile.Name = (string)stepContext.Values["name"];
            userProfile.Age = (int)stepContext.Values["age"];
            userProfile.City = (string)stepContext.Values["city"];
            userProfile.PhoneNumber = (string)stepContext.Values["phone"];
            userProfile.Email = (string)stepContext.Values["email"];
            userProfile.Gender = (string)stepContext.Values["gender"];

            var attachments = stepContext.Context.Activity.Attachments;
            if (attachments != null && attachments.Count > 0)
            {
                var profileImage = attachments[0];
                userProfile.ProfileImage = profileImage.ContentUrl;
            }

            await _userProfileAccessor.SetAsync(stepContext.Context, userProfile, cancellationToken);

            //var attachment = CreateProfileAdaptiveCard(userProfile);
            var attachment= ProfileCards.CreateProfileAdaptiveCard(userProfile);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(attachment), cancellationToken);

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> ProcessCardResponseAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var actionData = stepContext.Context.Activity.Value as dynamic;
            string userResponse = actionData?.Action;

            if (userResponse == "Confirm")
            {
                return await stepContext.NextAsync(cancellationToken: cancellationToken);
            }
            else if (userResponse == "Edit")
            {
                return await EditProfileStepAsync(stepContext, cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EditProfileStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            var editCardAttachment = ProfileCards.CreateEditProfileAdaptiveCard(userProfile);
            await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(editCardAttachment), cancellationToken);

            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> SaveProfileAndEndDialogAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

            var actionData = stepContext.Context.Activity.Value as Newtonsoft.Json.Linq.JObject;

            if (actionData != null)
            {
                // Extract and update profile details from adaptive card data
                userProfile.Name = actionData["Name"]?.ToString() ?? userProfile.Name;
                userProfile.Age = int.TryParse(actionData["Age"]?.ToString(), out var age) ? age : userProfile.Age;
                userProfile.City = actionData["City"]?.ToString() ?? userProfile.City;
                userProfile.PhoneNumber = actionData["Phone"]?.ToString() ?? userProfile.PhoneNumber;
                userProfile.Email = actionData["Email"]?.ToString() ?? userProfile.Email;
                userProfile.Gender = actionData["Gender"]?.ToString() ?? userProfile.Gender;
            }

            try
            {
                // Save profile using AzureTableHelper
                await _tableHelper.SaveUserProfileAsync(userProfile);
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your details have been saved to Azure Table Storage."), cancellationToken);
            }
            catch (Exception ex)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"An error occurred while saving your profile: {ex.Message}"), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(promptContext.Recognized.Succeeded
                                   && promptContext.Recognized.Value >= 18
                                   && promptContext.Recognized.Value <= 100);
        }

        private static Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var phoneNumber = promptContext.Recognized.Value;
            var phonePattern = @"^\d{10}$"; // Regular expression for 10-digit phone numbers
            return Task.FromResult(Regex.IsMatch(phoneNumber, phonePattern));
        }

        private static Task<bool> EmailValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var email = promptContext.Recognized.Value;
            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // Regular expression for basic email validation
            return Task.FromResult(Regex.IsMatch(email, emailPattern));
        }
    }
}