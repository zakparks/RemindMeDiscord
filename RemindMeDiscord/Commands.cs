using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemindMeDiscord
{
    [Group("admin")]
    [Description("Administrative commands.")]
    [Hidden]
    [RequirePermissions(Permissions.ManageGuild)]
    public class AdminCommands
    {
        // TODO implement admin console to interact with the database
    }

    public class Commands
    {
        #region Public Methods

        /// <summary> Main reminder command </summary>
        [Command("remindme")]
        public async Task RemindMe(CommandContext ctx, string arg)
        {
            IReadOnlyList<CommandArgument> msg = ctx.Command.Arguments;
            DiscordEmbed reply;

            // send the help embed if the first command is "help", if not create a normal reminder embed
            if (arg.Equals("help", StringComparison.CurrentCultureIgnoreCase))
            {
                reply = ReplyBuilders.BuildHelpReply(msg);
            }
            else if (arg.Equals("viewreminders", StringComparison.CurrentCultureIgnoreCase))
            {
                reply = ReplyBuilders.BuildViewRemindersReply();
            }
            else
            {
                string[] commandArguments = ctx.Message.ToString().Split();
                reply = ReplyBuilders.BuildReply(commandArguments);
            }

            // failsafe null check to prevent bad data being printed into the discord chat.
            if (reply != null)
            {
                // print the message as a bot comment
                await ctx.RespondAsync("", embed: reply);
                Program.Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMe", "Embed sent", DateTime.Now);
            }
            else
            {
                // something happened making the embed, show the user an error
                await ctx.RespondAsync("Unable to process that message, use `!remindme help` to see how to set a reminder.");
            }
        }

        #endregion Public Methods
    }
}