using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WhatPillsBot.Dialogs.Luis
{
    [Serializable]
    public class RootDialog:IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(ReceiveStart);
        }

        public async Task ReceiveStart(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            await context.PostAsync("Hey. My meaning in searching pills.\nJust enter some information about pill, it can be name or shape, color, numbers on pill sides. I try to find pills for you ;)");
            context.Call(new PillsLuisDialog(), ReturnToStartDialog);
        }

        public async Task ReturnToStartDialog(IDialogContext context, IAwaitable<object> argument)
        {
            await StartAsync(context);
        }
    }
}