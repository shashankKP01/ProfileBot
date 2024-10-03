using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace ProfileBot.Cards
{
    public static class WelcomeCard
    {
        public static Attachment CreateCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Welcome",
                Text = "Hello and Welcome to our bot! How can I assist you today?",
                Images = new List<CardImage>
                {
                    new CardImage("https://t3.ftcdn.net/jpg/02/41/32/08/360_F_241320849_OdSd5J47uglxJjdqBkZMMQm0sE6VXB1m.jpg")
                },
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.ImBack, "Get Started", value: "Get Started")
                }
            };

            return heroCard.ToAttachment();
        }
    }
}
