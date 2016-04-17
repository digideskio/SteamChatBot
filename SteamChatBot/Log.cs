﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SteamChatBot
{
    public class Log : IDisposable
    {

        private static Log instance;
        private string logFile;
        private string botName;
        private LogLevel consoleLogLevel;
        private LogLevel fileLogLevel;
        private static LoggerWindow window;

        public static Log Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException("Call Log.CreateInstance to create the instance");
                }
                else
                {
                    return instance;
                }
            }
        }

        /*
        private Log(string logFile, string botName, LogLevel consoleLogLevel, LogLevel fileLogLevel)
        {
            this.logFile = logFile;
            this.botName = botName;
            this.consoleLogLevel = consoleLogLevel;
            this.fileLogLevel = fileLogLevel;
        }
        */

        public static Log CreateInstance(string logFile, string botName, LogLevel consoleLogLevel, LogLevel fileLogLevel, LoggerWindow _window)
        {
            window = _window;
            return instance ?? (instance = new Log(logFile, botName, consoleLogLevel, fileLogLevel));
        }

        public enum LogLevel
        {
            Silly,
            Debug,
            Verbose,
            Info,
            Warn,
            Error
        }


        protected StreamWriter _FileStream;
        protected string _botName;
        private bool disposed;
        private static string path;
        public LogLevel OutputLevel;
        public LogLevel FileLogLevel;
        public ConsoleColor DefaultConsoleColor = ConsoleColor.White;
        public bool ShowBotName { get; set; }

        private Log(string logFile, string botName = "", LogLevel consoleLogLevel = LogLevel.Info, LogLevel fileLogLevel = LogLevel.Info)
        {
            this.logFile = logFile;
            this.botName = botName;
            this.consoleLogLevel = consoleLogLevel;
            this.fileLogLevel = fileLogLevel;

            path = Path.Combine(Bot.username + "/logs", logFile);
            Directory.CreateDirectory(Bot.username + "/logs");
            _botName = botName;
            OutputLevel = consoleLogLevel;
            FileLogLevel = fileLogLevel;
            ShowBotName = true;
        }

        ~Log()
        {
            Dispose(false);
        }

        // This outputs a log entry of the level info.
        public void Info(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Info, data, formatParams);
        }

        // This outputs a log entry of the level debug.
        public void Debug(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Debug, data, formatParams);
        }

        // This outputs a log entry of the level success.
        public void Silly(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Silly, data, formatParams);
        }

        // This outputs a log entry of the level warn.
        public void Warn(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Warn, data, formatParams);
        }

        // This outputs a log entry of the level error.
        public void Error(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Error, data, formatParams);
        }

        public void Verbose(string data, params object[] formatParams)
        {
            _OutputLine(LogLevel.Verbose, data, formatParams);
        }

        // Outputs a line to both the log and the console, if
        // applicable.
        protected void _OutputLine(LogLevel level, string line, params object[] formatParams)
        {
            if (disposed)
            {
                return;
            }
            string formattedString = string.Format(
                "[{0}{1}] {2}: {3}",
                GetLogBotName(),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                _LogLevel(level).ToUpper(), (formatParams != null && formatParams.Any() ? string.Format(line, formatParams) : line)
                );

            if (level >= FileLogLevel)
            {
                //_FileStream.WriteLine(formattedString);
                Write(formattedString);
            }
            if (level >= OutputLevel)
            {
                _OutputLineToConsole(level, formattedString);
            }
        }

        public static void Write(string data)
        {
            using (var sw = File.AppendText(path))
            {
                sw.WriteLine(data);
            }
        }

        private string GetLogBotName()
        {
            if (_botName == null)
            {
                return "(SYSTEM) ";
            }
            else if (ShowBotName)
            {
                return _botName + " ";
            }
            return "";
        }

        // Outputs a line to the console, with the correct color
        // formatting.
        protected void _OutputLineToConsole(LogLevel level, string line)
        {
            ListBox lb = window.loggerBox;
            ListBoxItem itemToAdd = new ListBoxItem();
            itemToAdd.Content = line;
            lb.Items.Add(itemToAdd);
            ListBoxItem justAddedItem = (ListBoxItem)lb.Items[lb.Items.Count - 1];
            justAddedItem.Foreground = _LogColor(level);

        }

        // Determine the string equivalent of the LogLevel.
        protected string _LogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info:
                    return "info";
                case LogLevel.Debug:
                    return "debug";
                case LogLevel.Silly:
                    return "silly";
                case LogLevel.Warn:
                    return "warn";
                case LogLevel.Error:
                    return "error";
                case LogLevel.Verbose:
                    return "verbose";
                default:
                    return "undef";
            }
        }

        // Determine the color to be used when outputting to the
        // console.
        protected Brush _LogColor(LogLevel level)
        { 
            switch (level)
            {
                case LogLevel.Info:
                    return Brushes.Green;
                case LogLevel.Debug:
                    return Brushes.Blue;
                case LogLevel.Silly:
                    return Brushes.Magenta;
                case LogLevel.Warn:
                    return Brushes.Yellow;
                case LogLevel.Error:
                    return Brushes.Red;
                case LogLevel.Verbose:
                    return Brushes.Cyan;
                default:
                    return Brushes.White;
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
                _FileStream.Dispose();
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
