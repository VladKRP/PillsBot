using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Linq;
using System.Threading.Tasks;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using WhatPillsBot.Services;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class PillsByMultipleParametrsDialog:IDialog<object>
    {
        protected UserRequest UserRequest = new UserRequest();

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Ok, we can go another way. What color is  it?");
            context.Wait(ReceivedColors);
        }

        public async Task ReceivedColors(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            UserRequest.PillColors = SearchColorsIdes(message.Text);
            await context.PostAsync("What shape is it?");
            context.Wait(ReceivedShape);
        }

        public async Task ReceivedShape(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            UserRequest.PillShape = SearchShapeId(message.Text);
            await context.PostAsync("Is it has any numbers?");
            context.Wait(ReceivedHasNumbers);
        }

        public async Task ReceivedHasNumbers(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            if (UserAgreement.isUserAgree(message.Text))
            {
                await context.PostAsync("Please enter number that you see");
                context.Wait(ReceivedFirstSideNumber);
            }
            else
            {
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest)).Select(x => x.ToAttachment()).ToList();
                await context.PostAsync(reply);
                context.Wait(ReceivedPillUsageInfo);
            }              
        }

        public async Task ReceivedFirstSideNumber(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            UserRequest.PillFrontSideId = message.Text;
            await context.PostAsync("Now flip pill on another side. Is any number here?");
            context.Wait(ReceivedSecondSideHasNumber);
        }

        public async Task ReceivedSecondSideHasNumber(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            if (UserAgreement.isUserAgree(message.Text))
            {
                await context.PostAsync("Please enter second number");
                context.Wait(ReceivedSecondNumber);
            }
            else
            {
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest)).Select(x => x.ToAttachment()).ToList();
                await context.PostAsync(reply);
                context.Wait(ReceivedPillUsageInfo);
            }
        }

        public async Task ReceivedSecondNumber(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            UserRequest.PillBackSideId = message.Text;
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest)).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(ReceivedPillUsageInfo);
        }

        public async Task ReceivedPillUsageInfo(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            if (message.Text.Contains("getpill:"))
            {
                var pillId = message.Text.Replace("getpill:", "");
                var pill = new PillsChecker().GetPill(pillId);
                reply.Attachments.Add(GenerateHeroCardView.GenerateFullPillCard(pill).ToAttachment());
                await context.PostAsync(reply);
            }
            else
                context.Done<object>(new object());
        }

        private string SearchColorsIdes(string colorsNames)
        {
            var knownColors = VariablesChecker.Colors;
            string result = null;
            var colors = knownColors.Where(x => colorsNames.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value);
            if (colors.Count() > 0)
                result = string.Join(",", colors);
            else
                result = knownColors.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
        }

        private string SearchShapeId(string shapeName)
        {
            var knownShapes = VariablesChecker.Shapes;
            string result = null;
            var shapes = knownShapes.Where(x => shapeName.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(shapes))
                result = shapes;
            else
                result = knownShapes.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
        }

    }
}