﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Discord;
using Discord.Commands;
using TH3DPrintBot.Commands.Preconditions;
using TH3DPrintBot.Models;
using TH3DPrintBot.src.Services;

namespace TH3DPrintBot.Services
{
    /// <inheritdoc />
    public class HelpService : IHelpService
    {
        private readonly DataService _dataService;

        public HelpService(DataService data)
        {
            _dataService = data;
        }
        /// <inheritdoc />
        /// <remarks>Commands are sorted alphabetically by name.</remarks>
        public void AddModuleField(ModuleInfo module, ref EmbedBuilder embed)
        {
            var commands = new StringBuilder();

            // Sorts commands alphabetically and builds the help strings.
            foreach (CommandInfo cmd in module.Commands.OrderBy(c => c.Name))
                commands.AppendLine($"`{cmd.Name}` - {cmd.Summary}");

            // Adds a field for the module if any commands for it were found. Removes 'Module' from the module's name.
            if (commands.Length != 0)
                embed.AddField(module.Name.Replace("Module", string.Empty), commands.ToString());
        }

        /// <inheritdoc />
        /// <remarks>
        /// <see cref="RequireContextAttribute"/> and <see cref="RequireNsfwAttribute"/> are considered contexts.
        /// </remarks>
        public string GetContexts(IEnumerable<PreconditionAttribute> preconditions)
        {
            var contexts = new List<string>();

            foreach (PreconditionAttribute precondition in preconditions)
            {
                switch (precondition)
                {
                    case RequireContextAttribute attr:
                        // Gets an enumerable of the set contexts.
                        IEnumerable<Enum> setFlags =
                            Enum.GetValues(typeof(ContextType)).Cast<Enum>().Where(m => attr.Contexts.HasFlag(m));
                        contexts.AddRange(setFlags.Select(f => f.ToString())); // Adds each set context's name.

                        break;
                    case RequireNsfwAttribute _:
                        contexts.Add("NSFW");

                        break;
                }
            }

            return string.Join("\n", contexts.OrderBy(c => c));
        }

        /// <inheritdoc />
        public string GetParameters(IReadOnlyCollection<ParameterInfo> parameters)
        {
            var param = new StringBuilder();

            foreach (ParameterInfo p in parameters)
            {
                param.Append(p.IsOptional ? $"___{p.Name}___" : $"__{p.Name}__"); // Italicises optional parameters.

                if (!string.IsNullOrWhiteSpace(p.Summary))
                    param.Append($" - {p.Summary}");

                // Appends default value if parameter is optional.
                if (p.IsOptional)
                {
                    object value = string.IsNullOrWhiteSpace(p.DefaultValue?.ToString()) ? "null/empty" : p.DefaultValue;
                    param.Append($" Default: `{value}`");
                }

                param.AppendLine();
            }

            return param.ToString();
        }

        /// <inheritdoc />
        public string GetPermissions(IEnumerable<PreconditionAttribute> preconditions)
        {
            var permissions = new List<string>();

            foreach (PreconditionAttribute precondition in preconditions)
            {
                if (precondition is RequireUserPermissionAttribute attr)
                    permissions.Add(attr.ChannelPermission?.ToString() ?? attr.GuildPermission.ToString());
            }

            return string.Join("\n", permissions.OrderBy(p => p));
        }

        /// <inheritdoc />
        /// <remarks>
        /// Attempts to fetch role names from the attribute if the string constructor was used. Otherwise, if the context is a
        /// guild, converts IDs to names. If not in a guild, the name in <see cref="Role"/> is used. If it's not in the enum,
        /// the raw ID is displayed.
        /// </remarks>
        public string GetRoles(IEnumerable<PreconditionAttribute> preconditions, ICommandContext context)
        {
            var roles = new List<string>();

            foreach (PreconditionAttribute precondition in preconditions)
            {
                if (precondition is RequireRoleAttribute attr)
                {
                    roles.AddRange(
                        attr.RoleNames ??
                        context.Guild?.Roles.Where(r => attr.RoleIds.Contains(r.Id)).Select(r => r.Name) ??
                        attr.RoleIds.Select(id => ((Role)id).ToString()));
                }
            }

            return string.Join("\n", roles.OrderBy(r => r));
        }

        /// <inheritdoc />
        /// <remarks>
        /// Contains the command's prefix, name, and parameters. Normal parameters are surrounded in square brackets, optional
        /// ones in angled brackets.
        /// </remarks>
        public string GetUsage(CommandInfo command) =>
            _dataService.RootSettings.program_settings.commandPrefix[0] +
            command.Name +
            " " +
            string.Join(" ", command.Parameters.Select(p => p.IsOptional ? $"<{p.Name}>" : $"[{p.Name}]"));
    }
}
