using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;
using TH3DPrintBot.Commands.Readers;
using TH3DPrintBot.Models;
using TH3DPrintBot.src.Models;

namespace TH3DPrintBot.src.Services
{
    public class DataService
    {
        private readonly DiscordSocketClient _client;
        private readonly Random _random;

        public RootSettings RootSettings { get; set; }

        // Channels
        public SocketTextChannel GeneralChannel { get; private set; }
        public SocketTextChannel LogChannel { get; private set; }

        // Roles
        public SocketRole Th3DsupeRole { get; private set; }
        public SocketRole Th3DmodRole { get; private set; }

        public DataService(DiscordSocketClient client, Random random)
        {
            _client = client;
            _random = random;

            // Some settings are needed before the client connects (e.g. token).
            ReadConfig();
            DeserialiseSettings();
        }

        public async Task DeserialiseConfig()
        {
            ReadConfig();
            await DeserialiseChannels();
            GetRoles();
            DeserialiseSettings();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("SETTINGS HAVE BEEN LOADED\n");
            Console.ResetColor();
        }

        private void ReadConfig()
        {
            const string CONFIG_PATH = "settings.json";

            RootSettings = JsonConvert.DeserializeObject<RootSettings>(File.ReadAllText(CONFIG_PATH));
		}

        private void DeserialiseSettings()
        {

        }

        /// <summary>
        /// Deserialises channels from the configuration file.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a channel can't be found.</exception>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        private async Task DeserialiseChannels()
        {
            SocketGuild guild = _client.Guilds.FirstOrDefault();

            LogChannel = await ParseChannel(RootSettings.program_settings.logChannel);
            GeneralChannel = await ParseChannel(RootSettings.general.welcomeChannel);

            async Task<SocketTextChannel> ParseChannel(string key)
            {
                SocketTextChannel channel = await ChannelTypeReader<SocketTextChannel>.GetBestResultAsync(guild, key);

                if (channel == null)
                    throw new InvalidOperationException($"The value of key '{key}' could not be parsed as a channel.");

                return channel;
            }
        }

        /// <summary>
        /// Retrieves role socket entities from the IDs in the <see cref="Role"/> enum.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a role can't be found.</exception>
        private void GetRoles()
        {
            SocketGuild guild = _client.Guilds.FirstOrDefault();

            Th3DmodRole = GetRole(Role.th3dmod);
            Th3DsupeRole = GetRole(Role.th3dsuper);

            SocketRole GetRole(Role role)
            {
                SocketRole r = guild?.GetRole((ulong)role);

                if (r == null)
                    throw new InvalidOperationException($"The role '{role}' could not be found.");

                return r;
            }
        }

        /// <summary>
        /// Logs a message to the log channel
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="mention">Alert TopHATTwaffle?</param>
        /// <returns>Task</returns>
        public Task ChannelLog(string message, bool mention = false)
        {
            string alert = null;
            if (mention && !string.IsNullOrWhiteSpace(RootSettings.program_settings.alertUser))
            {
                var splitUser = RootSettings.program_settings.alertUser.Split('#');
                alert = _client.GetUser(splitUser[0], splitUser[1]).Mention;
            }

            LogChannel.SendMessageAsync($"{alert}```{DateTime.Now}\n{message}```");
            Console.WriteLine($"{DateTime.Now}: {message}\n");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs a message to the log channel
        /// </summary>
        /// <param name="title">Title of log</param>
        /// <param name="message">Message to log</param>
        /// <param name="mention">Alert TopHATTwaffle?</param>
        /// <returns>Task</returns>
        public Task ChannelLog(string title, string message, bool mention = false)
        {
            string alert = null;
            if (mention)
            {
                var splitUser = RootSettings.program_settings.alertUser.Split('#');
                alert = _client.GetUser(splitUser[0], splitUser[1]).Mention;
            }

            LogChannel.SendMessageAsync($"{alert}```{DateTime.Now}\n{title}\n{message}```");
            Console.WriteLine($"{DateTime.Now}: {title}\n{message}\n");
            return Task.CompletedTask;
        }

        public List<List<string>> SearchKb(string searchTerm)
        {
            List<List<string>> listResults = new List<List<string>>();

            const string FAQURL = "https://www.th3dstudio.com/wp-admin/admin-ajax.php?action=epkb-search-kb&epkb_kb_id=1&search_words=";
            try
            {
                //New web client
                HtmlWeb kbWeb = new HtmlWeb();

                //Let's load the search
                HtmlDocument kbDocument = kbWeb.Load($"{FAQURL}{searchTerm}");

                //Look at all links that the page has on it
                foreach (HtmlNode link in kbDocument.DocumentNode.SelectNodes("//a[@href]"))
                {
                    List<string> singleResult = new List<string>();

                    //Setup the web request for this specific link found. Format it so we can get data about it.
                    string finalUrl = link.GetAttributeValue("href", string.Empty).Replace(@"\", "").Replace("\"", "");
                    HtmlWeb htmlWeb = new HtmlWeb();
                    HtmlDocument htmlDocument = htmlWeb.Load(finalUrl);

                    //Get page title.
                    string title = (from x in htmlDocument.DocumentNode.Descendants()
                                    where x.Name.ToLower() == "title"
                                    select x.InnerText).FirstOrDefault();

                    //Get article content, this is by ID. Only works for my site.
                    string description = null;
                    if (finalUrl.ToLower().Contains("th3dstudio"))
                    {
                        description = htmlDocument.GetElementbyId("kb-article-content").InnerText;
                    }

                    //Only if not Null - Fix the bad characters that get pulled from the web page.
                    description = description?.Replace(@"&#8211;", "-").Replace("\n", "").Replace(@"&#8220;", "\"").Replace(@"&#8221;", "\"").Replace(@"&#8217;", "'");
                    title = title?.Replace(@"&#8211;", "-").Replace("\n", "").Replace(" | TopHATTwaffle", "").Replace(@"&#8220;", "\"").Replace(@"&#8221;", "\"").Replace(@"&#8217;", "'");

                    //Limit length if needed
                    if (description != null && description.Length >= 180)
                        description = description.Substring(0, 180) + "...";

                    //Get images on the page
                    List<string> imgs = (from x in htmlDocument.DocumentNode.Descendants()
                                         where x.Name.ToLower() == "img"
                                         select x.Attributes["src"].Value).ToList<String>();

                    //Set image to the first non-header image if it exists.
                    string finalImg = "https://www.th3dstudio.com/wp-content/uploads/2017/08/DiviLogo.fw_-3.png";
                    if (imgs.Count > 1)
                        finalImg = imgs[_random.Next(0, imgs.Count)];

                    //Add results to list.
                    singleResult.Add(title);
                    singleResult.Add(finalUrl);
                    singleResult.Add(description);
                    singleResult.Add(finalImg);
                    listResults.Add(singleResult);
                }
            }
            catch (Exception)
            {
                //Do nothing. The command that called this will handle the no results found message.
            }
            return listResults;
        }
    }
}
