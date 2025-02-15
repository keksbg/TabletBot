using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TabletBot.Common;
using TabletBot.Common.Store;
using TabletBot.Discord.Commands;
using static TabletBot.Discord.DiscordExtensions;

namespace TabletBot.Discord.Watchers.ReactionRoles
{
    public class RoleReactionWatcher : IReactionWatcher, IMessageWatcher
    {
        private readonly Settings _settings;
        private readonly DiscordSocketClient _discordSocketClient;

        public RoleReactionWatcher(Settings settings, DiscordSocketClient discordSocketClient)
        {
            _settings = settings;
            _discordSocketClient = discordSocketClient;
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionAdded(textChannel, reaction);
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var textChannel = await channel.GetOrDownloadAsync() as ITextChannel;
            await HandleReactionRemoved(textChannel, reaction);
        }

        private async Task HandleReactionAdded(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (IsTracked(reaction, _discordSocketClient, out var reactionRole))
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.AddRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed(_settings.DeleteDelay);
                Log.Exception(ex);
            }
        }

        private async Task HandleReactionRemoved(ITextChannel channel, SocketReaction reaction)
        {
            try
            {
                if (IsTracked(reaction, _discordSocketClient, out var reactionRole))
                {
                    var guild = await _discordSocketClient.Rest.GetGuildAsync(channel.GuildId);
                    var role = guild.Roles.FirstOrDefault(r => r.Id == reactionRole.RoleId);
                    var user = await guild.GetUserAsync(reaction.UserId);
                    await user.RemoveRoleAsync(role);
                }
            }
            catch (Exception ex)
            {
                var systemChannel = await channel.Guild.GetSystemChannelAsync();
                var reply = await ReplyException(systemChannel ?? channel, ex);
                reply.DeleteDelayed(_settings.DeleteDelay);
                Log.Exception(ex);
            }
        }

        public Task Receive(IMessage message) => Task.CompletedTask;

        public Task Deleted(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        {
            if (_settings.ReactiveRoles.FirstOrDefault(m => m.MessageId == message.Id) is RoleManagementMessageStore roleStore)
                _settings.ReactiveRoles.Remove(roleStore);

            return Task.CompletedTask;
        }

        public bool IsTracked(
            SocketReaction reaction,
            DiscordSocketClient client,
            out RoleManagementMessageStore reactionRole
        )
        {
            if (reaction.UserId == client.CurrentUser.Id)
            {
                reactionRole = default;
                return false;
            }

            var query = from reactRole in _settings.ReactiveRoles
                where reactRole.MessageId == reaction.MessageId
                where reactRole.EmoteName == reaction.Emote.ToString()
                select reactRole;
            reactionRole = query.FirstOrDefault();
            return reactionRole != null;
        }
    }
}