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
                Subtitle = $"Imprint: {pill.Imprint}\n{GenerateTextView.GeneratePillIngredientsString(pill)}",
                Text = $"shape: {pill.Shape}\ncolors: {GenerateTextView.GeneratePillColorsString(pill)}\n",
                Tap = new CardAction("postBack",  value: $"getpill:{pill.Id}")
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

        public static IEnumerable<HeroCard> GeneratePillsResponse(IEnumerable<Pill> pills, int maxNumberOfShownPills)
        {
            IEnumerable<HeroCard> cards = new List<HeroCard>();
            if (pills != null && pills.Count() > 0)
            {
                var result = pills.Count();
                var pillsCard = GeneratePillsCard(pills.Take(maxNumberOfShownPills));
                cards = new List<HeroCard>() { GenerateMessageCard($"The result is {result} pill(s):") }.Concat(pillsCard);
                if (pills.Count() > maxNumberOfShownPills)
                    cards = cards.Concat(new List<HeroCard>() { GenerateMessageCard(" ")
                        .AddButton("show more")});  
            }
            else
                cards = new List<HeroCard>() { GenerateMessageCard("Something going wrong. I found nothing") };
            return cards;
        }

        public static IEnumerable<HeroCard> GenerateShowMoreResponse(IEnumerable<Pill> pills, int numberOfSkipedPills, int numberOfShownPills)
        {
            IEnumerable<HeroCard> cards = Enumerable.Empty<HeroCard>();
            var countOfRemainedPills = pills.Count() - numberOfSkipedPills;
            if (countOfRemainedPills <= 0)
                cards = new List<HeroCard>() { GenerateMessageCard("All found pills was shown.") };
            else if (countOfRemainedPills <= numberOfShownPills)
                cards = GeneratePillsCard(pills.Skip(numberOfSkipedPills).Take(countOfRemainedPills));
            else
            {
                cards = GeneratePillsCard(pills.Skip(numberOfSkipedPills).Take(numberOfShownPills)).Concat(new List<HeroCard>() { GenerateMessageCard(" ")
                .AddButton($"show more")});
            }
            return cards;
        }

        public static  HeroCard GenerateGroupCard(PillGroup group)
        {
            return new HeroCard()
            {
                Title = group.Name,
                Tap = new CardAction("postBack", value: $"getgroup:({group.Id})")
            };
        }

        public static IEnumerable<HeroCard> GenerateGroupsCard(IEnumerable<PillGroup> groups)
        {
            return groups.Select(x => GenerateGroupCard(x));
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

        public static HeroCard AddButtons(this HeroCard card, Dictionary<string,string> buttonParams)
        {
            var buttons = buttonParams.Select(x => new CardAction("postBack", title: x.Key, value: x.Value));
            if (card.Buttons == null)
                card.Buttons = buttons.ToList();
            else
                card.Buttons = card.Buttons.Concat(buttons).ToList();
            return card;
        }

        public static HeroCard AddButton(this HeroCard card, string value, string title = null)
        {
            var buttons = new List<CardAction>() { new CardAction("postBack", value: value, title: title == null ? value : title) };
            if (card.Buttons == null)
                card.Buttons = buttons;
            else
                card.Buttons = card.Buttons.Concat(buttons).ToList();
            return card;
        }

    }
}