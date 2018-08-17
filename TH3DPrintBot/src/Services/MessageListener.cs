using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TH3DPrintBot.src.Services
{
    class MessageListener
    {
        private readonly DiscordSocketClient _client;
        private readonly DataService _dataService;

        public MessageListener(DiscordSocketClient client, DataService dataService)
        {
            _client = client;
            _dataService = dataService;
        }

        /// <summary>
        /// This is used to scan each message for less important things.
        /// Mostly used for shit posting, but also does useful things like nag users
        /// to use more up to date tools, or automatically answer some simple questions.
        /// </summary>
        /// <param name="message">Message that got us here</param>
        /// <returns></returns>
        internal async Task Listen(SocketMessage message)
        {
            if (_dataService.RootSettings.autoReplies.firmware.Any(s => message.Content.ToLower().Contains(s)))
            {
                await Firmware(message);

                return;
            }

            if (_dataService.RootSettings.autoReplies.bootLoader.Any(s => message.Content.ToLower().Contains(s)))
            {
                await BootLoader(message);

                return;
            }
        }

        private async Task Firmware(SocketMessage message)
        {
            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,

                Title = $"Click here to go to the firmware page!",
                Url = "https://www.th3dstudio.com/knowledge-base/th3d-unified-firmware/",
                ThumbnailUrl = "https://www.th3dstudio.com/wp-content/uploads/2017/08/DiviLogo.fw_-3.png",
                Color = new Color(243, 128, 72),

                Description = "Based on your previous message, I believe that you may be looking for information on TH3D Unified Firmware. " +
                              "This page is full of helpful information, along with downloads."
            };

            await message.Channel.SendMessageAsync("", false, builder);
        }

        private async Task BootLoader(SocketMessage message)
        {
            var authBuilder = new EmbedAuthorBuilder()
            {
                Name = $"Hey there {message.Author.Username}!",
                IconUrl = message.Author.GetAvatarUrl(),
            };

            var builder = new EmbedBuilder()
            {
                Author = authBuilder,

                Title = $"Click here to go to the CR-10 Firmware flashing guide",
                Url = "https://www.th3dstudio.com/knowledge-base/cr-10-bootloader-flashing-guide/",
                ThumbnailUrl = "https://www.th3dstudio.com/wp-content/uploads/2017/08/DiviLogo.fw_-3.png",
                Color = new Color(243, 128, 72),

                Description = "Based on your previous message, I believe that you may be looking for information on flashing your bootloader. " +
                              "\n\nIf you have a Anet, you'll need to visit this link instead: https://www.th3dstudio.com/knowledge-base/anet-bootloader-flashing-video-guide/"
            };

            await message.Channel.SendMessageAsync("", false, builder);
        }
    }
}
