using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using WhatPillsBot.Model.JSONDeserialization;

namespace WhatPillsBot.Extensions
{
    public static class GenerateHeroCardView
    {
        public static HeroCard GenerateMessageCard(string message)
        {
            return new HeroCard(text: message);
        }

        public static IEnumerable<HeroCard> GeneratePillsCard(IEnumerable<Pill> pills)
        {
            return pills.Select(x => GeneratePillCard(x));
        }

        public static HeroCard GeneratePillCard(Pill pill)
        {
            HeroCard card = new HeroCard()
            {
                Images = new List<CardImage>() { new CardImage(pill.ImageUrl) },
                Title = pill.GroupName,
                Subtitle = $"Imprint: {pill.Imprint}\r\n{GenerateTextView.GeneratePillIngredientsString(pill)}",
                Text = $"shape:{pill.Shape}\tcolors: {GenerateTextView.GeneratePillColorsString(pill)}",
                Tap = new CardAction("postBack",value:pill.Id)        
            };
            return card;
        }

        public static HeroCard GenerateFullPillCard(Pill pill)
        {
            var card = GeneratePillCard(pill);
            card.Text += pill.Usage;
            return card;
        }

        public static HeroCard AddButton(this HeroCard card, string value)
        {
            card.Buttons = new List<CardAction>() { new CardAction("postBack", value: value, title: value) };
            return card;
        }

        public static IEnumerable<HeroCard> GenerateGroupsCard(IEnumerable<PillGroup> groups)
        {
            return groups.Select(x => GenerateGroupCard(x));               
        }

        public static  HeroCard GenerateGroupCard(PillGroup group)
        {
            return new HeroCard()
            {
                Title = group.Name,
                Tap = new CardAction("postBack", value:group.Id)
            };
        }

    }
}