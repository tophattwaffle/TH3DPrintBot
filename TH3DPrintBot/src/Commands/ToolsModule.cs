using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TH3DPrintBot.src.Commands
{
    public class ToolsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public ToolsModule(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("UnifiedFirmware")]
        [Summary("Provides a link to UnifiedFirmware")]
        [Alias("uf")]
        public async Task UnifiedFirmwareAsync()
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Unified Firmware",
                    IconUrl = _client.Guilds.FirstOrDefault()?.IconUrl
                },
                Title = "Click Here",
                Url = "https://www.th3dstudio.com/knowledge-base/th3d-unified-firmware/",
                Color = new Color(130, 203, 225),
                Description = "[Unified Firmware](https://www.th3dstudio.com/knowledge-base/th3d-unified-firmware/) is a custom " +
                              "branch of Marlin built by TH3D that is used to make flashing a custom firmware with additional features easier. " +
                              "This is the firmware that you will use if you have purchased an EZABL kit. Before flashing UF " +
                              "you'll need to flash a bootloader. Type `~bootloader` for more information on that."
            };

            await ReplyAsync(string.Empty, false, embed.Build());
        }

        [Command("Bootloader")]
        [Summary("Provides a link to bootloader flashing guides")]
        public async Task BootloaderAsync()
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "Boot Loader Flashing Guide",
                    IconUrl = _client.Guilds.FirstOrDefault()?.IconUrl,
                },
                Title = "Click Here",
                Url = "https://www.th3dstudio.com/knowledge-base/cr-10-bootloader-flashing-guide/",
                Color = new Color(130, 203, 225),
                Description = "Flashing a bootloder is required before you can easily flash firmware. This process is simple " +
                              "and will require a Arduino UNO to flash it." +
                              "\n[CR-10 Bootloader Flashing Guide](https://www.th3dstudio.com/knowledge-base/cr-10-bootloader-flashing-guide/)" +
                              "\n[ANET Bootloader Flashing Guide](https://www.th3dstudio.com/knowledge-base/anet-bootloader-flashing-video-guide/)"
            };

            await ReplyAsync(string.Empty, false, embed.Build());
        }

        [Command("pid")]
        [Summary("Provides a link to PID auto-tuning guide")]
        public async Task PIDAsync()
        {
            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = "PID Auto Tune Guide",
                    IconUrl = _client.Guilds.FirstOrDefault()?.IconUrl
                },
                Title = "Click Here",
                Url = "https://www.th3dstudio.com/knowledge-base/p-i-d-auto-tune-guide/",
                Color = new Color(130, 203, 225),
                Description = "PID [Proportional Integral Derivative](https://en.wikipedia.org/wiki/PID_controller) " +
                              "is the system the printers use for holding a set " +
                              "temperature. This system controls how fast the printer reaches the set temperature and how well " +
                              "it holds that temperature once it gets there. Without it numerous problems can arise. Fortunately " +
                              "for us, the printer has an automatic way of tuning these values."
            };

            await ReplyAsync(string.Empty, false, embed.Build());
        }
    }
}