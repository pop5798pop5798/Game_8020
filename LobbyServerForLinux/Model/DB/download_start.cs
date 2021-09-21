using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2PCore.Models
{
    public class download_start
    {
        public int id { get; set; }
        public string fileName { get; set; }
        public string fSn { get; set; }
        public int uID { get; set; }
        public string sn { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
