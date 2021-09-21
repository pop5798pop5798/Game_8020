using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreChatRoom.Model
{
    public class Player {
        public string userKey { get; set; }
        public int SrvPingTime { get; set; }     // 該玩家與Srv的Ping
        public DateTime SystemCheckTime { get; set; }       // 系統檢查時間.
        public string fname { get; set; } //檔案名稱
        public string uSn {get;set;}//持有者sn

    }

    //等待檔案
    public class WaitFiles
    {
        public string sn { get; set; }
        public string fSn { get; set; }
        public int hloderID { get; set; }
    }

    //檔案流量
    public class FileCount
    {
        public string fSn { get; set; }
        public long count { get; set; }
    }
    //搜尋
    public class SearchData
    {
        public string keyWord { get; set; }
    }

    //取得檔案
    public class GetData
    {
        public int sn { get; set; }
    }

    //設置base64
    public class SetBase64
    {
        public string name { get; set; }
        public string base64 { get; set; }
    }



    /*----------------------- 傳送部分 -----------------------*/

    public class PostData
    {
        public string Sn { get; set; }
        public string fileName { get; set; }
        public string Url { get; set; }
        public int uID { get; set; }
        public string fSn { get; set; }
        public int downID { get; set; } //下載者ID
        public string userToMd5 { get; set; }
        public int size { get; set; }

    }

    //取得單檔案
    public class DataDetail
    {
        public string FileName { get; set; }
        public string md5Sn { get; set; }
        public string user { get; set; }
        public int uID { get; set; }//持有者id
        public string fSn { get; set; }//檔案Sn
        public int size { get; set; }
        public int currentUID { get; set; }//取檔案的使用者id

    }

}
