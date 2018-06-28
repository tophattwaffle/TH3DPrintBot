using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using TH3DPrintBot.Commands.Readers;
using TH3DPrintBot.Models;
using TH3DPrintBot.src.Models;

namespace TH3DPrintBot.src.Services
{
    public class DataService
    {
        private readonly DiscordSocketClient _client;

        public RootSettings RootSettings { get; set; }

        // Channels
        public SocketTextChannel GeneralChannel { get; private set; }
        public SocketTextChannel LogChannel { get; private set; }

        // Roles
        public SocketRole Th3DsupeRole { get; private set; }
        public SocketRole Th3DmodRole { get; private set; }

        public DataService(DiscordSocketClient client)
        {
            _client = client;

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
    }
}
