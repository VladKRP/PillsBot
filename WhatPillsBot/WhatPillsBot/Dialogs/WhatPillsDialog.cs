using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using WhatPillsBot.Services;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class WhatPillsDialog:IDialog<object>
    {

        protected IEnumerable<Shape> Shapes = VariablesChecker.Shapes;
        protected IEnumerable<Color> Colors = VariablesChecker.Colors;
        protected DateTime LastUsing { get; set; }

        protected UserRequest UserRequest = new UserRequest();
        protected int Stage { get; set; } = 0;

        public  Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageHandle);
            return Task.CompletedTask;
        }

        public async Task MessageHandle(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            ResetUserDataAfterADay();
            var message = await argument as Activity;
            var messageText = message.Text;
            var reply = message.CreateReply();       
            switch (Stage)
            {
                case 0:ReceiveInitialQuestionAnswer(context,reply); break;      
                case 1:ReceiveFindPillAnswer(context, reply, messageText); break;
                case 2:ReceiveIsKnowPillAnswer(context,reply, messageText); break;
                case 3:ReceiveName(context, reply, messageText); break;
                case 4:ReceiveColorAnswer(context, reply, messageText); break;
                case 5:ReceiveShapeAnswer(context, reply, messageText);break;
                case 6:ReceiveHasNumberAnswer(context, reply, messageText);break;
                case 7:ReceiveFirstSideNumberAnswer(context, reply, messageText);break;
                case 8:ReceiveSecondSideHasNumberAnswer(context, reply, messageText);break;
                case 9:ReceiveSecondSideNumberAnswer(context, reply, messageText); break; 
            }
            await context.PostAsync(reply);
            context.Wait(MessageHandle);
            LastUsing = DateTime.Now;
        }

        public void ReceiveInitialQuestionAnswer(IDialogContext context, Activity activity)
        {
            activity.Text = "Do you want to find pill?";
            Stage = 1;                   
        }

        public void ReceiveFindPillAnswer(IDialogContext context, Activity activity, string messageText)
        {
            if (isUserAgree(messageText))
            {
                activity.Text = "Did you know the name of the pill?";
                Stage = 2;            
            }              
            else
            {
                activity.Text = "I can't help you with another thing, sorry =(";
                ResetUserData();
            }
        }

        public void ReceiveIsKnowPillAnswer(IDialogContext context,Activity activity, string messageText)
        {
            if (isUserAgree(messageText))
            {
                activity.Text = "It simplify our search.\n What is it name?";
                Stage = 3;           
            }
            else
            {
                activity.Text = "Ok.I think we can go another way.\n What color is  it?";
                Stage = 4;           
            }
        }

        public void ReceiveName(IDialogContext context, Activity activity, string messageText)
       {
            UserRequest.Name = messageText;
            ReceivePills(activity, PillsChecker.GetPillsByName(UserRequest.Name));  
        }

        public void ReceiveColorAnswer(IDialogContext context, Activity activity, string messageText)
        {
            if (!string.IsNullOrEmpty(messageText))
            {
                var colors = Colors.Where(x => messageText.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value);
                if (colors.Count() > 0)
                    UserRequest.Colors = string.Join(",", colors);
                else
                    UserRequest.Colors = Colors.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            }  
            activity.Text = "What shape is it?";
            Stage = 5;
        }

        public void ReceiveShapeAnswer(IDialogContext context, Activity activity, string messageText) {
            if (!string.IsNullOrEmpty(messageText))
            {
                var shapes = Shapes.Where(x => messageText.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
                if (!string.IsNullOrEmpty(shapes))
                    UserRequest.Shape = shapes;
                else
                    UserRequest.Shape = Shapes.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            }           
            activity.Text = "Is is has any numbers?";
            Stage = 6;
        }

        public void ReceiveHasNumberAnswer(IDialogContext context,Activity activity, string messageText) {
            if (isUserAgree(messageText))
            {
                activity.Text = "Please write the number that you see.";
                Stage = 7;
            }
            else
                ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceiveFirstSideNumberAnswer(IDialogContext context, Activity activity, string messageText) {
            UserRequest.FrontSideId = messageText;
            activity.Text = "Now flip pill on another side. Is it has number?";
            Stage = 8;
        }

        public void ReceiveSecondSideHasNumberAnswer(IDialogContext context, Activity activity, string messageText)
        {
            if (isUserAgree(messageText))
            {
                activity.Text = "Please enter it";
                Stage = 9;
            }
            else
                ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceiveSecondSideNumberAnswer(IDialogContext context, Activity activity, string messageText)
        {
            UserRequest.BackSideId = messageText;
            Stage = 0;
            ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceivePills(Activity activity, IEnumerable<Pill> pills)
        {
            string replyPhrase = null;
            if (pills != null && pills.Count() > 0)
            {
                var pillsCard = GenerateCardView.GeneratePillsCard(pills);
                replyPhrase = "Here what we found:";
                activity.Attachments = pillsCard.Select(x => x.ToAttachment()).ToList();
            }
            else
                replyPhrase = "Something going wrong. We found nothing.";
            activity.Text = replyPhrase;
            ResetUserData();
        }

        private bool isUserAgree(string message)
        {
            var agreeKeyWords = new List<string>() { "Yes", "Yep", "Ye", "y" };
            var isAgree = agreeKeyWords.Where(x => message.IndexOf(x, StringComparison.CurrentCultureIgnoreCase) >= 0).Count() > 0;
            return isAgree;
        }

        private void ResetUserData()
        {    
            UserRequest = new UserRequest();
            Stage = 0;
        }

        private void ResetUserDataAfterADay()
        {
            var currentDate = DateTime.Now;
            if (!currentDate.Date.Equals(LastUsing.Date))
                ResetUserData();
        }
    }
}