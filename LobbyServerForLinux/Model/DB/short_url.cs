using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Model.DB
{
    public class short_url
    {
        public long id { get; set; }
        public string base64 { get; set; }
        public string fSn { get; set; }

    }
}
