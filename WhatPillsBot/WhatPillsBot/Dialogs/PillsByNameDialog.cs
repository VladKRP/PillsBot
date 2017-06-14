using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WhatPillsBot.Services;
using WhatPillsBot.Model.JSONDeserialization;
using WhatPillsBot.Extensions;

namespace WhatPillsBot.Dialogs
{
    [Serializable]
    public class PillsByNameDialog:IDialog<object>
    {
        protected string PillName { get; set; }
        protected string PillGroup { get; set; }

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("It simplify our search. What is it name? ");
            context.Wait(PillNameMessageReceived);
        }

        public async Task PillNameMessageReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var messageText = message.Text;
            var reply = message.CreateReply();
            var pillChecker = new PillsChecker();
            PillName = messageText;
            var products = pillChecker.GetPillProducts(PillName);
            if (products.Count() > 0)
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(products).Select(x => x.ToAttachment()).ToList(); 
            else
            {
                var groups = pillChecker.GetPillGroups(PillName);
                if (groups.Count() > 0)
                    reply.Attachments = GenerateHeroCardView.GenerateGroupsResponse(groups).Select(x => x.ToAttachment()).ToList();
                else
                {
                    reply.Attachments.Add(GenerateHeroCardView.GenerateMessageCard("We found nothing.").ToAttachment());
                    context.Done<object>(new object());
                }        
            }
            await context.PostAsync(reply);
            context.Wait(ReceiveMultipleInfo);
        }


        public async Task ReceiveMultipleInfo(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            if (message.Text.Contains("getpill:"))
                await ReceivedPillUsageInfo(context, argument);
            else if (message.Text.Contains("getgroup:"))
                await ReceivedGroupId(context, argument);
            else
                context.Done<object>(new object());
        }

        public async Task ReceivedPillUsageInfo(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            var pillid = message.Text.Replace("getpill:","");
            if (pillid.Count() > 0)
            {
                var pillUsage = new PillsChecker().GetPillUsage(pillid);
                reply.Text = pillUsage;
                await context.PostAsync(reply);
            }         
        }

        public async Task ReceivedGroupId(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            var groupId = message.Text.Replace("getgroup:", "");
            if(groupId.Count() > 0)
            {
                PillGroup = groupId;
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillByGroupIdAndName(PillGroup, PillName)).Select(x => x.ToAttachment()).ToList();
                await context.PostAsync(reply);
            }    
        }
    }
}