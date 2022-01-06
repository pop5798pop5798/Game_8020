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
        // �C��ID.
        public const int GameID = 1020;
        public static int ServerID = 0;                 // ���A��ID
        public static int InnerChannel = 1;             // �������y��
        //���`
        public static int[] Ante = { 1, 10, 100 };          // ���`
        public static int[] Limit = { 10, 100, 1000 };   // �J�����B
        public static int[] GameArea = { 1, 2, 3 };
        public static int[] OpenState = { 1, 1, 1 };        // �}�񪬺A
        public static int[] Rate = { 95, 50, 85 };      // ���v
        public static int[] LoseRate = { 85, 90 };      // ��P���v
        public static int AvgRate = 50;      // �۵M���v
        public static int[] LoseLimit = { 5000, 10000, 10000 }; // �����`��ĵ�٭�
        private static decimal[] ObsWinLose = { 0, 0, 0 };      // �����`��Ĺ

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
