using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Threading.Tasks;
using System.Timers;

namespace RemindMeDiscord
{
    internal class Program
    {
        #region Public Fields

        private static CommandsNextModule CommandsModule;

        // app settings
        public static NameValueCollection _appSettings;

        // Virtual Discord client
        public static DiscordClient Client;

        public static Timer SendReminderTimer;

        #endregion Public Fields

        #region Public Methods

        /// <summary> Main entry point for the application </summary>
        private static void Main(string[] args)
        {
            // pass entry to async code
            Program prog = new Program();
            prog.MainAsync(args).GetAwaiter().GetResult();
        }

        /// <summary> Async effective entry point for the application </summary>
        private async Task MainAsync(string[] args)
        {
            _appSettings = ConfigurationManager.AppSettings;

            try
            {
                // setup discord configuration and instantiate the client
                Client = new DiscordClient(new DiscordConfiguration
                {
                    Token = _appSettings["DiscordToken"],
                    TokenType = TokenType.Bot,
                    AutoReconnect = true,
                    LogLevel = LogLevel.Debug,
                    UseInternalLogHandler = true
                });

                // Hook into client events, so we know what's going on
                Client.Ready += Client_Ready;
                Client.ClientErrored += Client_ClientError;

                // Set up the ability to read custome commands prefixed with '!'
                CommandsModule = Client.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = "!",
                    EnableDms = true
                });

                // create event hooks for the commands
                CommandsModule.CommandExecuted += Commands_CommandExecuted;
                CommandsModule.CommandErrored += Commands_CommandErrored;
                CommandsModule.RegisterCommands<Commands>();
                CommandsModule.RegisterCommands<AdminCommands>();

                // Set up the timer that sends the reminders
                SendReminderTimer = new Timer();
                SendReminderTimer.Interval = int.Parse(_appSettings["BotSendMsgTickRate"]);
                SendReminderTimer.Elapsed += SendReminder.SendReminderTimer_Tick;

                //connect and log in
                await Client.ConnectAsync();

                //prevent premature quitting
                await Task.Delay(-1);
            }
            catch (Exception e)
            {
                Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMe", "Error initializing Discord client:" + e.Message, DateTime.Now);
            }
        }

        /// <summary> Returns an error message when the Client errors out for whatever reason </summary>
        public static Task Client_ClientError(ClientErrorEventArgs e)
        {
            // log the details of the error that just occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMe", $"Exception occured within Discord Client: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        /// <summary> Return a status when the client is ready </summary>
        public static Task Client_Ready(ReadyEventArgs e)
        {
            // log the fact that this event occured
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMe", "Client is ready to process events.", DateTime.Now);

            // since this method is not async, return a completed task
            return Task.CompletedTask;
        }

        /// <summary> Logs errors with intercepted commands </summary>
        public async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
        }

        /// <summary> Logs sucessfully intercepted commands </summary>
        public async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMe", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
        }

        #endregion Public Methods
    }
}