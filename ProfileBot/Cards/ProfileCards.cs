using AdaptiveCards;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;

namespace ProfileBot.Cards
{
    public static class ProfileCards
    {
        public static Attachment CreateProfileAdaptiveCard(UserProfile userProfile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "User Profile",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = "auto",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Url = new Uri(userProfile.ProfileImage),
                                        Size = AdaptiveImageSize.Medium,
                                        Style = AdaptiveImageStyle.Person,
                                        PixelWidth = 150,
                                        PixelHeight = 150
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = "stretch",
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveFactSet
                                    {
                                        Facts = new List<AdaptiveFact>
                                        {
                                            new AdaptiveFact("Name:", userProfile.Name),
                                            new AdaptiveFact("Age:", userProfile.Age.ToString()),
                                            new AdaptiveFact("City:", userProfile.City),
                                            new AdaptiveFact("Phone Number:", userProfile.PhoneNumber),
                                            new AdaptiveFact("Email:", userProfile.Email),
                                            new AdaptiveFact("Gender:", userProfile.Gender)
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm",
                        Data = new { Action = "Confirm" }
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "Edit",
                        Data = new { Action = "Edit" }
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }

        public static Attachment CreateEditProfileAdaptiveCard(UserProfile userProfile)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Edit Profile",
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium
                    },
                    new AdaptiveTextInput
                    {
                        Id = "Name",
                        Value = userProfile.Name,
                        Placeholder = "Enter your name"
                    },
                    new AdaptiveNumberInput
                    {
                        Id = "Age",
                        Value = userProfile.Age,
                        Placeholder = "Enter your age"
                    },
                    new AdaptiveTextInput
                    {
                        Id = "City",
                        Value = userProfile.City,
                        Placeholder = "Enter your city"
                    },
                    new AdaptiveTextInput
                    {
                        Id = "Phone",
                        Value = userProfile.PhoneNumber,
                        Placeholder = "Enter your phone number"
                    },
                    new AdaptiveTextInput
                    {
                        Id = "Email",
                        Value = userProfile.Email,
                        Placeholder = "Enter your email address"
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = "Gender",
                        Value = userProfile.Gender,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice { Title = "Male", Value = "Male" },
                            new AdaptiveChoice { Title = "Female", Value = "Female" },
                            new AdaptiveChoice { Title = "Other", Value = "Other" }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Confirm",
                        Data = new { Action = "Confirm" }
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}
