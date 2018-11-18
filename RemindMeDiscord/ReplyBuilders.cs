using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;

namespace RemindMeDiscord
{
    public class ReplyBuilders
    {
        #region Public Methods

        /// <summary> Sends a standard help message </summary>
        public static DiscordEmbed BuildHelpReply(IReadOnlyList<CommandArgument> msg)
        {
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();

            eb.AddField("Usage", "Type `!remindme \"time\" \"reminder text\"` to set a reminder.\n" +
                                 "Type `!remindme viewReminders` to see all of your current reminders.\n" +
                                 "Type `!remindme cancel reminderNumber` to cancel a reminder using the ID from `viewReminders`.");
            eb.AddField("Valid Times", "Tomorrow (default if no time specified) \n" +
                                       "Month/Day/Year\n" +
                                       "<Num> Hours/Minutes/Seconds" +
                                       "<Num> Days/Weeks/Years\n" +
                                       "Next DayOfWeek (assumes the intended date is not tomorrow or day after)" +
                                       "Next Month/Year");

            // return the block of text to be printed out by the bot in chat
            return eb.Build();
        }

        /// <summary> Parse a verified comment and process it back out </summary>
        public static DiscordEmbed BuildReply(string[] msg)
        {
            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
            DateTime reminderTime = ParseDateTime(msg);

            // check for a failure case, if a date was unable to be found then send null to the
            // caller to handle
            if (reminderTime.Year == 9999)
            {
                return null;
            }

            // TODO - save the reminder in database

            // TODO - PM the sender a confirmation

            // return the embed to be printed out by the bot in chat
            return eb.Build();
        }

        /// <summary> Shows a list of all pending reminders for a user </summary>
        public static DiscordEmbed BuildViewRemindersReply()
        {
            try
            {
                // TODO - get message from database

                // TODO - build a reply
                DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
                eb.AddField("Hi", "not done yet");

                // TODO - PM the original user with the reminder

                Program.Client.DebugLogger.LogMessage(LogLevel.Warning, "RemindMeDiscord.ReplyBuilder", "BuildViewRemindersReply not yet implemented", DateTime.Now);

                return eb;
            }
            catch (Exception ex)
            {
                Program.Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMeDiscord.ReplyBuilder", $"Exception viewing reminders: {ex.Message}", DateTime.Now);
                return null;
            }
        }

        /// <summary> Gets the next weekday after a given day of the week, excluding tomorrow </summary>
        public static DateTime GetNextWeekday(DayOfWeek day)
        {
            // Add 1 day to the start date. ex. if it is Saturday, and you say "next Sunday", it
            // assumes you mean the following Sunday, as people would usually say "tomorrow"
            DateTime result = DateTime.UtcNow.AddDays(1);

            // Keep advancing the day until it maches the intended result
            while (result.DayOfWeek != day)
                result = result.AddDays(1);
            return result;
        }

        /// <summary> Looks through the input string for various date formats or date commands </summary>
        public static DateTime ParseDateTime(string[] msg)
        {
            // initialize to an invalid date. In success cases, this is overwritten. In failure
            // cases, this is left alone. If returned to the caller it will be handled and the user
            // sent an error message
            DateTime returnDate = new DateTime(9999, 1, 1);

            // process "next" times
            if (msg[1].Equals("next", StringComparison.CurrentCultureIgnoreCase))
            {
                string qualifier = msg[2].ToLowerInvariant();
                switch (qualifier)
                {
                    case "sunday":
                        return returnDate = GetNextWeekday(DayOfWeek.Sunday);
                    case "monday":
                        return returnDate = GetNextWeekday(DayOfWeek.Monday);
                    case "tuesday":
                        return returnDate = GetNextWeekday(DayOfWeek.Tuesday);
                    case "wednesday":
                        return returnDate = GetNextWeekday(DayOfWeek.Wednesday);
                    case "thursday":
                        return returnDate = GetNextWeekday(DayOfWeek.Thursday);
                    case "friday":
                        return returnDate = GetNextWeekday(DayOfWeek.Friday);
                    case "saturday":
                        return returnDate = GetNextWeekday(DayOfWeek.Saturday);
                    case "week":
                        return returnDate = DateTime.UtcNow.AddDays(7);
                    case "month":
                        return returnDate = DateTime.UtcNow.AddMonths(1);
                    case "year":
                        return returnDate = DateTime.UtcNow.AddYears(1);
                }
            }
            else if (msg[1].Equals("tomorrow", StringComparison.CurrentCultureIgnoreCase))
            {
                return returnDate = DateTime.UtcNow.AddDays(1);
            }

            //parse specified days/months/years increments
            try
            {
                int num = int.Parse(msg[1]);
                string qualifier = msg[2].ToLowerInvariant();
                switch (qualifier)
                {
                    case "second":
                    case "seconds":
                        return returnDate = DateTime.UtcNow.AddSeconds(num);
                    case "minute":
                    case "minutes":
                        return returnDate = DateTime.UtcNow.AddMinutes(num);
                    case "hour":
                    case "hours":
                        return returnDate = DateTime.UtcNow.AddHours(num);
                    case "week":
                    case "weeks":
                        return returnDate = DateTime.UtcNow.AddDays(num * 7);
                    case "month":
                    case "months":
                        return returnDate = DateTime.UtcNow.AddMonths(num);
                    case "year":
                    case "years":
                        return returnDate = DateTime.UtcNow.AddYears(num);
                }
            }
            catch (Exception ex)
            {
                // ignore and continue trying to parse for a DateTime
                Program.Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMeDiscord", $"Error trying to parse message for specific time: {ex.Message}", DateTime.Now);
            }

            // parse for a given date
            try
            {
                returnDate = DateTime.Parse(msg[1]);
                returnDate = returnDate.ToUniversalTime();
            }
            catch (FormatException ex)
            {
                // if the program gets to here, it has failed to recognize any of the previous time
                // patterns, meaning none was given. Therefore we default to tomorrow.
                Program.Client.DebugLogger.LogMessage(LogLevel.Info, "RemindMeDiscord", $"No time specified for reminder, defaulting to tomorrow: {ex.Message}", DateTime.Now);
                returnDate = DateTime.UtcNow.AddDays(1);
            }
            catch (Exception ex)
            {
                // at this point, something wrong happened. Return a totally invalid special date to
                // be handled by the caller.
                Program.Client.DebugLogger.LogMessage(LogLevel.Error, "RemindMeDiscord", $"Error trying to parse message for time: {ex.Message}", DateTime.Now);
            }

            return returnDate;
        }

        #endregion Public Methods
    }
}