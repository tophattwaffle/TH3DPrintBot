using System;
using System.Collections.Generic;
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
                string description = Regex.Replace(items[0].description, "<.*?>", String.Empty);

                if (description.Length > 300)
                    description = description.Substring(0, 300) + "...";

                await ReplyAsync("", embed: new EmbedBuilder()
                .WithAuthor(items[0].name + " $" + Math.Round((double)items[0].price, 2), _client.Guilds.FirstOrDefault()?.IconUrl, items[0].permalink)
                .WithTitle("Click Here")
                .WithUrl(items[0].permalink)
                .WithDescription(description)
                .WithImageUrl(_wooCommerce.GetProductImages(items[0])[_random.Next(items[0].images.Count)])
				.Build());
            }
            else if (items.Count < 4)
            {
                foreach (var i in items)
                {
                    string description = Regex.Replace(i.description, "<.*?>", String.Empty);

                    if (description.Length > 80)
                        description = description.Substring(0, 80) + "...";

                    await ReplyAsync("", embed: new EmbedBuilder()
                        .WithAuthor(i.name + " $" + Math.Round((double)i.price, 2), _client.Guilds.FirstOrDefault()?.IconUrl, i.permalink)
                        .WithTitle("Click Here")
                        .WithUrl(i.permalink)
                        .WithDescription(description)
                        .WithThumbnailUrl(_wooCommerce.GetProductImages(i)[_random.Next(i.images.Count)])
                        .Build());
                }
            }
            else
            {
                string reply = string.Empty;

                foreach (var i in items)
                {
                    if (reply.Length > 1800)
                        break;

                    reply = $"{reply}[{i.name}]({i.permalink}) - ${Math.Round((double)i.price, 2)}\n\n";
                }

                await ReplyAsync("", embed: new EmbedBuilder()
                    .WithAuthor("Product Search Results", _client.Guilds.FirstOrDefault()?.IconUrl, "https://www.th3dstudio.com/?s=" + search)
                    .WithTitle("Click Here")
                    .WithUrl("https://www.th3dstudio.com/?s=" + search)
                    .WithDescription(reply)
                    .WithThumbnailUrl(_client.Guilds.FirstOrDefault()?.IconUrl)
                    .WithFooter("You can click on the title to view all items on the website. Or limit search results with ~p # search")
                    .Build());
            }
        }
    }
}