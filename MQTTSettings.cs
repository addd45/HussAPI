using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HussAPI
{
    public class MQTTSettings
    {
        public MQTTSettings() { }

        public string Host { get; set; }
        public string Port { get; set; }
        public string ScoreTopic { get; set; }
        public string ConfigTopic {get;set;}
        public string User { get; set; }
        public string ClientID { get; set; }
    }
}
