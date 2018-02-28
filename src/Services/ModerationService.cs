using Discord;
using FFA.Common;
using FFA.Database.Models;
using FFA.Extensions.Database;
using FFA.Extensions.Discord;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FFA.Services
{
    // TODO: proper types, int vs uint, etc
    public sealed class ModerationService
    {
        private readonly IMongoCollection<Guild> _guildCollection;

        public ModerationService(IMongoCollection<Guild> guildCollection)
        {
            _guildCollection = guildCollection;
        }

        public Task LogMuteAsync(Context context, IUser subject, Rule rule, TimeSpan length, string reason = null)
        {
            var elements = new List<(string, string)>
            {
                ("Action", "Mute"),
                ("User", $"{subject} ({subject.Id})"),
                ("Rule", rule.Content),
                ("Length", $"{length.TotalHours}h")
            };

            if (!string.IsNullOrWhiteSpace(reason))
                elements.Add(("Reason", reason));

            return LogAsync(context.Guild, elements, Configuration.MUTE_COLOR, context.User);
        }

        public Task LogUnmuteAsync(Context context, IUser subject, string reason = null)
        {
            var elements = new List<(string, string)>
            {
                ("Action", "Unmute"),
                ("User", $"{subject} ({subject.Id})")
            };

            if (!string.IsNullOrWhiteSpace(reason))
                elements.Add(("Reason", reason));

            return LogAsync(context.Guild, elements, Configuration.UNMUTE_COLOR, context.User);
        }

        public Task LogAutoMuteAsync(Context context, TimeSpan length)
            => LogAsync(context.Guild, new(string, string)[]
            {
                ("Action", "Automatic Mute"),
                ("User", $"{context.User} ({context.User.Id})"),
                ("Length", $"{length.TotalHours}h")
            }, Configuration.MUTE_COLOR);

        public Task LogAutoUnmuteAsync(IGuild guild, IUser subject)
            => LogAsync(guild, new(string, string)[]
            {
                ("Action", "Automatic Unmute"),
                ("User", $"{subject} ({subject.Id})")
            }, Configuration.UNMUTE_COLOR);

        public Task LogClearAsync(Context context, IUser subject, Rule rule, int quantity, string reason = null)
        {
            var elements = new List<(string, string)>
            {
                ("Action", "Clear"),
                ("User", $"{subject} ({subject.Id})"),
                ("Rule", rule.Content),
                ("Quantity", quantity.ToString())
            };

            if (!string.IsNullOrWhiteSpace(reason))
                elements.Add(("Reason", reason));

            return LogAsync(context.Guild, elements, Configuration.CLEAR_COLOR, context.User);
        }

        public async Task LogAsync(IGuild guild, IReadOnlyCollection<(string, string)> elements, Color color, IUser moderator = null)
        {
            var dbGuild = await _guildCollection.GetGuildAsync(guild.Id);

            if (!dbGuild.LogChannelId.HasValue)
                return;

            var logChannel = await guild.GetChannelAsync(dbGuild.LogChannelId.Value) as ITextChannel;

            if (logChannel == null || !await logChannel.CanSendAsync())
                return;

            var description = string.Empty;

            foreach (var element in elements)
                description += $"**{element.Item1}:** {element.Item2}\n";

            var builder = new EmbedBuilder()
            {
                Timestamp = DateTimeOffset.UtcNow,
                Footer = new EmbedFooterBuilder { Text = $"Case #{dbGuild.LogCase}" },
                Description = description,
                Color = color
            };

            if (moderator != null)
            {
                builder.WithAuthor(new EmbedAuthorBuilder
                {
                    Name = $"{moderator} ({moderator.Id})",
                    IconUrl = moderator.GetAvatarUrl()
                });
            }

            await logChannel.SendMessageAsync("", false, builder.Build());
            await _guildCollection.UpdateAsync(dbGuild, x => x.LogCase++);
        }
    }
}
