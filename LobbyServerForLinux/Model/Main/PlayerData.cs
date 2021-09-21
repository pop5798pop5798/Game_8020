using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreChatRoom.Model
{
    public class PlayerData
    {
        public string userKey { get; set; }
        public int userID { get; set; }
        public List<DomData> domData { get; set; }
        public string sendSn { get; set; } //搜尋想發送的sn
        public DateTime SystemCheckTime { get; set; }       // 系統檢查時間.
    }
    public class DomData :ICloneable
    {
        public string name {get;set; }
        public string src { get; set; }

        private string _ext = "";
        public string ext
        {
            get
            {
                if (_ext == "" || _ext == null) return _ext;
                else return _ext.ToLower();
            }
            set { _ext = value; }
        }
        public string size { get; set; }
        public int sizeToInt { get; set; }
        public string username { get; set; }
        public int uID { get; set; }
        public string fSn { get; set; }
        public int isOpen { get; set; }
        public string base64 { get; set; }
        public bool isChange { get; set; }
        public long count { get; set; }
        public long urlID { get; set; }
        public bool isSet { get; set; } = false;

        public object Clone() {
            return this.MemberwiseClone();
        
        }
    }

}
