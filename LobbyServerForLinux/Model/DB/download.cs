using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2PCore.Models
{
    public class download
    {
        public int id { get; set; }
        public string fileName { get; set; }
        public int holderID { get; set; }
        public int uID { get; set; }
        public string ext { get; set; }
        public int size { get; set; }
        public string fSn { get; set; }
        public DateTime date { get; set; }
    }
}
