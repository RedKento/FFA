using Discord;
using Discord.Commands;
using FFA.Common;
using FFA.Extensions.Discord;
using FFA.Extensions.System;
using FFA.Preconditions.Parameter;
using FFA.Services;
using System.Linq;
using System.Threading.Tasks;

namespace FFA.Modules
{
    [Name("Utility")]
    [Summary("General utility commands.")]
    public sealed class Utility : ModuleBase<Context>
    {
        private readonly DeletedMessagesService _deletedMsgsService;

        public Utility(DeletedMessagesService deletedMsgsService)
        {
            _deletedMsgsService = deletedMsgsService;
        }

        [Command("AltCheck")]
        [Alias("alt")]
        [Summary("Verifies whether or not a user is an alterate account.")]
        public Task AltCheck(
            [Summary("Jimbo Steve#8842")] [Remainder] IGuildUser guildUser)
        {
            var joinedAt = guildUser.JoinedAt.GetValueOrDefault();

            return Context.SendAsync(
                $"**Created:** `{guildUser.CreatedAt.ToString("f")}`\n" +
                $"**Joined:** `{joinedAt.ToString("f")}`\n" +
                $"**Difference:** `{joinedAt.Subtract(guildUser.CreatedAt)}`", guildUser.ToString());
        }

        [Command("Deleted")]
        [Alias("deletedmessages", "deletedmsgs")]
        [Summary("Gets the last deleted messages of the channel.")]
        public Task Deleted(
            [Summary("5")] [Between(Config.MIN_DELETED_MSGS, Config.MAX_DELETED_MSGS)] int count = Config.DELETED_MSGS)
        {
            var deletedMsgs = _deletedMsgsService.GetLast(Context.Channel.Id, count);
            var response = string.Empty;

            foreach (var msg in deletedMsgs)
                response += $"{msg.Author.Bold()}: {new string(msg.Content.Take(Config.DELETED_MESSAGES_CHARS).ToArray())}" +
                    $"{(msg.Content.Length > Config.DELETED_MESSAGES_CHARS ? "..." : string.Empty)}\n\n";

            if (!string.IsNullOrWhiteSpace(response))
                return Context.SendAsync(response, $"{Context.Channel.Name.UpperFirstChar()}'s Deleted Messages");
            else
                return Context.ReplyErrorAsync("There have been no recent deleted messages in this channel.");
        }
    }
}
