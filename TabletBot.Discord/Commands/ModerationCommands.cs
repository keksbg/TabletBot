using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands
{
    public class ModerationCommands : ModuleBase
    {
        [Command("delete", RunMode = RunMode.Async), Alias("del"), RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task DeleteMessage(int count = 1)
        {
            await Context.Message.DeleteAsync();
            var messages = await Context.Channel.GetMessagesAsync(count).FlattenAsync();
            await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
        }

        [Command("force-save", RunMode = RunMode.Async), RequireOwner]
        public async Task ForceSaveSettings()
        {
            await Context.Message.DeleteAsync();
            Settings.Current.Write(Platform.SettingsFile);
            await Log.WriteAsync("Settings", $"Owner force-saved the configuration to {Platform.SettingsFile.FullName}");
        }

        [Command("kill-bot", RunMode = RunMode.Async), RequireOwner]
        public async Task ForceKillBot()
        {
            await Context.Message.DeleteAsync();
            await Bot.Current.Logout();
            Environment.Exit(0x0);
        }
    }
}