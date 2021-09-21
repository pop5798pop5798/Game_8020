using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreChatRoom.Model
{
    public class CmdData
    {
        public string Cmd { get; set; }
        public object Data { get; set; }
    }
}
