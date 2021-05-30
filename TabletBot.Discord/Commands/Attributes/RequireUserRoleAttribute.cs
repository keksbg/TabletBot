using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands.Attributes
{
    public class RequireUserRoleAttribute : PreconditionAttribute
    {
        public RequireUserRoleAttribute(params ulong[] roleIDs)
        {
            RoleIDs = roleIDs ?? Array.Empty<ulong>();
        }

        public ulong[] RoleIDs { get; }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is IGuildUser guildUser)
            {
                bool allowed = guildUser.GuildPermissions.Administrator || guildUser.RoleIds.Any(i => RoleIDs.Contains(i));
                return Task.FromResult(allowed ? PreconditionResult.FromSuccess() : PreconditionResult.FromError($"User does not have valid permissions to invoke '{Settings.Current.CommandPrefix}{command.Name}'."));
            }
            return Task.FromResult(PreconditionResult.FromError("Command must be invoked in a guild."));
        }
    }
}