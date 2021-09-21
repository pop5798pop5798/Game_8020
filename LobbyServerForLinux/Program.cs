using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using AspNetCoreChatRoom.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LobbyServerForLinux
{
    public class Program
    {
        public static List<PlayerData> playData = new List<PlayerData>();
        public static List<DataState> DataList = new List<DataState>();
        
        public static IConfiguration config;
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();                   
        }

       

        // �O���t�ΰT��.
        private static object _writeLot_lock = new object();
        public static void WriteLog(string msg) { WriteLog("GameLog", msg, false); }
        public static void WriteLog(string msg, bool showScreen) { WriteLog("GameLog", msg, showScreen); }
        public static void WriteLog(string fileName, string msg) { WriteLog(fileName, msg, false); }
        public static void WriteLog(string fileName, string msg, bool showScreen)
        {
            lock (_writeLot_lock)
            {
                string path = "";
                // �P�_�W��
                if (fileName == "TryCatch") path = @"Log/" + DateTime.Now.ToString("yyyy_MM_dd") + "/" + fileName + "/";
                else
                {
                    path = @"Log/" + DateTime.Now.ToString("yyyy_MM_dd") + "/" + DateTime.Now.Hour.ToString() + "/" + fileName + "/";
                }


                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                // �p�G�O�N��Log
                string file = "";
                file = path + fileName + "_" + DateTime.Now.Minute.ToString() + ".txt";

                StreamWriter w = File.AppendText(file);
                w.WriteLine(DateTime.Now.ToString("yyyy_MM_dd HH:mm:ss.fff") + " : " + msg);
                w.Close();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}