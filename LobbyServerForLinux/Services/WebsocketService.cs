using AspNetCoreChatRoom.Model;
using LobbyServerForLinux.Model;
using LobbyServerForLinux.Model.DB;
using LobbyServerForLinux.Models;
using LobbyServerForLinux.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using P2PCore.Models;
using RubyApi.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using WebPostFile.Helpers;

namespace LobbyServerForLinux
{
    /// <summary> WebSocket連線主服務 </summary>
    public class WebsocketService
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private static P2PContext _db = new P2PContext();

        private static DateTime m_ProgressTime = DateTime.Now;
        private static DateTime m_IPTime = DateTime.Now;
        private static DateTime m_SavaTime = DateTime.Now;

        private static List<WaitFiles> waitList = new List<WaitFiles>();
        private static List<download_record> waitRecord = new List<download_record>();

        private static List<PlayerData> SearchList = new List<PlayerData>();
        private static List<PlayerData> UseDataList = new List<PlayerData>();
        private static List<PlayerData> base64List = new List<PlayerData>();
        

        private static List<Player> AllPaleyr = new List<Player>();

        private static List<Player> GetProgress = new List<Player>();

        private static List<FileCount> fileCount = new List<FileCount>();

        //lock
        public static object _fDataLock = new object();

        private readonly IConfiguration _config;
        private static System.Timers.Timer aTimer; //計時器
        private static IDatabase redisData = RedisConnectorHelper.Connection.GetDatabase();
        




