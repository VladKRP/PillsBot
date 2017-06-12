using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.Linq;
using WhatPillsBot.Model;

namespace WhatPillsBot.Extensions
{
    public static class GenerateCardView
    {
        public static IEnumerable<HeroCard> GeneratePillsCard(IEnumerable<Pill> pills)
        {
            IEnumerable<HeroCard> cards = Enumerable.Empty<HeroCard>();
            cards = pills.Select(x => GeneratePillCard(x));
            return cards;
        }

        public static HeroCard GeneratePillCard(Pill pill)
        {
            HeroCard card = new HeroCard()
            {
                Images = new List<CardImage>() { new CardImage(pill.ImageUrl) },
                Title = pill.GroupName,
                Subtitle = $"Imprint: {pill.Imprint}\r\n{GenerateTextView.GeneratePillIngredientsString(pill)}",
                Text = $"shape:{pill.Shape}\tcolors: {GenerateTextView.GeneratePillColorsString(pill)} {Services.PillsChecker.GetPillUsage(pill.Id)}",
            };
            return card;
        }

        public static HeroCard GenerateFullPillCard(Pill pill)
        {
            var card = GeneratePillCard(pill);
            card.Text += pill.Usage;
            return card;
        }

        public static  HeroCard GenerateGroupCard(PillGroup group)
        {
            return new HeroCard()
            {
                Title = group.Name
            };
        }

    }
}