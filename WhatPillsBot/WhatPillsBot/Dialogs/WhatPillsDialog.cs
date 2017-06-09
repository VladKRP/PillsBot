using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WhatPillsBot.Model;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class WhatPillsDialog:IDialog<object>
    {
        protected Pill Pill = new Pill();
        protected int Stage { get; set; } = 0;

        public  Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageHandle);
            return Task.CompletedTask;
        }

        public async Task MessageHandle(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            var messageText = message.Text;
            switch (Stage)
            {
                case 0:await ReceiveInitialQuestionAnswer(context); break;      
                case 1:await ReceiveFindPillAnswer(context, messageText); break;
                case 2:await ReceiveIsKnowPillAnswer(context, messageText); break;
                case 3:await ReceivePills(context, messageText); break;
                case 4:await ReceiveColorAnswer(context, messageText); break;
                case 5:await ReceiveShapeAnswer(context, messageText);break;
                case 6:await ReceiveHasNumberAnswer(context, messageText);break;
                case 7:await ReceiveFirstSideNumberAnswer(context, messageText);break;
                case 8:await ReceiveSecondSideNumberAnswer(context, messageText);break;
                case 9:await ReceiveIsPillFoundedAnswer(context, messageText);break;
                case 10:await ReceiveCorrectRequestAnswer(context, messageText);break;    
            }
        }

        public async Task ReceiveInitialQuestionAnswer(IDialogContext context)
        {
            await context.PostAsync("Do you want to find pill?");
            var pills = Services.PillsChecker.GetPills();
            Stage = 1;
            context.Wait(MessageHandle);           
        }

        public async Task ReceiveFindPillAnswer(IDialogContext context, string messageText)
        {
            if (isUserAgree(messageText))
            {
                await context.PostAsync("Did you know the name of the pill?");
                Stage = 2;
                context.Wait(MessageHandle);                
            }              
            else
            {
                await context.PostAsync("I can't help you with another thing, sorry =(");
                Stage = 0;
            }
        }

        public async Task ReceiveIsKnowPillAnswer(IDialogContext context, string messageText)
        {
            if (isUserAgree(messageText))
            {
                await context.PostAsync("It simplify our search. What is it name?");
                Stage = 3;
                context.Wait(MessageHandle);              
            }
            else
            {
                await context.PostAsync("Ok.I think we can go another way.\n What color is  it?");
                Stage = 4;
                context.Wait(MessageHandle);              
            }
        }

        public async Task ReceivePills(IDialogContext context, string messageText)
        {
            Pill.Name = messageText;
            await context.PostAsync("Here what we found. PILLSSSSSSSS");
            //returning Data
            Stage = 0;
        }

        public async Task ReceiveColorAnswer(IDialogContext context, string messageText)
        {
            //find color in db
            //if found save to the pill 
            Pill.Colors = new List<string>() { messageText };
            await context.PostAsync("What shape is it?");
            Stage = 5;
            context.Wait(MessageHandle);
        }

        public async Task ReceiveShapeAnswer(IDialogContext context, string messageText) {

            //find shape in db
            //if found save to the pill
            Pill.Shape = messageText;
            await context.PostAsync("Is is has any numbers?");
            Stage = 6;
            context.Wait(MessageHandle);
        }

        public async Task ReceiveHasNumberAnswer(IDialogContext context, string messageText) {
            if (isUserAgree(messageText))
            {
                await context.PostAsync("Please write the number that you see.");
                Stage = 7;
                context.Wait(MessageHandle);
            }
            else
            {
                Stage = 3;
                context.Wait(MessageHandle);
            }           
        }

        public async Task ReceiveFirstSideNumberAnswer(IDialogContext context, string messageText) {
            //sideA value
            Pill.FrontSideId = messageText;
            await context.PostAsync("Now flip pill on another side. If any number here please enter it.");
            Stage = 8;
            context.Wait(MessageHandle);
        }

        public async Task ReceiveSecondSideNumberAnswer(IDialogContext context, string messageText) {
            //sideB number?
            Pill.BackSideId = messageText;
            Stage = 3;
        }

        public async Task ReceiveIsPillFoundedAnswer(IDialogContext context, string messageText) { }

        public async Task ReceiveCorrectRequestAnswer(IDialogContext context, string messageText) { }


        private bool isUserAgree(string message)
        {
            var agreeKeyWords = new List<string>() { "Yes", "Yep", "Ye", "y" };
            var isAgree = agreeKeyWords.Where(x => message.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) >= 0).Count() > 0;
            return isAgree;
        }

    }
}