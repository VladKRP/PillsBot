using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatPillsBot.Model;
using WhatPillsBot.Services;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class WhatPillsDialog:IDialog<object>
    {

        protected IEnumerable<Shape> Shapes = VariablesChecker.Shapes;
        protected IEnumerable<Color> Colors = VariablesChecker.Colors;

        protected UserRequest UserRequest = new UserRequest();
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
                case 3:await ReceiveName(context, messageText); break;
                case 4:await ReceiveColorAnswer(context, messageText); break;
                case 5:await ReceiveShapeAnswer(context, messageText);break;
                case 6:await ReceiveHasNumberAnswer(context, messageText);break;
                case 7:await ReceiveFirstSideNumberAnswer(context, messageText);break;
                case 8:await ReceiveSecondSideHasNumberAnswer(context, messageText);break;
                case 9: await ReceiveSecondSideNumberAnswer(context, messageText); break;
                    //case 9:await ReceiveIsPillFoundedAnswer(context, messageText);break;
                    //case 10:await ReceiveCorrectRequestAnswer(context, messageText);break;    
            }
        }

        public async Task ReceiveInitialQuestionAnswer(IDialogContext context)
        {
            await context.PostAsync("Do you want to find pill?");
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

       public async Task ReceiveName(IDialogContext context, string messageText)
       {
            UserRequest.Name = messageText;
            await context.PostAsync(ReceivePills(context));
        }

        public string ReceivePills(IDialogContext context)
        {          
            string searchResult = null;
            var pills = PillsChecker.GetPills(UserRequest);
            if (pills.Count() > 0)
                searchResult = "Here what we found";
                //function to generate result string
            else
                searchResult = "Something going wrong. We found nothing.";
            Stage = 0;
            UserRequest = new UserRequest();
            return searchResult;
        }

        public async Task ReceiveColorAnswer(IDialogContext context, string messageText)
        {
            var colors = Colors.Where(x => messageText.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value);
            if (colors.Count() > 0)
                UserRequest.Colors = string.Join(",", colors);
            else
                UserRequest.Colors = Colors.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            await context.PostAsync("What shape is it?");
            Stage = 5;
            context.Wait(MessageHandle);
        }

        public async Task ReceiveShapeAnswer(IDialogContext context, string messageText) {

            var shapes = Shapes.Where(x => messageText.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
            if (shapes != null)
                UserRequest.Shape = shapes;
            else
                UserRequest.Shape = Shapes.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
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
                Stage = 0;
                await context.PostAsync(ReceivePills(context));
            }           
        }

        public async Task ReceiveFirstSideNumberAnswer(IDialogContext context, string messageText) {
            UserRequest.FrontSideId = messageText;
            await context.PostAsync("Now flip pill on another side. Is it has number?");
            Stage = 8;
            context.Wait(MessageHandle);
        }

        public async Task ReceiveSecondSideHasNumberAnswer(IDialogContext context, string messageText)
        {
            if (isUserAgree(messageText))
            {
                await context.PostAsync("Please enter it");
                Stage = 9;
                context.Wait(MessageHandle);
            }
            else
            {
                Stage = 0;
                await context.PostAsync(ReceivePills(context));
            }
        }

        public async Task ReceiveSecondSideNumberAnswer(IDialogContext context, string messageText)
        {
            UserRequest.BackSideId = messageText;
            Stage = 0;
            await context.PostAsync(ReceivePills(context));
        }

        //public async Task ReceiveIsPillFoundedAnswer(IDialogContext context, string messageText) { }

        //public async Task ReceiveCorrectRequestAnswer(IDialogContext context, string messageText) { }


        private bool isUserAgree(string message)
        {
            var agreeKeyWords = new List<string>() { "Yes", "Yep", "Ye", "y" };
            var isAgree = agreeKeyWords.Where(x => message.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) >= 0).Count() > 0;
            return isAgree;
        }

    }
}