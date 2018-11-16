using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace RemindMeDiscord
{
    internal class Program
    {
        #region Public Fields

        // app settings
        public static NameValueCollection appSettings;

        #endregion Public Fields

        #region Public Properties

        // Virtual Discord client
        public DiscordClient Client { get; set; }

        #endregion Public Properties

        #region Public Methods

        public static DiscordEmbed BuildHelpReply(string[] input)
        {
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();

            eb.AddField("Usage", "Type `!remindme \"time\" \"reminder title\"` to set a reminder.\n" +
                                 "Type `!remindme viewReminders` to see all of your current reminders.\n" +
                                 "Type `!remindme cancel reminderNumber` to cancel a reminder using the ID from `viewReminders`.");
            eb.AddField("Valid Times", "other text"); //TODO - finish filling out the help reply

            // return the block of text to be printed out by the bot in chat
            return eb.Build();
        }

        /// <summary> Parse a verified commen and process it back out </summary>
        /// <param name="input">  </param>
        /// <returns>  </returns>
        public static DiscordEmbed BuildReply(string[] input)
        {
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();

            DateTime reminderTime = ParseDateTime(input[1]); // TODO - parse out the time

            // TODO - save the reminder in database

            // TODO - PM the sender a confirmation

            // return the embed to be printed out by the bot in chat
            return eb.Build();
        }

        /// <summary> Main task that listens for the incoming messages </summary>
        public async Task RunBotAsync()
        {
            Contract.Ensures(Contract.Result<Task>() != null);
            try
            {
                // setup discord configuration
                DiscordConfiguration cfg = new DiscordConfiguration
                {
                    Token = appSettings["DiscordToken"],
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    LogLevel = LogLevel.Debug,
                    UseInternalLogHandler = true
                };

                // instantiate the client
                Client = new DiscordClient(cfg);

                // Hook into client events, so we know what's going on
                Client.Ready += Client_Ready;
                Client.ClientErrored += Client_ClientError;
                Client.MessageCreated += Client_MessageCreated;
            }
            catch (Exception e)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMe", "Error initializing Discord client:" + e.Message, DateTime.Now);
            }

            //connect and log in
            await Client.ConnectAsync();

            //prevent premature quitting
            await Task.Delay(-1);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary> Entry point for the application </summary>
        private static void Main(string[] args)
        {
            var prog = new Program();
            appSettings = ConfigurationManager.AppSettings;
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        /// <summary> Returns an error message when the Client errors out for whatever reason </summary>
        /// <param name="e">  </param>
        /// <returns>  </returns>
        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // log the details of the error that just occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMe", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        /// <summary> intercept messages and reply to them if applicable </summary>
        /// <param name="e">  </param>
        /// <returns>  </returns>
        private async Task Client_MessageCreated(MessageCreateEventArgs e)
        {
            // ignore bot comments
            if (e.Author.IsBot)
                return;

            // listener for messages
            try
            {
                string[] msg = e.Message.Content.ToLower().Split(' ');

                // only accept comments that contain a reminder command
                if (msg[0].Equals("!remindme"))
                {
                    // send the help embed if the first command is "help", if not create a normal reminder embed TODO - split this apart to look for
                    // other commands
                    DiscordEmbed reply = msg[1].Equals("help") ? BuildHelpReply(msg) : BuildReply(msg);

                    // failsafe null check to prevent bad data being printed into the discord chat.
                    if (reply != null)
                    {
                        // print the message as a bot comment
                        await e.Message.RespondAsync("", embed: reply);
                        Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMe", "Embed sent", DateTime.Now);
                    }
                }
            }
            catch (Exception ex)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMe", "Error with incoming message: " + ex.Message, DateTime.Now);
            }
        }

        /// <summary> Return a status when the client is ready </summary>
        /// <param name="e">  </param>
        /// <returns>  </returns>
        private Task Client_Ready(ReadyEventArgs e)
        {
            // log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMe", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        #endregion Private Methods
    }
}