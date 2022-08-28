using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lan_Address___Port_Scanner
{
    public class SettingsMemory
    {
        public string ports { get; set; }
        public int maxThread { get; set; }
        public int threadWait { get; set; }
        public string ipAddrases { get; set; }
        public string wifiName { get; set; }
        public long totalTrials { get; set; }
        public List<ipFormat> ipLists { get; set; }
        public void saveSettings()
        {
            Properties.Settings.Default.Ports = ports;
            Properties.Settings.Default.maxThread = maxThread;
            Properties.Settings.Default.threadWait = threadWait;
            Properties.Settings.Default.ipAddrases = ipAddrases;
            Properties.Settings.Default.Save();
        }
    }
}
