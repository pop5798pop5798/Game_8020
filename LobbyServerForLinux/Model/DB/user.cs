using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2PCore.Models
{
    public class user
    {
        public int id {get;set;}
        public string username { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string sn { get; set; }
        public int isOpen { get; set; }
        public int LoginState { get; set; }

        public int uLv { get; set; }
        public DateTime date { get; set; }
    }
}
