using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TH3DPrintBot.src.Services;
using TH3DPrintBot.src.Services.WooCommerce;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v2;

namespace TH3DPrintBot.src.Commands
{
    public class InformationModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService _dataService;
        private readonly Random _random;
        private readonly WooCommerce _wooCommerce;

        public InformationModule(DiscordSocketClient client, DataService data, Random random, WooCommerce wooCommerce)
        {
            _client = client;
            _dataService = data;
            _random = random;
            _wooCommerce = wooCommerce;
        }

        [Command("gcode", RunMode = RunMode.Async)]
        [Summary("Gets information on gcode commands.")]
        [Remarks("Searches the Marlin website's documentation on gcode commands.")]
        [Alias("gc")]
        public async Task GcodeAsync([Remainder] string searchTerm)
        {
            if (searchTerm.Length < 2)
            {
                await ReplyAsync("Searching GCode requires at least 2 characters or more in the search term. Please try again.");
                return;
            }

            IUserMessage wait = await ReplyAsync(
                $":eyes: Searching for **{searchTerm}** in the GCode Docs. This may take a moment depending on the results! :eyes:");

            List<List<string>> results = _dataService.SearchGcode(searchTerm); // Performs a search.

            // Notifies the user of a lack of search results.
            if (!results.Any())
            {
                results.Add(
                    new List<string>
                    {
                        "Try a different search term",
                        "http://marlinfw.org/meta/gcode/",
                        "I could not locate anything for the search term you provided. Please try a different search term.",
                        null
                    });
            }

            //Limit results for GCODE specific searches, we know what we want.
            if (searchTerm.ToUpper()[0] == 'M' || searchTerm.ToUpper()[0] == 'G')
                results.RemoveRange(1, results.Count - 1);

            if (results.Count == 1)
            {
                string description = results[0][2];

                if (description.Length > 400)
                    description = description.Substring(0, 400) + "...";

                await ReplyAsync("", embed: new EmbedBuilder()
                .WithAuthor(results[0][0], _client.Guilds.FirstOrDefault()?.IconUrl, results[0][1])
                .WithTitle("Click Here")
                .WithUrl(results[0][1])
                .WithColor(130, 203, 225)
                .WithDescription(description)
                .Build());
            }
            else if (results.Count < 4)
            {
                foreach (var r in results)
                {
                    string description = r[2];

                    if (description.Length > 200)
                        description = description.Substring(0, 200) + "...";

                    await ReplyAsync("", embed: new EmbedBuilder()
                        .WithAuthor(r[0])
                        .WithTitle("Click Here")
                        .WithUrl(r[1])
                        .WithColor(130, 203, 225)
                        .WithDescription(description)
                        .WithThumbnailUrl(_client.Guilds.FirstOrDefault()?.IconUrl)
                        .Build());
                }
            }
            else
            {
                string reply = string.Empty;

                int count = 1;

                foreach (var r in results)
                {
                    if (reply.Length > 1800)
                        break;

                    reply = $"{reply}{count}. [{r[0]}]({r[1]})\n\n";

                    count++;
                }

                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithAuthor("GCODE Search Results", _client.Guilds.FirstOrDefault()?.IconUrl, "http://marlinfw.org/meta/gcode/")
                    .WithUrl("https://www.th3dstudio.com/knowledgebase/")
                    .WithDescription(reply)
                    .WithColor(130, 203, 225)
                    .WithThumbnailUrl(_client.Guilds.FirstOrDefault()?.IconUrl)
                    .WithFooter("Marlin Rocks!")
                    .Build());
            }

