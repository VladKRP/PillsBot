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
        protected UserMultipleRequest UserRequest = new UserMultipleRequest();

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Ok, we can go another way. What color is  it?");
            context.Wait(ReceivedColors);
        }

        public async Task ReceivedColors(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            UserRequest.PillColors = VariablesSearcher.SearchColorsIdes(message.Text);
            await context.PostAsync("What shape is it?");
            context.Wait(ReceivedShape);
        }

        public async Task ReceivedShape(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            UserRequest.PillShape = VariablesSearcher.SearchShapeId(message.Text);
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
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest), 3).Select(x => x.ToAttachment()).ToList();
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
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest), 3).Select(x => x.ToAttachment()).ToList();
                await context.PostAsync(reply);
                context.Wait(ReceivedPillUsageInfo);
            }
        }

        public async Task ReceivedSecondNumber(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            UserRequest.PillBackSideId = message.Text;
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(UserRequest), 3).Select(x => x.ToAttachment()).ToList();
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
                var pillUsage = new PillsChecker().GetPillUsage(pillId);
                reply.Text = pillUsage;
                await context.PostAsync(reply);
            }
            else
                context.Done<object>(new object());
        }

    }
}