using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using Microsoft.Bot.Builder.Luis;

namespace WhatPillsBot.Dialogs
{
    [LuisModel("d79b7e46-7b62-4a37-b582-c6a5a4a42289", "86cb2f447fc34bed9e2084c761a480d6")]
    [Serializable]
    public class PillsLuisDialog:LuisDialog<object>
    {
        UserMultipleRequest userRequest = new UserMultipleRequest();
        public async Task  StartAsync(IDialogContext context)
        {
            context.Wait(ReceiveStart);
        }

        public async Task ReceiveStart(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            await context.PostAsync("Hey. My meaning is searching pills. Just enter some information about pill, it can be name, shape, color, numbers on pill sides. I try to find pills for you ;)");
            context.Wait(DefineReceivedMessage);
        }


        [LuisIntent("SearchPills")]
        public async Task DefineReceivedMessage(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var messageText = message.Text;
            var colors = VariablesSearcher.SearchColorsIdes(messageText);
            VariablesSearcher.SearchShapeId(messageText);
        }
    }
}