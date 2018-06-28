﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TH3DPrintBot.Commands
{
    public class UtilitiesModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService _dataService;

        public UtilitiesModule(DiscordSocketClient client, DataService dataService)
        {
            _client = client;
            _dataService = dataService;
        }

        /// <summary>
        /// Displays the estimated round-trip latency, in miliseconds, to the gateway server.
        /// </summary>
        /// <seealso cref="DiscordSocketClient.Latency"/>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        [Command("ping")]
        [Summary("Replies with the bot's latency to Discord.")]
        [Remarks("The ping is the estimated round-trip latency, in milliseconds, to the gateway server.")]
        public async Task PingAsync()
        {
            var builder = new EmbedBuilder
            {
                Color = new Color(47, 111, 146),
                Description = $"It took me **{_client.Latency} ms** to reach the Discord API."
            };

            await ReplyAsync(string.Empty, false, builder.Build());
        }

        /// <summary>
        /// Toggles the invoking user's roles.
        /// </summary>
        /// <remarks>
        /// Yields a list of all toggleable roles when invoked without parameters. The roles which can be used with this command
        /// are specified in the <c>roleMeWhiteListCSV</c> config field.
        /// </remarks>
        /// <param name="roles">A case-insensitive space-delimited list of roles to toggle.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        [Command("RoleMe")]
        [Summary("Toggles a user role to show what printers they have, or what skills they have.")]
        [Remarks(
            "Toggleable roles typically display possession of a skill, such as 3D modelling or level design. To send multiple " +
            "roles in one invocation, separate the names with a space. Invoking without any parameters displays a list of " +
            "all toggleable roles.")]
        [RequireContext(ContextType.Guild)]
        public async Task RolemeAsync(
            [Summary("A case-insensitive, space-delimited list of roles to toggle.")] [Remainder]
            string roles = null)
        {
            if (string.IsNullOrWhiteSpace(roles))
            {
                await ReplyAsync($"Toggleable roles are:```\n{string.Join("\n", _dataService.RoleMeWhiteList)}```" +
                                 $"\n`Example: >roleme Fusion360 CR10` will give you both `Fusion360` and `CR10` roles.");
                return;
            }

            var roleNames = new List<string>();

            foreach (string role in _dataService.RoleMeWhiteList)
            {
                Match match = Regex.Match(roles, $@"\b{role}\b", RegexOptions.IgnoreCase);

                if (!match.Success) continue;

                // Finds and removes all occurrences of the whitelisted role in the input string.
                while (match.Success)
                {
                    roles = roles.Remove(match.Index, match.Length);
                    match = Regex.Match(roles, $@"\b{role}\b", RegexOptions.IgnoreCase);
                }

                roleNames.Add(role);
            }

            // Splits the remaining roles not found in the whitelist. Filters out empty elements.
            ImmutableArray<string> rolesInvalid = roles.Split(' ').Where(r => !string.IsNullOrWhiteSpace(r)).ToImmutableArray();

            // Finds all SocketRoles from roleNames.
            IEnumerable<SocketRole> rolesValid =
                Context.Guild.Roles.Where(r => roleNames.Contains(r.Name, StringComparer.InvariantCultureIgnoreCase));

            var user = (SocketGuildUser)Context.User;
            var rolesAdded = new List<SocketRole>();
            var rolesRemoved = new List<SocketRole>();

            // Updates roles.
            foreach (SocketRole role in rolesValid)
            {
                if (user.Roles.Contains(role))
                {
                    await ((IGuildUser)user).RemoveRoleAsync(role);
                    rolesRemoved.Add(role);
                }
                else
                {
                    await ((IGuildUser)user).AddRoleAsync(role);
                    rolesAdded.Add(role);
                }
            }

            // Builds the response.
            var logMessage = new StringBuilder();

            var embed = new EmbedBuilder();
            embed.WithTitle("`roleme` Results");
            embed.WithDescription($"Results of toggled roles for {Context.User.Mention}:");

            if (rolesAdded.Any())
            {
                string name = $"Added ({rolesAdded.Count})";

                embed.AddInlineField(name, string.Join("\n", rolesAdded.Select(r => r.Mention)));
                logMessage.AppendLine($"{name}\n    " + string.Join("\n    ", rolesAdded.Select(r => r.Name)));
            }

            if (rolesRemoved.Any())
            {
                string name = $"Removed ({rolesRemoved.Count})";

                embed.AddInlineField(name, string.Join("\n", rolesRemoved.Select(r => r.Mention)));
                logMessage.AppendLine($"{name}\n    " + string.Join("\n    ", rolesRemoved.Select(r => r.Name)));
            }

            if (rolesInvalid.Any())
            {
                string name = $"Failed ({rolesInvalid.Count()})";

                embed.AddInlineField(name, string.Join("\n", rolesInvalid));
                embed.WithFooter("Failures occur when roles don't exist or toggling them is disallowed.");
                logMessage.Append($"{name}\n    " + string.Join("\n    ", rolesInvalid));
            }

            await ReplyAsync(string.Empty, false, embed.Build());
            await _dataService.ChannelLog($"Toggled Roles for {Context.User}", logMessage.ToString());
        }
    }
}
