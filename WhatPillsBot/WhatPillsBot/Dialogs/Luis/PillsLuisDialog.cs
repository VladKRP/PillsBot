using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using WhatPillsBot.Model.JSONDeserialization;
using WhatPillsBot.Services;

namespace WhatPillsBot.Dialogs.Luis
{
    [LuisModel("d79b7e46-7b62-4a37-b582-c6a5a4a42289", "86cb2f447fc34bed9e2084c761a480d6")]
    [Serializable]
    public class PillsLuisDialog:LuisDialog<object>
    {
        protected UserMultiplePillRequest pillRequest = new UserMultiplePillRequest();
        protected IEnumerable<Pill> Pills { get; set; }
        protected int skipFactor = 3;
        const int countOfShownMorePills = 3;

        [LuisIntent("SearchPillsByName")]
        public async Task SearchPillsByName(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {        
            var foundedEntities = result.Entities;
            pillRequest.Name  = foundedEntities.TakeWhile(x => x.Type.Equals("PillName")).Select(x => x.Entity.ToString()).FirstOrDefault();
            await ShowPillsByName(context, activity);
        }
        
        public async Task ShowPillsByName(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            var pillChecker = new PillsChecker();
            Pills = pillChecker.GetPillProducts(pillRequest.Name);
            if (Pills.Count() > 0)
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(Pills, 3).Select(x => x.ToAttachment()).ToList();
            else
            {
                var groups = pillChecker.GetPillGroups(pillRequest.Name);
                if (groups.Count() > 0)
                    reply.Attachments = GenerateHeroCardView.GenerateGroupsResponse(groups).Select(x => x.ToAttachment()).ToList();
                else
                {
                    reply.Attachments.Add(GenerateHeroCardView.GenerateMessageCard("We found nothing.").ToAttachment());
                    context.Done<object>(new object());
                }
            }
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ShowPill")]
        public async Task ShowPill(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            var pillId = result.Entities.Where(x => x.Type.Equals("PillId")).Select(x => x.Entity).FirstOrDefault();
            reply.Text = new PillsChecker().GetPillUsage(pillId);
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ShowPillsByGroup")]
        public async Task ShowPillsByGroup(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            var groupId = result.Entities.Where(x => x.Type.Equals("GroupId")).Select(x => x.Entity).FirstOrDefault();
            pillRequest.Group = groupId;
            Pills = new PillsChecker().GetPillByGroupIdAndName(pillRequest.Group, pillRequest.Name);
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(Pills , 3).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("SearchPillsByMultipleParameters")]
        public async Task SearchPillsByMultipleParameters(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity as Activity;
            var reply = message.CreateReply();
            var foundedEntities = result.Entities;
            pillRequest = FillRequestDataByFoundedEntities(foundedEntities);
            var recognizedUserInput = GenerateStringFromPillRequestData(pillRequest);
            pillRequest.Colors = pillRequest.Colors == "" ? "" : VariablesSearcher.SearchColorsIdes(pillRequest.Colors);
            pillRequest.Shape = pillRequest.Shape == "" ? "" : VariablesSearcher.SearchShapeId(pillRequest.Shape);
            reply.Attachments = new List<Attachment>() { GenerateHeroCardView.GenerateMessageCard($"I found that you input \n{ recognizedUserInput}").AddButton("Add more info").AddButton("Show result").ToAttachment() };
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("ShowPillsByMultipleParameters")]
        public async Task ShowPillsByMultipleParameters(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            Pills = new PillsChecker().GetPillsByMultipleParametres(pillRequest);
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(Pills, 3).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("AddMoreInfo")]
        public async Task AddMoreInfo(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            reply.Attachments = new List<Attachment>() { GenerateHeroCardView.GenerateMessageCard($"Choose what you want to add.\n\n").AddButtons(GenerateNotFilledData(pillRequest)).ToAttachment() };
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("SetColor")]
        public async Task SetColor(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            await context.PostAsync("Please enter color");
            context.Wait(ColorReceived);   
        }

        public async Task ColorReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            pillRequest.Colors = VariablesSearcher.SearchColorsIdes(message.Text);
            await ShowPillsByMultipleParameters(context, argument, new LuisResult());
        }

        [LuisIntent("SetShape")]
        public async Task SetShape(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            await context.PostAsync("Please enter shape");
            context.Wait(ShapeReceived);
        }

        public async Task ShapeReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            pillRequest.Shape = VariablesSearcher.SearchShapeId(message.Text);
            await ShowPillsByMultipleParameters(context, argument, new LuisResult());
        }

        [LuisIntent("SetSide")]
        public async Task SetSide(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            await context.PostAsync("Please enter number");
            context.Wait(SideIdReceived);
        }

        public async Task SideIdReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (string.IsNullOrEmpty(pillRequest.FrontSideId))
                pillRequest.FrontSideId = message.Text;
            else
                pillRequest.BackSideId = message.Text;
            await ShowPillsByMultipleParameters(context, argument, new LuisResult());
            context.Wait(MessageReceived);
        }

