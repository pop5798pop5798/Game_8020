using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2PCore.Models
{
    public class download_record
    {
        public int id { get; set; }
        public string fileName { get; set; }
        public string fSn { get; set; }
        public int uID { get; set; }
        public string url { get; set; }
        public string fileContent { get; set; }
        public string image { get; set; }
        public int state { get; set; }
        public DateTime date { get; set; }
        public long count { get; set; }
    }
}
