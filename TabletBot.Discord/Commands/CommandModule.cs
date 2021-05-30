using System.Threading.Tasks;
using Discord.Commands;
using TabletBot.Common;

namespace TabletBot.Discord.Commands
{
    public class CommandModule : ModuleBase
    {
        public const ulong BotRole = 615621006844887054;
        public const ulong ModeratorRole = 644180151755735060;
        public const ulong HelperRole = 723418791991705631;

        protected const string ITALIC_AFFIX = "*";
        protected const string BOLD_AFFIX = "**";
        protected const string UNDERLINE_AFFIX = "__";
        protected const string CODE_AFFIX = "`";

        protected const string QUOTE_PREFIX = "> ";
        protected const string CODE_BLOCK = "```";

        protected async Task OverwriteSettings()
        {
            Platform.SettingsFile.Refresh();
            if (Platform.SettingsFile.Exists)
                await Settings.Current.Write(Platform.SettingsFile);
        }
    }
}