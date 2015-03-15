using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerGUI
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string UUID { get; set; }
        public string Group { get; set; }
        public string Registered { get; set; }
        public string LastAccessed { get; set; }
        public string KnownIps { get; set; }

        public User(string name, string pass, string uuid, string group, string registered, string last, string known)
        {
            Name = name;
            Password = pass;
            UUID = uuid;
            Group = group;
            Registered = registered;
            LastAccessed = last;
            KnownIps = known;
        }

        public User()
        {
            Name = "";
            Password = "";
            UUID = "";
            Group = "";
            Registered = "";
            LastAccessed = "";
            KnownIps = "";
        }
    }

}
