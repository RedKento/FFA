﻿using Discord;
using Discord.Commands;
using FFA.Common;
using FFA.Database;
using FFA.Extensions;
using FFA.Preconditions;
using System.Threading.Tasks;

namespace FFA.Modules
{
    public sealed class Reputation : ModuleBase<Context>
    {
        private readonly FFAContext _ffaContext;

        public Reputation(FFAContext ffaContext)
        {
            _ffaContext = ffaContext;
        }

        [Command("rep")]
        [Summary("Give reputation to any user.")]
        public async Task Rep([Cooldown(24)] IUser user)
        {
            await _ffaContext.UpdateUser(user.Id, (x) => x.Reputation++);
            await Context.ReplyAsync($"You have successfully repped {user.Tag()}.");
        }

        [Command("unrep")]
        [Summary("Remove reputation from any user.")]
        public async Task Unrep([Cooldown(24)] IUser user)
        {
            await _ffaContext.UpdateUser(user.Id, (x) => x.Reputation--);
            await Context.ReplyAsync($"You have successfully unrepped {user.Tag()}.");
        }

        [Command("myrep")]
        [Summary("Get your current reputation.")]
        public async Task Myrep()
        {
            var dbUser = await _ffaContext.GetUser(Context.User.Id);

            await Context.DmAsync($"You currently have {dbUser.Reputation} reputation.");
            await Context.ReplyAsync($"You have been DMed with your reputation.");
        }
    }
}
