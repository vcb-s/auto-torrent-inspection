﻿using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AutoTorrentInspection.Forms;
using AutoTorrentInspection.Logging.Handlers;
using Microsoft.Win32;
using AutoTorrentInspection.Util;
using Jil;

namespace AutoTorrentInspection
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.SetCompatibleTextRenderingDefault(false);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Logger.StoreLogMessages = true;
            Logger.LoggerHandlerManager.AddHandler(new DebugConsoleLoggerHandler());

            if (!File.Exists("config.json"))
            {
                Logger.Log("Extract default config file to current directory");
                File.WriteAllText("config.json", new Configuration().ToString());
            }
            else
            {
                try
                {
                    var updateConfigFile = false;
                    Configuration config, defaultConfig;
                    using (var input = new StreamReader("config.json"))
                    {
                        config = JSON.Deserialize<Configuration>(input);
                        defaultConfig = new Configuration();
                        if (defaultConfig.Version > config.Version)
                        {
                            updateConfigFile = true;
                        }
                    }
                    if (updateConfigFile)
                    {
                        File.WriteAllText("config.json", config.ToString());
                        Logger.Log($"Update the config file version from {config.Version}->{defaultConfig.Version}");
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    Notification.ShowError("无法读取配置文件", e);
                }
#if DEBUG
                File.WriteAllText("config.json", new Configuration().ToString());
                Logger.Log("Config file is now up to date");
#endif
            }
            if (args.Length == 0)
            {
                Application.Run(new Form1());
            }
            else
            {
                var argsFull = string.Join(" ", args);
                //argsFull = "\"" + argsFull + "\"";
                Application.Run(new Form1(argsFull));
            }
        }
    }
}
