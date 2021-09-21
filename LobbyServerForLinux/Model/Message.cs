using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Models
{
    public class Message
    {
        /// <summary> WS 用戶ID </summary>
        public string Sn { get; set; }
        /// <summary> 名稱 </summary>
        public string Cmd { get; set; }
        /// <summary> 資料 </summary>
        public object Data { get; set; }
    }
}
