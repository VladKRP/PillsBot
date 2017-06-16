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
            await context.PostAsync("Hey. My meaning is searching pills.\n\nJust enter some information about pill.\n\nIt can be name or shape, colors, numbers on pill sides.\n\nI hope I will be helpfull for you ;)");
            context.Call(new PillsLuisDialog(), ReturnToStartDialog);
        }

        public async Task ReturnToStartDialog(IDialogContext context, IAwaitable<object> argument)
        {
            await StartAsync(context);
        }
    }
}