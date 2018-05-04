using Discord;
using FFA.Common;
using FFA.Entities.Service;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace FFA.Services
{
    public sealed class DeletedMessagesService : Service
    {
        private readonly ConcurrentDictionary<ulong, ConcurrentQueue<IUserMessage>> _deletedMsgs =
            new ConcurrentDictionary<ulong, ConcurrentQueue<IUserMessage>>();

        public void Add(ulong channelId, IUserMessage msg)
        {
            var channelMsgs = _deletedMsgs.GetOrAdd(channelId, x => new ConcurrentQueue<IUserMessage>());

            if (channelMsgs.Count == Config.MAX_DELETED_MSGS)
                channelMsgs.TryDequeue(out var old);

            channelMsgs.Enqueue(msg);
        }

        public IReadOnlyList<IUserMessage> GetLast(ulong channelId, int quantity)
        {
            if (!_deletedMsgs.TryGetValue(channelId, out var channelMsgs))
                return Enumerable.Empty<IUserMessage>().ToImmutableArray();

            return channelMsgs.OrderByDescending(x => x.Timestamp).Take(quantity).ToImmutableArray();
        }
    }
}
