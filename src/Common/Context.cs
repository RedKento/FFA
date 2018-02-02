﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FFA.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace FFA.Common
{
    public class Context : SocketCommandContext
    {
        private readonly IServiceProvider _provider;
        private readonly Sender _sender;

        public Context(DiscordSocketClient client, SocketUserMessage msg, IServiceProvider provider) : base(client, msg)
        {
            _provider = provider;
            _sender = _provider.GetRequiredService<Sender>();
        }

        public async Task DmAsync(string description)
        {
            await _sender.SendAsync(await User.GetOrCreateDMChannelAsync(), description);
        }

        public Task SendAsync(string description, string title = null, Color? color = null)
        {
            return _sender.SendAsync(Channel, description, title, color);
        }

        public Task ReplyAsync(string description, string title = null, Color? color = null)
        {
            return _sender.ReplyAsync(User, Channel, description, title, color);
        }

        public Task ReplyErrorAsync(string description)
        {
            return _sender.ReplyErrorAsync(User, Channel, description);
        }
    }
}
