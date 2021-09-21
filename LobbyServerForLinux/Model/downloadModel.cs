using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyServerForLinux.Models
{
    [Serializable]
    public class downloadModel
    {
        public string uSn { get; set; }
        public string fileName { get; set; }

        public int progress { get; set; }


    }
}
