using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using WhatPillsBot.Services;
using WhatPillsBot.Model.JSONDeserialization;

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
                case 0:ReceiveInitialQuestionAnswer(reply); break;      
                case 1:ReceiveFindPillAnswer(reply, messageText); break;
                case 2:ReceiveIsKnowPillAnswer(reply, messageText); break;
                case 3:ReceiveName(reply, messageText);break;
                case 4:ReceiveColorAnswer(reply, messageText); break;
                case 5:ReceiveShapeAnswer(reply, messageText);break;
                case 6:ReceiveHasNumberAnswer(reply, messageText);break;
                case 7:ReceiveFirstSideNumberAnswer(reply, messageText);break;
                case 8:ReceiveSecondSideHasNumberAnswer(reply, messageText);break;
                case 9:ReceiveSecondSideNumberAnswer(reply, messageText); break;
                case 10:ReceiveGroupId(reply, messageText);break;    
            }
            await context.PostAsync(reply);
            context.Wait(MessageHandle);
            LastUsing = DateTime.Now;
        }

        public void ReceiveInitialQuestionAnswer(Activity activity)
        {
            SetActivityTextAndStage(activity, "Do you want to find pill?", 1);       
        }

        public void ReceiveFindPillAnswer(Activity activity, string messageText)
        {
            if (isUserAgree(messageText))
                SetActivityTextAndStage(activity, "Did you know the name of the pill?", 2);             
            else
            {
                activity.Text = "I can't help you with another thing, sorry =(";
                ResetUserData();
            }
        }

        public void ReceiveIsKnowPillAnswer(Activity activity, string messageText)
        {
            if (isUserAgree(messageText))
                SetActivityTextAndStage(activity, "It simplify our search. What is it name?", 3);                
            else
                SetActivityTextAndStage(activity, "Ok, I think we can go another way. What color is  it?", 4);
        }

        public void ReceiveName(Activity activity, string messageText)
        {
            UserRequest.PillName = messageText;
            var products = PillsChecker.GetPillProducts(UserRequest.PillName);
            var groups = PillsChecker.GetPillGroups(UserRequest.PillName);
            if (products.Count() > 0)
                ReceivePills(activity, products);
            else if(groups.Count() > 0)
            {
                ReceiveGroups(activity, PillsChecker.GetPillGroups(UserRequest.PillName));
                Stage = 10;
            }
            else
            {
                activity.Text = "We found nothing.";
                ResetUserData();
            }            
        }

        public void ReceiveColorAnswer(Activity activity, string message)
        {
            UserRequest.PillColors = SearchColorsIdes(message);
            SetActivityTextAndStage(activity, "What shape is it?", 5);
        }

        public void ReceiveShapeAnswer(Activity activity, string message) {
            UserRequest.PillShape = SearchShapeId(message);
            SetActivityTextAndStage(activity, "Is is has any numbers?", 6);       
        }

        public void ReceiveHasNumberAnswer(Activity activity, string message) {
            if (isUserAgree(message))
                SetActivityTextAndStage(activity, "Please write the number that you see.", 7);
            else
                ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceiveFirstSideNumberAnswer(Activity activity, string message) {
            UserRequest.PillFrontSideId = message;
            SetActivityTextAndStage(activity, "Now flip pill on another side. Is it has number?", 8);
        }

        public void ReceiveSecondSideHasNumberAnswer(Activity activity, string message)
        {
            if (isUserAgree(message))
                SetActivityTextAndStage(activity, "Please enter it", 9);
            else
                ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceiveSecondSideNumberAnswer(Activity activity, string message)
        {
            UserRequest.PillBackSideId = message;
            ReceivePills(activity, PillsChecker.GetPillsByMultipleParametres(UserRequest));
        }

        public void ReceiveGroupId(Activity activity, string message)
        {
                UserRequest.PillGroup = message;
                ReceivePills(activity, PillsChecker.GetPillByGroupIdAndName(UserRequest.PillGroup, UserRequest.PillName));
        }

        public void ReceivePills(Activity activity, IEnumerable<Pill> pills)
        {
            string replyPhrase = null;
            if (pills != null && pills.Count() > 0)
            {
                var pillsCard = GenerateHeroCardView.GeneratePillsCard(pills);
                replyPhrase = "Here what we found:";
                activity.Attachments = pillsCard.Select(x => x.ToAttachment()).ToList();
            }
            else
                replyPhrase = "Something going wrong. We found nothing.";
            activity.Text = replyPhrase;
            //??
            ResetUserData();
        }

        public void ReceiveGroups(Activity activity, IEnumerable<PillGroup> groups)
        {
            string replyPhrase = null;
            if (groups != null && groups.Count() > 0)
            {
                var pillsCard = GenerateHeroCardView.GenerateGroupsCard(groups);
                replyPhrase = "Choose the group to get result:";
                activity.Attachments = pillsCard.Select(x => x.ToAttachment()).ToList();
            }
            else
                replyPhrase = "Something going wrong. We found nothing.";
            activity.Text = replyPhrase;
        }

        private void SetActivityTextAndStage(Activity activity, string text, int stage )
        {
            activity.Text = text;
            Stage = stage;
        }

        private string SearchColorsIdes(string colorsNames)
        {
            string result = null;
            var colors = Colors.Where(x => colorsNames.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value);
            if (colors.Count() > 0)
                result = string.Join(",", colors);
            else
                result = Colors.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
        }

        private string SearchShapeId(string shapeName)
        {
            string result = null;
            var shapes = Shapes.Where(x => shapeName.IndexOf(x.Name, StringComparison.CurrentCultureIgnoreCase) >= 0).Select(x => x.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(shapes))
                result = shapes;
            else
                result = Shapes.Where(x => x.Name.Equals("other")).Select(x => x.Value).FirstOrDefault();
            return result;
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