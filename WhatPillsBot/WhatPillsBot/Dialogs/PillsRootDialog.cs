using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Threading.Tasks;
using WhatPillsBot.Extensions;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class PillsRootDialog:IDialog<object>
    {

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedStart);
        }

        public async Task MessageReceivedStart(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {    
            await context.PostAsync("Do you want to find pill?");
            context.Wait(MessageReceivedFindPillAnswer);
        }

        public async Task MessageReceivedFindPillAnswer(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (UserAgreement.isUserAgree(message.Text))
                await context.PostAsync("Did you know the name of the pill?");
            else
                await context.PostAsync("I can't help you with another things, sorry =(");
            context.Wait(MessageReceivedIsPillNameKnownAnswer);
        }

        public async Task MessageReceivedIsPillNameKnownAnswer(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (UserAgreement.isUserAgree(message.Text))
                context.Call<object>(new PillsByNameDialog(), AfterChildDialogDone);
            else
                context.Call<object>(new PillsByMultipleParametrsDialog(), AfterChildDialogDone);
        }

        public async Task MessageFeedback(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (UserAgreement.isUserAgree(message.Text))
                await context.PostAsync("That great!");
            else
                await context.PostAsync("Sorry, I do all I can.");
            context.Wait(MessageReceivedStart);
        }

        private async Task AfterChildDialogDone(IDialogContext context, IAwaitable<object> argument)
        {
            await context.PostAsync("Did you found what you want?");
            context.Wait(MessageFeedback);
        } 

    }
}