        public WebsocketService(RequestDelegate next,ILoggerFactory loggerFactory, IConfiguration config)        
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<WebsocketService>();
            _config = config;
            Program.config = _config;
            //SetTimer();
            //啟動計時器
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(200);
            // Hook up the Elapsed event for the timer.            
            aTimer.Elapsed += async (sender, e) => await OnTimedEventAsync();
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        /// <summary> 調用webScoket </summary>
        /// <param name="context"> 傳入資料 </param>
        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                string clientId = Guid.NewGuid().ToString(); ;
                var wsClient = new WebsocketClient
                {
                    Id = clientId,
                    WebSocket = webSocket
                };
                try
                {
                    await Handle(wsClient);
                }
                catch (Exception ex)
                {
                    await context.Response.WriteAsync("closed");
                }
            }
            else
            {
                await _next(context);
                //context.Response.StatusCode = 404;
            }


        }
        /// <summary> 交握處理 </summary>
        /// <param name="webSocket"> Scoket資料 </param>
        private async Task Handle(WebsocketClient webSocket)
        {
            WsCollectionService.Add(webSocket);

            WebSocketReceiveResult result = null;
            var response = "";
            do
            {
                //var buffer = new byte[8192];
                //result = await webSocket.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                response = await ReceiveStringAsync(webSocket.WebSocket);
                if (!string.IsNullOrEmpty(response))
                {
                    //var msgString = Encoding.UTF8.GetString(buffer);
                    var message = JsonConvert.DeserializeObject<Message>(response);
                    message.Sn = webSocket.Id;
                    await MessageRouteAsync(message);                   
                }
            }
            while (!string.IsNullOrEmpty(response));           
            WsCollectionService.Remove(webSocket);
            await DataClose(webSocket.Id);
            //_logger.LogInformation($"Websocket client closed.");//離線
        }
        /// <summary> 資料Rout </summary>
        /// <param name="message"> 回傳資料訊息 </param>
        private async Task MessageRouteAsync(Message message)
        {
            var client = WsCollectionService.Get(message.Sn);
            if (client == null)
            {
                return;
            }
            
            switch (message.Cmd)
            {
                //case "SystemCheck"://取得是否連線
                //    cmdSystemCheck(message.Sn);
                //    break;
                case "GetMore":
                    GetMore(message.Sn, message.Data.ToString());
                    break;
                case "SetBase64":
                    SetBase64(message.Sn, message.Data.ToString());
                    break;
                case "GetPing":
                    await GetPing(client);
                    break;
                case "GetFileState"://取得全部存在檔案的狀態
                    GetFilesState(client);
                    break;
                case "DataState"://取得檔案的狀態
                    GetDataState(message.Sn, client, message.Data.ToString());
                    break;

                case "Search"://取得搜尋的檔案資訊
                    SearchAsync(message.Sn,client, message.Data.ToString());
                    break;
                case "SetData"://設定使用者分享檔案
                    await SetPlayerAsync(message.Data.ToString(), client);
                    break;
                case "GetDetail"://發送下載請求給檔案持有者
                    SendDetailAsync(message.Sn, message.Data.ToString());

                    break;
                case "GetData": //取得自已的全部分享檔案
                    SendStringAsync(message.Sn,client, message.Data.ToString());
                    break;
                case "GetDataDetail": //取得分享連結的檔案
                    GetDataDetailAsync(message.Sn, client, message.Data.ToString());
                    break;
                case "PostData": //取得檔案並上傳(回傳和發送)
                    try
                    {
                        await SendPostDataAsync(message.Data.ToString());
                    }
                    catch (Exception ex)
                    {
                        Program.WriteLog("TryCatch", "Err: " + ex);
                    }

                    break;
                default:
                    break;
            }
        }

        private void cmdSystemCheck(string Sn) {
            //var cache = RedisConnectorHelper.Connection.GetDatabase();
            //var model = JsonConvert.DeserializeObject<downloadModel>(cache.StringGet("Test"));

            /*Player p = new Player();
            p.userKey = Sn;
            var gd = AllPaleyr.Where(x => x.userKey == Sn).FirstOrDefault();
            if (gd != null)
            {
                for(int i = 0;i < AllPaleyr.Count;i++)
                {
                    if(AllPaleyr[i].userKey == Sn)
                    {
                        AllPaleyr[i].SrvPingTime = (int)DateTime.Now.Subtract(gd.SystemCheckTime).TotalSeconds;
                        AllPaleyr[i].SystemCheckTime = DateTime.Now;
                        break;
                    }
                }
            }
            else 
            {
                p.SrvPingTime = (int)DateTime.Now.Subtract(DateTime.Now).TotalSeconds;
                p.SystemCheckTime = DateTime.Now;
                AllPaleyr.Add(p);
            }*/
            
        }

        private static async Task GetPing(WebsocketClient client)
        {
            CmdData info = new CmdData();
            info.Cmd = "GetPing";
            info.Data = "";
            string _data = JsonConvert.SerializeObject(info);
            await client.SendMessageAsync(_data);
        }

        //斷線清除
        private static async Task DataClose(string sn) 
        {
            try {
                int dataCount = Program.playData.Count;
                int FileCount = Program.DataList.Count;


                Program.playData = Program.playData.Where(x => x.userKey != sn).ToList();

                for (int i = 0; i < FileCount; i++)
                {
                    if (Program.DataList[i].userKey == sn)
                    {
                        try
                        {
                            using (P2PContext _db = new P2PContext())
                            {
                                var _dlrList = _db.download_record.Where(x => x.uID == Program.DataList[i].userID).ToList();
                                if (_dlrList.Count() != 0)
                                {
                                    _dlrList.ForEach(a => a.state = 0);
                                    await _db.SaveChangesAsync();
                                    //_db.SaveChanges();
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Program.WriteLog("TryCatch", "Err: " + ex);
                        }
                        lock (_fDataLock)
                        {
                            Program.DataList.Remove(Program.DataList[i]);
                        }



                        break;
                    }
                }

            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
            




        }

        //取得更多
        private static void GetMore(string Sn, string data)
        {
            try
            {
                SearchData json = JsonConvert.DeserializeObject<SearchData>(data);
                //List<PlayerData> SearchList = new List<PlayerData>();
                List<PlayerData> MoreList = new List<PlayerData>();
                string[] subs = json.keyWord.Split();
                int count = 0;
                //Program.WriteLog("PlayerData", JsonConvert.SerializeObject(Program.playData));
                //List<PlayerData> searchData = Program.playData.ToList();
                
                for (int i = 0; i < Program.playData.Count(); i++)
                {
                    PlayerData player = new PlayerData();
                    if (Program.playData[i] != null)
                    {
                        List<DomData> dom = new List<DomData>();
                        foreach (var sub in subs)
                        {
                            var toUptxt = sub.ToUpper();
                           
                            foreach(var item in Program.playData[i].domData)
                            {
                                if (item.name == json.keyWord) continue;
                                //DomData d = new DomData();
                                if (item.isOpen == 2)
                                {
                                    if(item.name.ToUpper().Contains(toUptxt) || item.ext.ToUpper().Contains(toUptxt))
                                    {
                                        if (dom.Any(x => x.name == item.name)) continue;
                                        dom.Add(item);
                                    }
                                }
                            }
                        }
                        player.sendSn = Sn;
                        player.userKey = Program.playData[i].userKey;
                        player.userID = Program.playData[i].userID;
                        player.domData = dom;
                        count += dom.Count;
                        MoreList.Add(player);

                    }
                }



                CmdData info = new CmdData();
                info.Cmd = "GetMore";
                info.Data = MoreList;

                string _data = JsonConvert.SerializeObject(info);
                var client = WsCollectionService.Get(Sn);
                client.SendMessageAsync(_data);

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }







        }


        //設定base64圖
        private static void SetBase64(string Sn,string data) 
        {
            try {
                SetBase64 json = JsonConvert.DeserializeObject<SetBase64>(data);
                for (int i = 0; i < Program.playData.Count; i++)
                {
                    if (Program.playData[i].userKey == Sn)
                    {
                        for (int j = 0; j < Program.playData[i].domData.Count; j++)
                        {
                            if (Program.playData[i].domData[j].name == json.name)
                            {
                                Program.playData[i].domData[j].base64 = json.base64;
                                break;
                            }
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }







        }



        //取得全部存在檔案的狀態
        private static void GetFilesState(WebsocketClient client)
        {
            try {
                CmdData info = new CmdData();
                info.Cmd = "GetFileState";
                info.Data = Program.DataList;

                string _data = JsonConvert.SerializeObject(info);

                client.SendMessageAsync(_data);
                Player p = new Player();
                p.userKey = client.Id;
                GetProgress.Add(p);
            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }


        }

        //取得檔案狀態
        private static void GetDataState(string Sn,WebsocketClient client, string data)
        {
            try {
                SData json = JsonConvert.DeserializeObject<SData>(data);
                var ds = Program.DataList.Where(x => x.userID == json.uID).ToList();
                Player p = new Player();
                p.userKey = client.Id;
                p.fname = json.fileName;
                p.uSn = MD5Hash(json.username);
                GetProgress.Add(p);
                SData sdata = new SData();
                foreach (var _ds in ds)
                {
                    var _sData = _ds.sData.Where(x => x.fSn == json.fSn).FirstOrDefault();
                    if (_sData != null)
                    {
                        sdata = _ds.sData.Where(x => x.fSn == json.fSn).FirstOrDefault();
                        break;
                    }

                }

                //bool isgoWait = waitList.Any(x => x.fSn == json.fSn && x.sn == Sn);
                //if (isgoWait) return;
                WaitFiles wait = new WaitFiles();
                wait.sn = Sn;
                wait.fSn = json.fSn;
                wait.hloderID = json.uID;
                waitList.Add(wait);



                CmdData info = new CmdData();
                info.Cmd = "DataState";
                info.Data = sdata;

                string _data = JsonConvert.SerializeObject(info);
                client.SendMessageAsync(_data);

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }




        }

        //發送下載請求給檔案持有者
        private static void SendDetailAsync(string cSn, string data)
        {
            DataDetail detail = JsonConvert.DeserializeObject<DataDetail>(data);

            //錯誤封包設置
            CmdData errorinfo = new CmdData();
            errorinfo.Cmd = "ShareStop";
            errorinfo.Data = "";
            string _data2 = JsonConvert.SerializeObject(errorinfo);
            var client2 = WsCollectionService.Get(cSn);
            //--------------------------------------
            try
            {
                DomData _info = null;
                var _userList = Program.playData.Where(x => x.userID == detail.uID).ToList();
                foreach(var item in _userList)
                {
                    foreach(var dom in item.domData)
                    {
                        if(dom.fSn == detail.fSn)
                        {
                            _info = dom;
                        }
                    }
                }                  
                if (_info == null) return;
                DataDetail dd = new DataDetail();
                dd.FileName = detail.FileName;
                dd.md5Sn = MD5Hash(detail.user);
                dd.fSn = detail.fSn;
                dd.uID = detail.uID;
                dd.size = _info.sizeToInt;
                dd.currentUID = detail.currentUID;

                var ds = Program.DataList.Where(x => x.userID == detail.uID).ToList();
                
                //SData sdata = new SData();
                foreach (var _ds in ds)
                {
                    for (int j = 0; j < _ds.sData.Count(); j++)
                    {
                        if (_ds.sData[j].fSn == detail.fSn)
                        {
                            if (_ds.sData[j].state == 1) return;//下載中跳出
                            if (_ds.sData[j].state == 3) { }
                            else
                            {
                                _ds.sData[j].state = 1;                               
                            }
                            
                            break;
                        }
                    }

                }
               

                CmdData info = new CmdData();
                info.Cmd = "GetDetail";
                info.Data = dd;

                string _data = JsonConvert.SerializeObject(info);
                var buffer = Encoding.UTF8.GetBytes(_data);
                var segment = new ArraySegment<byte>(buffer);

                string sSn = "";
                foreach(var item in Program.playData)
                {
                    foreach(var dom in item.domData)
                    {
                        if(dom.fSn == dd.fSn)
                        {
                            sSn = item.userKey;
                            break;
                        }
                    }
                }

                var client = WsCollectionService.Get(sSn);
                if (client == null)
                {
                    client2.SendMessageAsync(_data2);
                }
                else 
                {
                    client.SendMessageAsync(_data);
                }
                
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        //取得檔案並上傳(回傳)
        private async Task SendPostDataAsync(string data)
        {
            try
            {
                PostData postData = JsonConvert.DeserializeObject<PostData>(data);
                             
                try
                {
                    using (P2PContext _db = new P2PContext())
                    {
                        var _dlr = _db.download_record.Where(x => x.fSn == postData.fSn).FirstOrDefault();
                        var _info = Program.playData.Where(x => x.userID == postData.uID).FirstOrDefault().domData.Where(x => x.fSn == postData.fSn).FirstOrDefault();
                        if (_info == null) return;
                        var cache = RedisConnectorHelper.Connection.GetDatabase();
                        string fileSn = MD5Hash(postData.userToMd5 + postData.fileName);
                        try
                        {
                            var redisdata = cache.StringGet(fileSn);
                            var model = JsonConvert.DeserializeObject<downloadModel>(redisdata);
                            if (model.progress != _info.sizeToInt) return;
                        }
                        catch (Exception ex)
                        {
                            Program.WriteLog("TryCatch", "Err: " + ex);
                            return;
                        }
                        string defURL = postData.Url;
                        var fileExt = Path.GetExtension(postData.fileName);

                        if (fileExt == ".kjv")
                        {
                            try
                            {
                                HttpWebRequest oHttp_Web_Req = (HttpWebRequest)WebRequest.Create(postData.Url);
                                Stream oStream = oHttp_Web_Req.GetResponse().GetResponseStream();
                                using (StreamReader respStreamReader = new StreamReader(oStream, Encoding.UTF8))
                                {
                                    string line = string.Empty;
                                    while ((line = respStreamReader.ReadLine()) != null)
                                    {

                                        UTF8Encoding utf8 = new UTF8Encoding(false);
                                        defURL = line;
                                        postData.Url = line;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                Program.WriteLog("TryCatch", "Err: " + ex);
                            }
                        }
                        if (_dlr == null)
                        {
                            bool isExist = waitRecord.Any(x=>x.fSn == postData.fSn && x.uID == _info.uID);
                            
                            if (!isExist)
                            {
                                download_record dlr = new download_record();
                                dlr.fSn = postData.fSn;
                                
                                dlr.url = defURL;
                                dlr.date = DateTime.Now;
                                dlr.state = 1; //在線
                                dlr.uID = _info.uID;



                                waitRecord.Add(dlr);
                                //_db.Add(dlr);
                                //_db.SaveChangesAsync();
                                //_db.SaveChanges();
                            }
                            
                        }
                        else 
                        {
                            _dlr.url = defURL;
                            _db.download_record.Update(_dlr);
                            _db.Entry(_dlr).State = EntityState.Modified;
                            await _db.SaveChangesAsync();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Program.WriteLog("TryCatch", "Err: " + ex);
                }



                var ds = Program.DataList.Where(x => x.userID == postData.uID).ToList();
                SData sdata = new SData();
                foreach (var _ds in ds)
                {
                    for (int j = 0; j < _ds.sData.Count(); j++)
                    {
                        if (_ds.sData[j].fSn == postData.fSn)
                        {
                            if (_ds.sData[j].state == 1) _ds.sData[j].state = 3;
                            break;
                        }
                    }

                }




                CmdData info = new CmdData();
                info.Cmd = "PostData";
                info.Data = postData;
                string _data = JsonConvert.SerializeObject(info);
                //發送給等待者
                for (int i = 0;i < waitList.Count;i++)
                {
                    if (waitList[i].fSn == postData.fSn && waitList[i].hloderID == postData.uID)
                    {
                        //string sn = waitList[i].sn;                       
                        var client = WsCollectionService.Get(waitList[i].sn);
                        if (client != null)
                        {
                            await client.SendMessageAsync(_data);
                        }
                    }
                }

                //清除等待
                for (int i = 0; i < waitList.Count; i++)
                {
                    if (waitList[i].fSn == postData.fSn && waitList[i].hloderID == postData.uID)
                    {
                        waitList.Remove(waitList[i]);
                    }
                }




            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }

        }

        //取得單一使用者全部分享檔案(分享連結)
        private static void GetDataDetailAsync(string Sn,WebsocketClient client, string data)
        {
            try
            {
                GetData detail = JsonConvert.DeserializeObject<GetData>(data);
                CmdData info = new CmdData();
                //info.Cmd = "GetData";
                var pd = new List<PlayerData>();
                if (detail.sn != 0)
                {
                    pd = Program.playData.Where(x => x.domData.FirstOrDefault().isOpen != 0 && x.userID == detail.sn).ToList();
                }
                info.Data = pd;
                int count = 0;
                for (int i = 0; i < pd.Count; i++)
                {
                    PlayerData player = new PlayerData();
                    player.sendSn = Sn;
                    player.userKey = pd[i].userKey;
                    player.userID = pd[i].userID;
                    player.domData = pd[i].domData;
                    count += pd[i].domData.Count;

                    UseDataList.Add(player);
                }
                if (count != 0) return;
                info = new CmdData();
                info.Cmd = "GetData";
                info.Data = new List<PlayerData>();

                var _data = JsonConvert.SerializeObject(info);
                client.SendMessageAsync(_data);

                //string _data = JsonConvert.SerializeObject(info);
                //client.SendMessageAsync(_data);
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }

        }

        //取得自已的全部分享檔案
        private static void SendStringAsync(string Sn,WebsocketClient client, string data)
        {
            try
            {
                bool isExist = UseDataList.Any(x => x.sendSn == Sn);
                if (isExist) return;
                GetData detail = JsonConvert.DeserializeObject<GetData>(data);
                //CmdData info = new CmdData();
                //info.Cmd = "GetData";
                List<PlayerData> pd = new List<PlayerData>();
                if (detail.sn != 0)
                {
                    pd = Program.playData.Where(x => x.userID == detail.sn).ToList();
                }
                //info.Data = pd;
                int count = 0;
                for(int i = 0;i < pd.Count;i++)
                {
                    PlayerData player = new PlayerData();
                    player.sendSn = Sn;
                    player.userKey = pd[i].userKey;
                    player.userID = pd[i].userID;
                    player.domData = pd[i].domData;
                    count += pd[i].domData.Count;

                    UseDataList.Add(player);
                }
                if (count != 0) return;
                var info = new CmdData();
                info.Cmd = "GetData";
                info.Data = new List<PlayerData>();

                var _data = JsonConvert.SerializeObject(info);
                client.SendMessageAsync(_data);

                //string _data = JsonConvert.SerializeObject(info);
                //client.SendMessageAsync(_data);
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
        }




        //取得搜尋的檔案資訊
        private static void SearchAsync(string Sn,WebsocketClient client, string data)
        {
            try
            {
                bool isExist = SearchList.Any(x => x.sendSn == Sn);
                if (isExist) return;

                SearchData json = JsonConvert.DeserializeObject<SearchData>(data);
                //List<PlayerData> SearchList = new List<PlayerData>();
                string[] subs = json.keyWord.Split();
                int count = 0;
                //Program.WriteLog("PlayerData", JsonConvert.SerializeObject(Program.playData));
                List<PlayerData> searchData = Program.playData.ToList();
                for (int i = 0; i < searchData.Count(); i++)
                {
                    PlayerData player = new PlayerData();
                    if (searchData[i] != null)
                    {
                        List<DomData> dom = new List<DomData>();
                        foreach (var sub in subs)
                        {
                            var toUptxt = sub.ToUpper();
                            List<DomData> d = searchData[i].domData
                                .Where(x => x.isOpen == 2)
                                .Where(e => e.name.ToUpper().Contains(toUptxt) || e.ext.ToUpper().Contains(toUptxt)).ToList();

                            if (d != null)
                            {
                                dom.AddRange(d);
                            }
                        }
                        player.sendSn = Sn;
                        player.userKey = searchData[i].userKey;
                        player.userID = searchData[i].userID;
                        player.domData = dom;
                        count += dom.Count;
                        SearchList.Add(player);

                    }
                }

                //傳數量
                CmdData info = new CmdData();
                info.Cmd = "DataCount";
                info.Data = count;
                string _data = JsonConvert.SerializeObject(info);
                client.SendMessageAsync(_data);

                if (count != 0) return;
                info = new CmdData();
                info.Cmd = "GetData";
                info.Data = new List<PlayerData>();

                _data = JsonConvert.SerializeObject(info);
                client.SendMessageAsync(_data);
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }

        }

        //設定使用者分享檔案
        private async Task SetPlayerAsync(string data, WebsocketClient client)
        {


            CmdData cmd = new CmdData();
            cmd.Cmd = "UploadUrl";
            cmd.Data = _config.GetValue<string>("Upload");
            string _data = JsonConvert.SerializeObject(cmd);
            try
            {
                var _c = WsCollectionService.Get(client.Id);
                if (_c != null) await _c.SendMessageAsync(_data);
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }


            try
            {
                List<DomData> json = JsonConvert.DeserializeObject<List<DomData>>(data);
                if (json.Count == 0) return;
                var ReDat = Program.playData.Where(x => x.userID == json.FirstOrDefault().uID && x.userKey != client.Id).FirstOrDefault();
                if (ReDat != null)
                {
                    cmd.Cmd = "ReData";
                    cmd.Data = "";
                    _data = JsonConvert.SerializeObject(cmd);
                    if (json.FirstOrDefault().isChange)
                    {
                        try
                        {
                            Program.playData = Program.playData.Where(x => x.userKey != ReDat.userKey).ToList();
                            var _c = WsCollectionService.Get(ReDat.userKey);
                            if (_c != null) await _c.SendMessageAsync(_data);

                        }
                        catch (Exception ex)
                        {
                            Program.WriteLog("TryCatch", "Err: " + ex);
                        }
                    }
                    else
                    {
                        try
                        {
                            var _c = WsCollectionService.Get(client.Id);
                            if (_c != null) await _c.SendMessageAsync(_data);

                        }
                        catch (Exception ex)
                        {
                            Program.WriteLog("TryCatch", "Err: " + ex);
                        }
                        return;
                    }


                }

                List<SData> dsl = new List<SData>();
                foreach (var j in json)
                {
                    SData ds = new SData();
                    string fSn = MD5Hash(j.username + j.name);
                    j.fSn = fSn;
                    ds.fileName = j.name;
                    ds.fileTomd5 = fSn;
                    ds.md5Sn = MD5Hash(j.username);
                    ds.uID = j.uID;
                    ds.fSn = fSn;
                    ds.username = j.username;
                    var fc = fileCount.Where(x => x.fSn == fSn).FirstOrDefault();
                    if (fc != null) j.count = fc.count;
                    dsl.Add(ds);
                }


                bool isData = false;
                int dataCount = Program.playData.Count();
                for (int i = 0; i < dataCount; i++)
                {
                    if (Program.playData[i].userKey == client.Id)
                    {
                        Program.playData[i].SystemCheckTime = DateTime.Now;
                        json = dataFilter(json, json.FirstOrDefault().uID);

                        foreach (var _json in json)
                        {
                            var getbase64 = Program.playData[i].domData.Where(x => x.name == _json.name).FirstOrDefault();
                            if (getbase64 != null)
                            {
                                if (getbase64.base64 != "")
                                {
                                    _json.base64 = getbase64.base64;
                                }

                            }
                        }

                        Program.playData[i].domData = json;

                        for (int j = 0; j < Program.DataList.Count; j++)
                        {
                            if (Program.DataList[j].userKey == client.Id)
                            {
                                Program.DataList[j].sData = dsl;
                            }
                        }
                        isData = true;
                        break;
                    }
                }
                if (!isData)
                {
                    PlayerData info = new PlayerData();
                    info.userKey = client.Id;
                    info.SystemCheckTime = DateTime.Now;
                    info.userID = json.FirstOrDefault().uID;
                    info.domData = dataFilter(json, json.FirstOrDefault().uID);
                    Program.playData.Add(info);

                    DataState dslist = new DataState();
                    dslist.userKey = client.Id;
                    dslist.userID = json.FirstOrDefault().uID;
                    dslist.sData = dsl;
                    Program.DataList.Add(dslist);

                    using (P2PContext _db = new P2PContext())
                    {
                        var _dlrList = _db.download_record.Where(x => x.uID == info.userID).ToList();
                        if (_dlrList.Count() != 0)
                        {
                            _dlrList.ForEach(a => a.state = 1);
                            _db.download_record.UpdateRange(_dlrList);
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        private static List<DomData> dataFilter(List<DomData> data,int uid)
        {
            int defCount = 50;
            long defSize = 500000000;
            using (P2PContext p2p = new P2PContext())
            {
                var u = p2p.user.Where(x => x.id == uid).FirstOrDefault();

                if (u.uLv == 5)
                {
                    defCount = 100000;
                    defSize = 1000000000000000000;
                }
                else if (u.uLv == 1)
                {

                    defCount = 80;
                    defSize = 1000000000;
                }
                else if (u.uLv == 2)
                {
                    defCount = 120;
                    defSize = 1500000000;

                }
            }

            long total = 0;
            int count = 0;
            List<DomData> json = new List<DomData>();
            try {
                foreach(var item in data)
                {
                                      
                    if(item.ext == "png" || item.ext == "gif" || item.ext == "jpg" || item.ext == "zip"
                            || item.ext == "ai" || item.ext == "sql" || item.ext == "html" || item.ext == "txt" || item.ext == "unitypackage"
                            || item.ext == "exe" || item.ext == "iso" || item.ext == "dmg" || item.ext == "apk" || item.ext == "nupkg" || item.ext == "ipa" || item.ext == "7z"
                            || item.ext == "rar" || item.ext == "mp4" || item.ext == "mp3" || item.ext == "pdf" || item.ext == "msi" || item.ext == "psd"
                            || item.ext == "docx" || item.ext == "doc" || item.ext == "xlsx" || item.ext == "xls" || item.ext == "3ds" || item.ext == "ogg" || item.ext == "pdb"
                            || item.ext == "ttf" || item.ext == "tgz" || item.ext == "tga" || item.ext == "swf" || item.ext == "ppt" || item.ext == "json" || item.ext == "xml"
                            || item.ext == "dwg" || item.ext == "bmp" || item.ext == "tar" || item.ext == "pptx" || item.ext == "jpeg" || item.ext == "svg" || item.ext == "fon"
                            || item.ext == "avi" || item.ext == "mkv" || item.ext == "mov" || item.ext == "qt" || item.ext == "qtx" || item.ext == "ico"
                            || item.ext == "webm" || item.ext == "m3u8" || item.ext == "ts" || item.ext == "kjv")
                    {
                        total += item.sizeToInt;
                        if (total > defSize) break;
                        if(count > defCount) break;
                        json.Add(item);

                        count++;
                    }

                }

            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
            
            return json;
        }

        ///<summary> netcore MD5加密 </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string MD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "");
            }
        }

        ///<summary> 轉bytes </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private T ByteArrayToObject<T>(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryFormatter = new BinaryFormatter();
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                var obj = binaryFormatter.Deserialize(memoryStream);
                return (T)obj;
            }
        }

        private static async Task<string> ReceiveStringAsync(WebSocket socket)
        {
            var buffer = new ArraySegment<byte>(new byte[8 * 1024]);
            using (var ms = new MemoryStream())
            {
                WebSocketReceiveResult result;
                do
                {
                    //ct.ThrowIfCancellationRequested();

                    result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                }
                while (!result.EndOfMessage);

                ms.Seek(0, SeekOrigin.Begin);
                if (result.MessageType != WebSocketMessageType.Text)
                {
                    return null;
                }

                // Encoding UTF8: https://tools.ietf.org/html/rfc6455#section-5.6
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
        //base64轉換
        public static string urlsafe_b64encode(string data)
        {
            string sOut = data;
            sOut = sOut.Replace("+", "-");
            sOut = sOut.Replace("/", "_");
            sOut = sOut.Replace("=", ".");
            return sOut;
        }



        //計時器await
        public static async Task OnTimedEventAsync()
        {
            sendSearch();          
            await addRecord();         
            sendUserData();

            //removeData();
            //removePalyer();


            //await removeDataAsync();
            //playerRemove();//清除連線
            removePalyer();//清除資料

            if ((int)DateTime.Now.Subtract(m_ProgressTime).TotalSeconds > 0.5)
            {
                FileProgress();
            }
            if ((int)DateTime.Now.Subtract(m_IPTime).TotalSeconds > 2)
            {
                getFileCount();
            }
            sendbase64();
            if ((int)DateTime.Now.Subtract(m_SavaTime).TotalSeconds > 60)
            {
                savaData();
            }

            await shorURL();






        }

        private static async Task shorURL() 
        {
            try {
                if (Program.playData.Count == 0) return;

                using (P2PContext db = new P2PContext())
                {
                    List<short_url> urlData = db.short_url.ToList();

                    foreach (var item in Program.playData)
                    {
                        foreach (var dom in item.domData)
                        {
                            if (dom.urlID == 0 && !dom.isSet)
                            {
                                dom.isSet = true;
                                JObject o = new JObject();
                                o["fileName"] = dom.name;
                                o["fileExt"] = dom.ext;
                                o["user"] = dom.username;
                                o["uID"] = dom.uID;
                                o["fSn"] = dom.fSn;
                                o["size"] = dom.sizeToInt;
                                byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(o.ToString());
                                string ToBase64 = Convert.ToBase64String(bytes);
                                ToBase64 = urlsafe_b64encode(ToBase64);

                                short_url url = urlData.Where(x => x.fSn == dom.fSn).FirstOrDefault();
                                if (url == null)
                                {
                                    short_url savaUrl = new short_url();
                                    savaUrl.base64 = ToBase64;
                                    savaUrl.fSn = dom.fSn;
                                    db.short_url.Add(savaUrl);
                                    await db.SaveChangesAsync();
                                    dom.urlID = savaUrl.id;

                                }
                                else
                                {
                                    dom.urlID = url.id;

                                }
                            }


                        }
                    }


                }

            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
            


        }

        private static void savaData()        
        {
            m_SavaTime = DateTime.Now;
            string json = JsonConvert.SerializeObject(Program.playData);

            redisData.StringSet("FileList", json);

        }

        private static void sendSearch() 
        {
            //發送搜尋給玩家
            try
            {
                if (SearchList.Count == 0) return;
                for(int i = 0;i < SearchList.Count;i++)
                {

                    CmdData info = new CmdData();
                    string _data = "";
                    info.Cmd = "GetData";
                    
                    var client = WsCollectionService.Get(SearchList[i].sendSn);
                    if (client == null)
                    {
                        SearchList.Remove(SearchList[i]);
                        continue;
                    }
                    if (SearchList[i].domData.Count <= 0)
                    {
                        info.Data = "";
                        _data = JsonConvert.SerializeObject(info);
                        client.SendMessageAsync(_data);
                        SearchList.Remove(SearchList[i]);
                        continue;
                    }
                    List<PlayerData> sp = SearchList.ToList();
                   
                    PlayerData temp = new PlayerData();
                    PlayerData temp64 = new PlayerData();
                    //temp = sp[i];


                    temp = new PlayerData()
                    {
                        userID = SearchList[i].userID,
                        userKey = SearchList[i].userKey,
                        domData = new List<DomData>()
                    };
                    temp64 = new PlayerData()
                    {
                        userID = SearchList[i].userID,
                        userKey = SearchList[i].userKey,
                        domData = new List<DomData>(),
                        sendSn = SearchList[i].sendSn
                    };
                    List<DomData> alldom = sp[i].domData.Select(x => (DomData)x.Clone()).ToList();
                    List<DomData> alldom64 = sp[i].domData.Select(x => (DomData)x.Clone()).ToList();

                    int count = (alldom64.Count > 9) ? 10 : alldom64.Count;

                    for (int j = 0; j < count; j++)
                    {
                        if (alldom64[j].urlID == 0) continue;
                        temp64.domData.Add(alldom64[j]);
                        alldom[j].base64 = "";
                        temp.domData.Add(alldom[j]);
                        //SearchList[i].domData.Take(5).ToList()
                    }
                    base64List.Add(temp64);

                    //傳搜尋的文字訊息
                    info.Data = temp;
                    _data = JsonConvert.SerializeObject(info);
                    client.SendMessageAsync(_data);

                    //傳base64
                    //CmdData send64 = new CmdData();
                    //send64.Cmd = "Send64";
                    //send64.Data = temp64;
                    //_data = JsonConvert.SerializeObject(send64);
                    //client.SendMessageAsync(_data);

                    SearchList[i].domData = SearchList[i].domData.Skip(10).ToList();

                    
                    //if (SearchList[i].domData.Count >= 5) SearchList[i].domData = SearchList[i].domData.Skip(5).ToList();
                    //else SearchList[i].domData = SearchList[i].domData.ToList();



                }

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        private static void sendbase64() {
            //發送base64圖給玩家
            try
            {
                if (base64List.Count == 0) return;
                for (int i = 0; i < base64List.Count; i++)
                {

                    CmdData info = new CmdData();
                    string _data = "";
                    info.Cmd = "Send64";

                    var client = WsCollectionService.Get(base64List[i].sendSn);
                    if (client == null)
                    {
                        base64List.Remove(base64List[i]);
                        continue;
                    }
                    if (base64List[i].domData.Count <= 0)
                    {
                        info.Data = "";
                        _data = JsonConvert.SerializeObject(info);
                        client.SendMessageAsync(_data);
                        base64List.Remove(base64List[i]);
                        continue;
                    }
                    List<PlayerData> sp = base64List.ToList();


                    PlayerData temp64 = new PlayerData();
                    temp64 = sp[i];


                    if (base64List[i].domData.Count > 1)
                    {
                        temp64 = new PlayerData()
                        {
                            userID = base64List[i].userID,
                            userKey = base64List[i].userKey,
                            domData = base64List[i].domData.Take(2).ToList()
                        };

                    }

                    //傳base64
                    info.Data = temp64;
                    _data = JsonConvert.SerializeObject(info);
                    client.SendMessageAsync(_data);

                    base64List[i].domData = base64List[i].domData.Skip(2).ToList();


                    //if (SearchList[i].domData.Count >= 5) SearchList[i].domData = SearchList[i].domData.Skip(5).ToList();
                    //else SearchList[i].domData = SearchList[i].domData.ToList();



                }

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        private static void sendUserData()
        {
            //發送個人UserData
            try
            {
                if (UseDataList.Count == 0) return;
                for (int i = 0; i < UseDataList.Count; i++)
                {

                    CmdData info = new CmdData();
                    string _data = "";
                    info.Cmd = "GetData";

                    var client = WsCollectionService.Get(UseDataList[i].sendSn);
                    if (client == null)
                    {
                        UseDataList.Remove(UseDataList[i]);
                        continue;
                    }
                    if (UseDataList[i].domData.Count <= 0)
                    {
                        info.Data = "";
                        _data = JsonConvert.SerializeObject(info);
                        client.SendMessageAsync(_data);
                        UseDataList.Remove(UseDataList[i]);
                        continue;
                    }
                    List<PlayerData> sp = UseDataList.ToList();

                    PlayerData temp = new PlayerData();
                    PlayerData temp64 = new PlayerData();

                    temp = new PlayerData()
                    {
                        userID = UseDataList[i].userID,
                        userKey = UseDataList[i].userKey,
                        domData = new List<DomData>()
                    };
                    temp64 = new PlayerData()
                    {
                        userID = UseDataList[i].userID,
                        userKey = UseDataList[i].userKey,
                        domData = new List<DomData>(),
                        sendSn = UseDataList[i].sendSn
                    };
                    List<DomData> alldom = sp[i].domData.Select(x => (DomData)x.Clone()).ToList();
                    List<DomData> alldom64 = sp[i].domData.Select(x => (DomData)x.Clone()).ToList();

                    int count = (alldom64.Count > 9) ? 10 : alldom64.Count;

                    for (int j = 0; j < count; j++)
                    {
                        if (alldom64[j].urlID == 0) continue;
                        temp64.domData.Add(alldom64[j]);
                        alldom[j].base64 = "";
                        temp.domData.Add(alldom[j]);
                        //SearchList[i].domData.Take(5).ToList()
                    }

                    base64List.Add(temp64);

                    info.Data = temp;
                    _data = JsonConvert.SerializeObject(info);
                    client.SendMessageAsync(_data);


                    //傳base64
                    /*CmdData send64 = new CmdData();
                    send64.Cmd = "Send64";
                    send64.Data = temp64;
                    _data = JsonConvert.SerializeObject(send64);
                    client.SendMessageAsync(_data);*/

                    UseDataList[i].domData = UseDataList[i].domData.Skip(10).ToList();
                    //if (UseDataList[i].domData.Count >= 5) UseDataList[i].domData = UseDataList[i].domData.Skip(5).ToList();
                    //else UseDataList[i].domData = UseDataList[i].domData.ToList();


                }

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        
        private static void FileProgress() 
        {
            m_ProgressTime = DateTime.Now;
            try 
            {
                var cache = RedisConnectorHelper.Connection.GetDatabase();
                if (cache == null) return;
                List<downloadModel> dom = new List<downloadModel>();
                using (P2PContext _db = new P2PContext())
                {
                    var list = _db.download_start.ToList();
                    for (int i = 0; i < list.Count(); i++)
                    {
                        string fileSn = MD5Hash(list[i].sn + list[i].fileName);
                        try 
                        {
                            var data = cache.StringGet(fileSn);
                            if (!data.IsNull)
                            {
                                var model = JsonConvert.DeserializeObject<downloadModel>(data);
                                dom.Add(model);
                            }
                        }
                        catch(Exception ex)
                        {
                            Program.WriteLog("TryCatch", "Err: " + ex);
                        }
                                               
                    }
                }
                    
                for (int i = 0; i < GetProgress.Count; i++)
                {
                    var client = WsCollectionService.Get(GetProgress[i].userKey);
                    if (client != null)
                    {
                        CmdData info = new CmdData();
                        info.Cmd = "Progress";
                        if (GetProgress[i].fname != null && GetProgress[i].uSn != null)
                        {
                            var getData = dom.Where(x => x.uSn == GetProgress[i].uSn && x.fileName == GetProgress[i].fname).ToList();
                            info.Data = getData;
                        }
                        else {
                            info.Data = dom;
                        }
                        
                        var _data = JsonConvert.SerializeObject(info);
                        client.SendMessageAsync(_data);
                    }
                    else
                    {

                    }
                }

                //Debug.WriteLine(dom.Count());
                //Debug.WriteLine(JsonConvert.SerializeObject(dom));
            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
                     

        }

        /*private static void playerRemove()
        {
            try {
                List<Player> removeTemp = new List<Player>();
                for (int i = 0; i < AllPaleyr.Count; i++)
                {
                    if (AllPaleyr[i] != null)
                    {
                        if (AllPaleyr[i].SrvPingTime > 70)
                        {
                            var webSocket = WsCollectionService.Get(AllPaleyr[i].userKey);
                            if (webSocket != null)
                            {
                                WsCollectionService.Remove(webSocket);
                                //await DataClose(AllPaleyr[i].userKey);
                                removeTemp.Add(AllPaleyr[i]);

                            }

                        }
                    }

                }

                for (int i = 0; i < removeTemp.Count; i++)
                {
                    AllPaleyr.Remove(removeTemp[i]);
                }

            }
            catch (Exception ex)
            {

                Program.WriteLog("TryCatch", "Err: " + ex);
            }


        }*/

        private static void removePalyer() {
            try{

                List<PlayerData> removePlayer = new List<PlayerData>();
                for (int i = 0; i < Program.playData.Count; i++)
                {
                    int SrvPingTime = (int)DateTime.Now.Subtract(Program.playData[i].SystemCheckTime).TotalSeconds;
                    if (SrvPingTime > 70)
                    {
                        removePlayer.Add(Program.playData[i]);
                        Program.DataList = Program.DataList.Where(x => x.userKey != Program.playData[i].userKey).ToList();
                    }
                }

                for (int i = 0; i < removePlayer.Count; i++)
                {
                    Program.playData.Remove(removePlayer[i]);
                }
            }catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }
            


        }

        private static async Task removeDataAsync() {
            try 
            {
                List<PlayerData> removePlayer = new List<PlayerData>();
                List<DataState> removeTemp = new List<DataState>();
                var socketList = WsCollectionService.GetAll();
                for (int i = 0; i < socketList.Count; i++)
                {
                    var open = socketList[i].WebSocket.State.ToString();
                    if (open != "Open")
                    {
                        removePlayer = Program.playData.Where(x => x.userKey == socketList[i].Id).ToList();
                        removeTemp = Program.DataList.Where(x => x.userKey == socketList[i].Id).ToList();
                        
                    }
                    /*var client = WsCollectionService.Get(Program.playData[i].userKey);
                    if (client == null)
                    {
                        removeTempP.Add(Program.playData[i]);
                    }*/


                }
                var temp = Program.playData;
                for (int i = 0;i < temp.Count();i++)
                {
                    var getSocket = WsCollectionService.Get(temp[i].userKey);
                    if(getSocket == null)
                    {
                        await DataClose(temp[i].userKey);
                    }
                    //await DataClose(webSocket.Id);
                }

                /*for (int i = 0; i < removePlayer.Count; i++)
                {
                    Program.playData.Remove(removePlayer[i]);
                    var client = WsCollectionService.Get(removePlayer[i].userKey);
                    if(client != null)
                    {
                        WsCollectionService.Remove(client);
                    }
                    
                }


                for (int i = 0; i < removeTemp.Count; i++)
                {
                    Program.DataList.Remove(removeTemp[i]);
                    var client = WsCollectionService.Get(removeTemp[i].userKey);
                    if (client != null)
                    {
                        WsCollectionService.Remove(client);
                    }
                }*/


                /*for (int i = 0; i < Program.DataList.Count; i++)
                {
                    var client = WsCollectionService.Get(Program.DataList[i].userKey);
                    if (client == null)
                    {
                        removeTemp.Add(Program.DataList[i]);
                    }


                }*/


            }
            catch (Exception ex) {

                Program.WriteLog("TryCatch", "Err: " + ex);
            }
                         

        }

        //------------------------------------Task
        private static async Task addRecord() {
            

             //資料庫新增檔案下載資訊
            try
            {
                if (waitRecord.Count == 0) return;
                if (waitRecord.FirstOrDefault() != null)
                {
                    download_record temp = waitRecord.FirstOrDefault();
                    waitRecord.Remove(waitRecord.FirstOrDefault());
                    using (P2PContext _db = new P2PContext())
                    {
                        bool isExits = _db.download_record.Any(x => x.uID == temp.uID && x.fSn == temp.fSn);
                        if (!isExits)
                        {
                            _db.download_record.Add(temp);
                            await _db.SaveChangesAsync();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }



        }

        private static void getFileCount()
        {
            m_IPTime = DateTime.Now;
            try 
            {
                //fileCount
                using (P2PContext _db = new P2PContext())
                {

                    var group = _db.ip_record.GroupBy(x => x.fSn).Select(x => new FileCount { fSn = x.First().fSn, count = x.Count() }).ToList();
                    fileCount.Clear();
                    fileCount.AddRange(group);
                }

            }
            catch(Exception ex)
            {
                Program.WriteLog("TryCatch", "Err: " + ex);
            }


        }







    }
}
