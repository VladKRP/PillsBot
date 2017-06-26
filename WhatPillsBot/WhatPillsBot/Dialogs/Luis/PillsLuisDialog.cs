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
    public class PillsLuisDialog : LuisDialog<object>
    {
        protected UserMultiplePillRequest pillRequest = new UserMultiplePillRequest();
        protected IEnumerable<Pill> Pills { get; set; }
        protected int skipFactor = 3;
        const int countOfShownMorePills = 3;

        [LuisIntent("Welcome")]
        public async Task Welcome(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Hey. My meaning is searching pills.\n\nJust enter some information about pill.\n\nIt can be name or shape, colors, numbers on pill sides.\n\nI hope I will be helpfull for you ;)");
            context.Wait(MessageReceived);
        }

        [LuisIntent("SearchPillsByName")]
        public async Task SearchPillsByName(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            ClearData();
            var foundedEntities = result.Entities;
            pillRequest.Name = foundedEntities.TakeWhile(x => x.Type.Equals("PillName")).Select(x => x.Entity.ToString()).FirstOrDefault();   
            if(string.IsNullOrEmpty(pillRequest.Name))
            {
                await context.PostAsync("Name not defined");
                context.Wait(MessageReceived);
            }
            else
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
                    reply.Attachments.Add(GenerateHeroCardView.GenerateMessageCard("I found nothing.").ToAttachment());
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
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(Pills, 3).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("SearchPillsByMultipleParameters")]
        public async Task SearchPillsByMultipleParameters(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            ClearData();
            var message = await activity as Activity;
            var reply = message.CreateReply();
            var foundedEntities = result.Entities;
            pillRequest = FillRequestDataByFoundedEntities(foundedEntities);
            //var recognizedUserInput = GenerateStringFromPillRequestData(pillRequest);
            //pillRequest.Colors = pillRequest.Colors == "" ? "" : VariablesSearcher.SearchColorsIdes(pillRequest.Colors);
            //pillRequest.Shape = pillRequest.Shape == "" ? "" : VariablesSearcher.SearchShapeId(pillRequest.Shape);
            //reply.Attachments = new List<Attachment>() { GenerateHeroCardView.GenerateMessageCard($"I found that you input \n{ recognizedUserInput}").AddButton("Add more info").AddButton("Show result").ToAttachment() };
            await ShowPillsByMultipleParameters(context, activity, result);
        }

        [LuisIntent("ShowPillsByMultipleParameters")]
        public async Task ShowPillsByMultipleParameters(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            Pills = new PillsChecker().GetPillsByMultipleParametres(pillRequest);
            var attachments = GenerateHeroCardView.GeneratePillsResponse(Pills, 3);
            if(attachments.Count() > countOfShownMorePills)
                attachments.Last().AddButton("refine");
            reply.Attachments = attachments.Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Refine")]
        public async Task Refine(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            reply.Attachments = new List<Attachment>() { GenerateHeroCardView.GenerateMessageCard($"Choose what you want to add\n\n").AddButtons(GenerateNotFilledData(pillRequest)).ToAttachment() };
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
            await context.PostAsync("Please enter imprint");
            context.Wait(SideImprintReceived);
        }

        public async Task SideImprintReceived(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (string.IsNullOrEmpty(pillRequest.FrontSideId))
                pillRequest.FrontSideId = message.Text;
            else
                pillRequest.BackSideId = message.Text;
            await ShowPillsByMultipleParameters(context, argument, new LuisResult());
        }

        [LuisIntent("ShowMore")]
        public async Task ShowMore(IDialogContext context, IAwaitable<IMessageActivity> argument, LuisResult result)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            reply.Attachments = GenerateHeroCardView.GenerateShowMoreResponse(Pills, skipFactor, countOfShownMorePills).Select(x => x.ToAttachment()).ToList();
            skipFactor += countOfShownMorePills;
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Thank")]
        public async Task Thank(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Thank for collaboration :)");
            context.Wait(MessageReceived);
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("Sorry, I don't understand what are you talking about.");
            context.Wait(MessageReceived);
        }

        private void ClearData()
        {
            pillRequest = new UserMultiplePillRequest();
            skipFactor = 3;
        }

        private UserMultiplePillRequest FillRequestDataByFoundedEntities(IList<EntityRecommendation> entities)
        {
            UserMultiplePillRequest request = new UserMultiplePillRequest();
            if (entities.Any(x => x.Type.Equals("Shape")))
                request.Shape = VariablesSearcher.SearchShapeId(entities.Where(x => x.Type.Equals("Shape")).Select(x => x.Entity.ToString()).FirstOrDefault());
            if (entities.Any(x => x.Type.Equals("Color")))
                request.Colors = VariablesSearcher.SearchColorsIdes(entities.Where(x => x.Type.Equals("Color")).Select(x => x.Entity.ToString()).Aggregate("", (x, y) => x += y));
            if (entities.Any(x => x.Type.Equals("FrontSideId")))
                request.FrontSideId = entities.Where(x => x.Type.Equals("FrontSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
            if (entities.Any(x => x.Type.Equals("BackSideId")))
                request.BackSideId = entities.Where(x => x.Type.Equals("BackSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
            return request;
        }

        private Dictionary<string, string> GenerateNotFilledData(UserMultiplePillRequest request)
        {
            var notFilledParams = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(request.Colors))
                notFilledParams.Add("Color", "set color");
            if (string.IsNullOrEmpty(request.Shape))
                notFilledParams.Add("Shape", "set shape");
            if (string.IsNullOrEmpty(request.FrontSideId))
                notFilledParams.Add("Front side imprint", "set side");
            if (string.IsNullOrEmpty(request.BackSideId))
                notFilledParams.Add("Back side imprint", "set side");
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
                result.Append($"Front side imprint: {request.FrontSideId}\n");
            if (!string.IsNullOrEmpty(request.BackSideId))
                result.Append($"Back side imprint: {request.BackSideId}");
            return result.ToString();
        }
    }
}