            if (!Context.IsPrivate)
                await wait.DeleteAsync();
        }

        [Command("KB", RunMode = RunMode.Async)]
        [Summary("Gets information on in the knowledge base.")]
        [Remarks("You can limit results to get more information on a specific KB. You can do this by putting the limit before " +
                 "the search term. Example: `~KB 3 cr-10` will limit to 3 results when searching for cr-10")]
        public async Task KBAsync([Remainder] string searchTerm)
        {
            string search = searchTerm;

            int limit = 0;

            if (Regex.IsMatch(search, @"^\d+"))
            {
                limit = Int32.Parse(Regex.Match(search, @"\d+").Value);
                search = Regex.Replace(search, @"^\d+", "").Trim();
            }

            IUserMessage wait = await ReplyAsync(
                $":eyes: Searching for **{search}** in the Knowledge Base. This may take a moment depending on the results! :eyes:");

            List<List<string>> results = _dataService.SearchKb(search); // Performs a search.

            // Trim to limit
            if (limit != 0 && results.Count > limit)
                results.RemoveRange(limit, results.Count - limit);

            // Notifies the user of a lack of search results.
            if (!results.Any())
            {
                results.Add(
                    new List<string>
                    {
                        "Try a different search term",
                        "https://www.th3dstudio.com/knowledgebase/",
                        "I could not locate anything for the search term you provided. Please try a different search term.",
                        null
                    });
            }

            if (results.Count == 1)
            {
                string description = results[0][2];

                if (description.Length > 400)
                    description = description.Substring(0, 400) + "...";

                await ReplyAsync("", embed: new EmbedBuilder()
                .WithAuthor(results[0][0], _client.Guilds.FirstOrDefault()?.IconUrl, results[0][1])
                .WithTitle("Click Here")
                .WithUrl(results[0][1])
                .WithColor(130, 203, 225)
                .WithDescription(description)
                .WithImageUrl(results[0][3])
                .Build());
            }
            else if (results.Count < 4)
            {
                foreach (var r in results)
                {
                    string description = r[2];

                    if (description.Length > 200)
                        description = description.Substring(0, 200) + "...";

                    await ReplyAsync("", embed: new EmbedBuilder()
                        .WithAuthor(r[0])
                        .WithTitle("Click Here")
                        .WithUrl(r[1])
                        .WithColor(130, 203, 225)
                        .WithDescription(description)
                        .WithThumbnailUrl(r[3])
                        .Build());
                }
            }
            else
            {
                string reply = string.Empty;

                int count = 1;

                foreach (var r in results)
                {
                    if (reply.Length > 1800)
                        break;

                    reply = $"{reply}{count}. [{r[0]}]({r[1]})\n\n";

                    count++;
                }

                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithAuthor("Knowledge Base Search Results", _client.Guilds.FirstOrDefault()?.IconUrl, "https://www.th3dstudio.com/knowledgebase/")
                    .WithUrl("https://www.th3dstudio.com/knowledgebase/")
                    .WithDescription(reply)
                    .WithColor(130, 203, 225)
                    .WithThumbnailUrl(_client.Guilds.FirstOrDefault()?.IconUrl)
                    .WithFooter("You can limit search results with ~kb # search")
                    .Build());
            }

            if (!Context.IsPrivate)
                await wait.DeleteAsync();
        }

        [Command("Product", RunMode = RunMode.Async)]
        [Summary("Gets information on products.")]
		[Remarks("You can limit results to get more information on a specific item. You can do this by putting the limit before " +
		         "the search term. Example: `~p 3 cr-10` will limit to 3 results when searching for cr-10")]
        [Alias("p")]
        public async Task ProductAsync([Remainder]string searchTerm)
        {
            string search = searchTerm;

            int limit = 0;

            if (Regex.IsMatch(search, @"^\d+"))
            {
                limit = Int32.Parse(Regex.Match(search, @"\d+").Value);
                search = Regex.Replace(search, @"^\d+", "").Trim();
            }

            IUserMessage wait = await ReplyAsync(
                $":eyes: Searching for **{search}** in the product database. This may take a moment! :eyes:");

            var items = _wooCommerce.SearchProducts(search).Result;

            // Trim to limit
            if (limit != 0 && items.Count > limit)
                items.RemoveRange(limit, items.Count - limit);

            if (items.Count == 0)
            {
                await ReplyAsync("I could not find any items with that keyword. Please try again with another keyword.");
                return;
            }

            if (items.Count == 1)
            {
                //strip HTML tags
                string description = Regex.Replace(items[0].description, "<.*?>", String.Empty);

                if (description.Length > 300)
                    description = description.Substring(0, 300) + "...";

                await ReplyAsync("", embed: new EmbedBuilder()
                .WithAuthor(items[0].name + " $" + Math.Round((double)items[0].price, 2), _client.Guilds.FirstOrDefault()?.IconUrl, items[0].permalink)
                .WithTitle("Click Here")
                .WithUrl(items[0].permalink)
                .WithColor(130, 203, 225)
                .WithDescription(description)
                .WithImageUrl(_wooCommerce.GetProductImages(items[0])[_random.Next(items[0].images.Count)])
				.Build());
            }
            else if (items.Count < 4)
            {
                foreach (var i in items)
                {
                    //strip HTML tags
                    string description = Regex.Replace(i.description, "<.*?>", String.Empty);

                    if (description.Length > 80)
                        description = description.Substring(0, 80) + "...";

                    await ReplyAsync("", embed: new EmbedBuilder()
                        .WithAuthor(i.name + " $" + Math.Round((double)i.price, 2), _client.Guilds.FirstOrDefault()?.IconUrl, i.permalink)
                        .WithTitle("Click Here")
                        .WithUrl(i.permalink)
                        .WithColor(130, 203, 225)
                        .WithDescription(description)
                        .WithThumbnailUrl(_wooCommerce.GetProductImages(i)[_random.Next(i.images.Count)])
                        .Build());
                }
            }
            else
            {
                string reply = string.Empty;

                int count = 1;

                foreach (var i in items)
                {
                    if (reply.Length > 1800)
                        break;

                    reply = $"{reply}{count}. [{i.name}]({i.permalink}) - ${Math.Round((double)i.price, 2)}\n\n";

                    count++;
                }

                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithAuthor("View All Products", _client.Guilds.FirstOrDefault()?.IconUrl, "https://www.th3dstudio.com/shop/")
                    .WithUrl("https://www.th3dstudio.com/shop/")
                    .WithDescription(reply)
                    .WithColor(130, 203, 225)
                    .WithThumbnailUrl(_client.Guilds.FirstOrDefault()?.IconUrl)
                    .WithFooter("You can click on the title to view all items on the website. Or limit search results with ~p # search")
                    .Build());
            }

            if (!Context.IsPrivate)
                await wait.DeleteAsync();
        }

        [Command("CatFact", RunMode = RunMode.Async)]
        [Summary("Provides a cat fact!")]
        [Remarks("Ever want to know more about cats? Now you can.")]
        [Alias("gimme a cat fact", "hit me with a cat fact", "hit a nigga with a cat fact", "cat fact", "CatFacts", "cat facts")]
        public async Task CatFactAsync()
        {
            var catFact = "Did you know cats have big bushy tails?";
            var name = "Cat Fact 0";

            // Gets a fact from the file.
            if (File.Exists(_dataService.RootSettings.program_settings.catFactsPath))
            {
                string[] allLines = File.ReadAllLines(_dataService.RootSettings.program_settings.catFactsPath);
                int lineNumber = _random.Next(0, allLines.Length);
                catFact = allLines[lineNumber];

                // Splits the name and the fact in the selected line.
                Match match = Regex.Match(catFact, @"^\w+ Fact \d*", RegexOptions.IgnoreCase);
                name = match.Value;
                catFact = catFact.Substring(match.Length).Trim();
            }

            var embed = new EmbedBuilder
            {
                ThumbnailUrl = _dataService.GetRandomImgFromUrl("https://content.tophattwaffle.com/BotHATTwaffle/catfacts/"),
                Color = new Color(230, 235, 240)
            };

            embed.WithAuthor("CAT FACTS!", Context.Message.Author.GetAvatarUrl());
            embed.WithFooter("This was cat facts, you cannot unsubscribe.");
            embed.AddField(name, catFact);

            await _dataService.ChannelLog($"{Context.Message.Author.Username.ToUpper()} JUST GOT HIT WITH A CAT FACT");
            await ReplyAsync(string.Empty, false, embed.Build());
        }
    }
}