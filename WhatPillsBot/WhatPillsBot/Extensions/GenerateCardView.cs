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

        public static HeroCard GeneratePillCard(Pill pill)
        {
            HeroCard card = new HeroCard()
            {
                Images = new List<CardImage>() { new CardImage(pill.ImageUrl) },
                Title = pill.GroupName,
                Subtitle = $"Imprint: {pill.Imprint}\r\n{GenerateTextView.GeneratePillIngredientsString(pill)}",
                Text = $"shape:{pill.Shape}\tcolors: {GenerateTextView.GeneratePillColorsString(pill)}",
                Tap = new CardAction("postBack",  value: "getpill:" + pill.Id)
            };
            return card;
        }

        public static HeroCard GenerateFullPillCard(Pill pill)
        {
            var card = GeneratePillCard(pill);
            card.Text += pill.Usage;
            return card;
        }

        public static IEnumerable<HeroCard> GeneratePillsCard(IEnumerable<Pill> pills)
        {
            return pills.Select(x => GeneratePillCard(x));
        }

        public static HeroCard GeneratePillCardWithoutImage(Pill pill)
        {
            var card = GeneratePillCard(pill);
            card.Images = null;
            return card;
        }

        public static IEnumerable<HeroCard> GeneratePillsCardWithoutImage(IEnumerable<Pill> pills)
        {
            return pills.Select(x => GeneratePillCardWithoutImage(x));
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
                Tap = new CardAction("postBack", value: "getgroup:" + group.Id)
            };
        }

        public static IEnumerable<HeroCard> GeneratePillsResponse(IEnumerable<Pill> pills)
        {
            IEnumerable<HeroCard> cards = new List<HeroCard>();
            if (pills != null && pills.Count() > 0)
            {
                IEnumerable<HeroCard> pillsCard = new List<HeroCard>();
                if (pills.Count() > 15)
                    pillsCard = GeneratePillsCardWithoutImage(pills);
                else
                    pillsCard = GeneratePillsCard(pills);
                cards = new List<HeroCard>() { GenerateMessageCard("Here what we found:") }.Concat(pillsCard)
                    .Concat(new List<HeroCard>() { GenerateMessageCard("To see more info about pill, click on  it.  In another way click on button bellow").AddButton("reset") });
            }
            else
                cards = new List<HeroCard>() { GenerateMessageCard("Something going wrong. We found nothing.") };
            return cards;
        }

        public static IEnumerable<HeroCard> GenerateGroupsResponse(IEnumerable<PillGroup> groups)
        {
            IEnumerable<HeroCard> cards = Enumerable.Empty<HeroCard>();
            if (groups != null && groups.Count() > 0)
            {
                var groupsCard = GenerateGroupsCard(groups);
                cards = new List<HeroCard>() { GenerateMessageCard("Choose the group to get result: ") }.Concat(groupsCard);
            }
            else
                cards = new List<HeroCard>() { GenerateMessageCard("Something going wrong. We found nothing.") };
            return cards;
        }

    }
}