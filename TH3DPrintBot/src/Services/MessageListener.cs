using System;
using System.Threading.Tasks;
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
            //Not yet used
        }
    }
}