        [LuisIntent("ShowNumberMore")]
        public async Task ShowAll(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            reply.Attachments =  GenerateHeroCardView.GenerateShowMoreResponse(Pills,skipFactor, countOfShownMorePills).Select(x => x.ToAttachment()).ToList();
            skipFactor += countOfShownMorePills;
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Reset")]
        public async Task Reset(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Thank for collaboration :)");
            context.Done<object>(new object());
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Sorry, I don't understand what are you talking about.");
            context.Done<object>(new object());
        }

        private UserMultiplePillRequest FillRequestDataByFoundedEntities(IList<EntityRecommendation> entities)
        {
            UserMultiplePillRequest request = new UserMultiplePillRequest();
            if (entities.Any(x => x.Type.Equals("Shape")))
                request.Shape = entities.Where(x => x.Type.Equals("Shape")).Select(x => x.Entity.ToString()).FirstOrDefault();
            if (entities.Any(x => x.Type.Equals("Color")))
                request.Colors = entities.Where(x => x.Type.Equals("Color")).Select(x => x.Entity.ToString()).Aggregate("", (x, y) => x += y);
            if (entities.Any(x => x.Type.Equals("FrontSideId")))
                request.FrontSideId = entities.Where(x => x.Type.Equals("FrontSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
            if (entities.Any(x => x.Type.Equals("BackSideId")))
                request.BackSideId = entities.Where(x => x.Type.Equals("BackSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
            return request;
        }

        private Dictionary<string,string> GenerateNotFilledData(UserMultiplePillRequest request)
        {
            var notFilledParams = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(request.Colors))
                notFilledParams.Add("Color", "set color");
            if (string.IsNullOrEmpty(request.Shape))
                notFilledParams.Add("Shape", "set shape");
            if (string.IsNullOrEmpty(request.FrontSideId))
                notFilledParams.Add("Front side number", "set frontSideId");
            if (string.IsNullOrEmpty(request.BackSideId))
                notFilledParams.Add("Back side number", "set backSideId");
            return notFilledParams;
        }

        private string GenerateStringFromPillRequestData(UserMultiplePillRequest request)
        {
            StringBuilder result = new StringBuilder();
            if (!string.IsNullOrEmpty(request.Colors))
                result.Append($"Colors: {request.Colors}\n");
            if (!string.IsNullOrEmpty(request.Shape))
                result.Append($"Shape: {request.Shape}\n");
            if (!string.IsNullOrEmpty(request.FrontSideId))
                result.Append($"Front side number: {request.FrontSideId}\n");
            if (!string.IsNullOrEmpty(request.BackSideId))
                result.Append($"Back side number: {request.BackSideId}");
            return result.ToString();
        }
    }
}
