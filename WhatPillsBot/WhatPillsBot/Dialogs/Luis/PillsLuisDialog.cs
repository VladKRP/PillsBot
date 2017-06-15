using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WhatPillsBot.Extensions;
using WhatPillsBot.Model;
using WhatPillsBot.Services;

namespace WhatPillsBot.Dialogs.Luis
{
    [LuisModel("d79b7e46-7b62-4a37-b582-c6a5a4a42289", "86cb2f447fc34bed9e2084c761a480d6")]
    [Serializable]
    public class PillsLuisDialog:LuisDialog<object>
    {
        protected UserMultiplePillRequest pillRequest = new UserMultiplePillRequest();

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
            var products = pillChecker.GetPillProducts(pillRequest.Name);
            if (products.Count() > 0)
                reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(products, 3).Select(x => x.ToAttachment()).ToList();
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
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillByGroupIdAndName(pillRequest.Group, pillRequest.Name), 3).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("SearchPillsByMultipleParamenters")]
        public async Task SearchPillsByMultipleParameters(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var foundedEntities = result.Entities;
            FillRequestDataByFoundedEntities(foundedEntities);
            var recognizedUserInput = $"Colors:{pillRequest.Colors}\nShapes:{pillRequest.Shape}\nFrontSideId:{pillRequest.FrontSideId}\nBackSideId:{pillRequest.BackSideId}";
            PromptDialog.Confirm(context, AddMoreInfo, $"We found that you input {recognizedUserInput}. Do you want to add something?");
        }
    


        public async Task AddMoreInfo(IDialogContext context, IAwaitable<bool> argument)
        {
            if(await argument)
            {
                //PromptDialog.Choice<>(context,)
                //add more info
                if(string.IsNullOrEmpty(pillRequest.Shape))
                {

                }
                if(string.IsNullOrEmpty(pillRequest.Colors))
                {

                }
                if (string.IsNullOrEmpty(pillRequest.FrontSideId))
                {

                }
                if (string.IsNullOrEmpty(pillRequest.BackSideId))
                {

                }

            }
            else
            {
                //context.Wait(ShowPillsByMultipleParameters);
                //show pills
            }

        }

        public async Task ShowPillsByMultipleParameters(IDialogContext context, IAwaitable<IDialogContext> argument)
        {
            var message = await argument as Activity;
            var reply = message.CreateReply();
            reply.Attachments = GenerateHeroCardView.GeneratePillsResponse(new PillsChecker().GetPillsByMultipleParametres(pillRequest), 3).Select(x => x.ToAttachment()).ToList();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        private void FillRequestDataByFoundedEntities(IList<EntityRecommendation> entities)
        {
            if (entities.Any(x => x.Type.Equals("Shape")))
                pillRequest.Shape = VariablesSearcher.SearchShapeId(entities.Where(x => x.Type.Equals("Shape")).Select(x => x.Entity.ToString()).FirstOrDefault());
            if (entities.Any(x => x.Type.Equals("Color")))
                pillRequest.Colors = VariablesSearcher.SearchColorsIdes(entities.Where(x => x.Type.Equals("Color")).Select(x => x.Entity.ToString()).Aggregate("", (x, y) => x += y));
            if (entities.Any(x => x.Type.Equals("FrontSideId")))
                pillRequest.FrontSideId = entities.Where(x => x.Type.Equals("FrontSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
            if (entities.Any(x => x.Type.Equals("BackSideId")))
                pillRequest.BackSideId = entities.Where(x => x.Type.Equals("BackSideId")).Select(x => x.Entity.ToString()).FirstOrDefault();
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            await context.PostAsync("I have no idea what are you talking about");
            context.Wait(MessageReceived);
        }

    }
}