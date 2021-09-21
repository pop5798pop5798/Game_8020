using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreChatRoom.Model
{
    public class DataState
    {

        public string userKey { get; set; }
        public int userID { get; set; }
        public List<SData> sData { get; set; }

    }

    //取得檔案狀態
    public class SData
    {
        public string fileName { get; set; }
        public string fileTomd5 { get; set; }
        public string md5Sn { get; set; }
        public int uID { get; set; }
        public string fSn { get; set; }
        public string username { get; set; }
        public int currentUID { get; set; }
        public bool isUse { get; set; } = true;//是否可以使用

        public int state { get; set; }//0無下載中 1下載中 3 下載完畢

    }
}
