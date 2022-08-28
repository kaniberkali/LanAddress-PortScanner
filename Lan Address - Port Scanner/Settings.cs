using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace Lan_Address___Port_Scanner
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        int[] ports = new int[] { 0, 1, 4, 5, 7, 9, 13, 17, 18, 19, 20, 21, 22, 23, 25, 26, 37, 38, 39, 41, 42, 49, 53, 57, 67, 68, 69, 70, 79, 80, 280, 88, 101, 107, 109, 110, 113, 115, 118, 119, 123, 137, 138, 139, 143, 152, 153, 156, 158, 161, 162, 179, 194, 201, 209, 213, 218, 220, 259, 264, 318, 323, 366, 369, 384, 387, 389, 401, 411, 427, 443, 444, 2445, 464, 465, 500, 513, 2514, 515, 524, 530, 531, 540, 542, 546, 547, 554, 563, 587, 591, 593, 604, 631, 636, 639, 646, 647, 648, 652, 654, 666, 674, 691, 692, 695, 698, 699, 700, 701, 702, 706, 711, 712, 720, 829, 860, 873, 901, 981, 989, 990, 991, 992, 993, 995, 1099, 1194, 1198, 1214, 1223, 21337, 1352, 21387, 1414, 1433, 21434, 1494, 1521, 21547, 21723, 1761, 1863, 1900, 1935, 1984, 22000, 2030, 22031, 2082, 2083, 2086, 2087, 2095, 2096, 22181, 2222, 2427, 22447, 22710, 2809, 2967, 23050, 3074, 3128, 3306, 3389, 3396, 3689, 3690, 3724, 3784, 3785, 4500, 4662, 4672, 4894, 4899, 5000, 5003, 5121, 5190, 5222, 5223, 5269, 5432, 5517, 5800, 5900, 6000, 6112, 6346, 6347, 6600, 6667, 9009, 9715, 9714, 9987 };
        public string textClear(string txt)
        {
            return Regex.Replace(txt, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
        private string time()
        {
            return DateTime.Now.ToString().Replace(":", ".");
        }
        private int getPortFormatter(string text)
        {
            try
            {
                int port = Convert.ToInt32(text);
                if (!(port >= 0 && port <= 65535))
                    return -1;
                return port;
            }
            catch { return -1; }
        }
        private string[] getIpFormatter(string text)
        {
            try
            {
                string[] ipMin = text.Split('-')[0].Split('.');
                string[] ipMax = text.Split('-')[1].Split('.');
                for (int i = 0; i <= 3; i++)
                {
                    int ipMinSlot = Convert.ToInt32(ipMin[i]);
                    int ipMaxSlot = Convert.ToInt32(ipMax[i]);
                    if (!(ipMinSlot >= 0 && ipMinSlot <= 255 && ipMaxSlot >= 0 && ipMaxSlot <= 255 && ipMinSlot <= ipMaxSlot))
                        return null;
                }
                return new string[] { text.Split('-')[0], text.Split('-')[1] };
            }
            catch { return null; }
        }
        private string getWifiName()
        {
            string result = "";
            var process = new Process
            {
                StartInfo =
                {
                FileName = "netsh.exe",
                Arguments = "wlan show interfaces",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(l => l.Contains("SSID") && !l.Contains("BSSID"));
            if (line == null && GetLocalIPAddress() != null)
            {
                if (GetLocalIPAddress() != "127.0.0.1")
                    return "Etharnet";
                else
                    return "";
            }
            result = line.Substring(4, line.Length - 4);
            result = result.Substring(4, result.Length - 4);
            return result;
        }
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            }
            return null;
        }
        private long getAllPorts()
        {
            return listBox4.Items.Count;
        }
        private long getAllTrials()
        {
            long result = 0;
            long result_ip = 1;
            List<ipFormat> ips = getAllIps();
            for (int i = 0; i < ips.Count; i++)
            {
                result_ip = 1;
                for (int q = 3; q >= 0; q--)
                    result_ip *= (ips[i].maxIp[q] + 1) - ips[i].minIp[q];
                result += result_ip;
            }
            return result;
        }
        private List<ipFormat> getAllIps()
        {
            List<ipFormat> ips = new List<ipFormat>();
            for (int i = 0; i < listBox1.Items.Count; i++)
                ips.Add(ipFormatList(i));
            return ips;
        }
        private ipFormat ipFormatList(int id)
        {
            string[] minIp = listBox1.Items[id].ToString().Split('.');
            string[] maxIp = listBox2.Items[id].ToString().Split('.');
            ipFormat ipFormat = new ipFormat();
            for (int i = 0; i < 4; i++)
            {
                ipFormat.minIp[i] = Convert.ToInt32(minIp[i]);
                ipFormat.maxIp[i] = Convert.ToInt32(maxIp[i]);
            }
            return ipFormat;
        }

        private bool writeTxt(List<string> datas, string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                foreach (string data in datas)
                    sw.WriteLine(data);
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch { }
            return false;
        }
        private bool writeTxt(List<int> datas, string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                foreach (int data in datas)
                    sw.WriteLine(data.ToString());
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch { }
            return false;
        }

        private bool writeAndAddTxt(List<int> datas,ListBox list, string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                foreach (int data in datas)
                {
                    list.Items.Add(data.ToString());
                    sw.WriteLine(data.ToString());
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                return true;
            }
            catch { }
            return false;
        }
        private List<string> readTxt(string path)
        {
            List<string> result = new List<string>();
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sw = new StreamReader(fs);
                string yazi = sw.ReadLine();
                while (yazi != null)
                {
                    result.Add(yazi);
                    yazi = sw.ReadLine();
                }
                sw.Close();
                fs.Close();
            }
            catch
            {
                return null;
            }
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Text Document | *.txt";
            file.Title = "Select the text document hosting the ports";

            if (file.ShowDialog() == DialogResult.OK)
            {
                listBox4.Items.Clear();
                List<string> readLines = readTxt(file.FileName);
                if (readLines != null)
                {
                    foreach (string line in readLines)
                    {
                        int port = getPortFormatter(line);
                        if (port != -1)
                            listBox4.Items.Add(port.ToString());
                    }
                }
                else
                {
                    MessageBox.Show(new Form() { TopMost = true },"Could not read from file.", "@kodzamani.tk");
                }
                textBox1.Text = file.FileName;
                textBox2.Text = getAllPorts().ToString();
            }
            else
                MessageBox.Show(new Form() { TopMost = true },"The read operation has been cancelled.", "@kodzamani.tk");
        }
        string wifiName;
        private void Settings_Load(object sender, EventArgs e)
        {
            textBox6.Text = getAllTrials().ToString();
            wifiName = textClear(getWifiName()) + " - " + time();
            if (wifiName.Length <= 0)
            {
                MessageBox.Show(new Form() { TopMost = true },"You must be connected to the internet to use the application.", "@kodzamani.tk");
                Application.Exit();
            }
            Thread thread = new Thread(() =>
            {
                textBox7.Text = wifiName;
                textBox1.Text = Properties.Settings.Default.Ports;
                numericUpDown9.Value = Properties.Settings.Default.maxThread;
                numericUpDown10.Value = Properties.Settings.Default.threadWait;

                if (File.Exists(Properties.Settings.Default.ipAddrases))
                {
                    List<string> readLines = readTxt(Properties.Settings.Default.ipAddrases);
                    if (readLines != null)
                    {
                        foreach (string line in readLines)
                        {
                            string[] ips = getIpFormatter(line);
                            if (ips != null)
                            {
                                listBox1.Items.Add(ips[0]);
                                listBox2.Items.Add(ips[1]);
                            }
                        }
                    }
                    textBox3.Text = Properties.Settings.Default.ipAddrases;
                }
                else
                {
                    writeTxt(new List<string> { "192.168.0.0-192.168.255.255", "172.16.0.0-172.31.255.255", "10.0.0.0-10.255.255.255" }, "Ip Address Range.txt");
                    listBox1.Items.Add("192.168.0.0");
                    listBox1.Items.Add("172.16.0.0");
                    listBox1.Items.Add("10.0.0.0");
                    listBox2.Items.Add("192.168.255.255");
                    listBox2.Items.Add("172.31.255.255");
                    listBox2.Items.Add("10.255.255.255");
                    textBox3.Text = "Ip Address Range.txt";
                }
                if (File.Exists(Properties.Settings.Default.Ports))
                {
                    List<string> readLines = readTxt(Properties.Settings.Default.Ports);
                    foreach (string line in readLines)
                    {
                        int port = getPortFormatter(line);
                        if (port != -1)
                            listBox4.Items.Add(port.ToString());
                    }
                    textBox1.Text = Properties.Settings.Default.Ports;
                }
                else
                {
                    writeAndAddTxt(ports.ToList(), listBox4, "Ports.txt");
                    textBox1.Text = "Ports.txt";
                }
                Properties.Settings.Default.Ports = textBox1.Text;
                Properties.Settings.Default.ipAddrases = textBox3.Text;
                textBox6.Text = getAllTrials().ToString();
                textBox2.Text = getAllPorts().ToString();
            });
            thread.Start();
        }
        private void btn_add_and_remove(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            string ipMin = numericUpDown1.Value + "." + numericUpDown2.Value + "." + numericUpDown3.Value + "." + numericUpDown4.Value;
            string ipMax = numericUpDown8.Value + "." + numericUpDown7.Value + "." + numericUpDown6.Value + "." + numericUpDown5.Value;
            if (btn.Text == "ADD")
            {
                if (numericUpDown1.Value <= numericUpDown8.Value &&
                    numericUpDown2.Value <= numericUpDown7.Value &&
                    numericUpDown3.Value <= numericUpDown6.Value &&
                    numericUpDown4.Value <= numericUpDown5.Value &&
                    listBox1.Items.Contains(ipMin) == false &&
                    listBox2.Items.Contains(ipMax) == false)
                {
                    listBox1.Items.Add(ipMin);
                    listBox2.Items.Add(ipMax);
                    textBox6.Text = Convert.ToString(getAllTrials());
                    if (Convert.ToUInt64(textBox6.Text) > 4294967296)
                    {
                        listBox1.Items.Clear();
                        listBox2.Items.Clear();
                        listBox1.Items.Add("0.0.0.0");
                        listBox2.Items.Add("255.255.255.255");
                        MessageBox.Show(new Form() { TopMost = true },"The maximum value range that can be taken is 0.0.0.0 to 255.255.255.255. There is no need to try the same IP addresses again.", "@kodzamani.tk");
                    }
                }
                else
                    MessageBox.Show(new Form() { TopMost = true },"The ip address on the left must be less than or equal to the ip address you specify on the right.", "@kodzamani.tk");
            }
            else if (listBox1.Items.Contains(ipMin) && listBox2.Items.Contains(ipMax))
            {
                listBox1.Items.Remove(ipMin);
                listBox2.Items.Remove(ipMax);
            }
            else
                MessageBox.Show(new Form() { TopMost = true },"The ip address on the left must be less than or equal to the ip address you specify on the right.", "@kodzamani.tk");
            textBox6.Text = getAllTrials().ToString();
        }

        private void lists_selected(object sender, EventArgs e)
        {
            ListBox list = sender as ListBox;
            try
            {
                if (list.Name == "listBox1")
                    listBox2.SelectedIndex = listBox1.SelectedIndex;
                else
                    listBox1.SelectedIndex = listBox2.SelectedIndex;
                ipFormat ip = ipFormatList(list.SelectedIndex);
                numericUpDown1.Value = ip.minIp[0];
                numericUpDown2.Value = ip.minIp[1];
                numericUpDown3.Value = ip.minIp[2];
                numericUpDown4.Value = ip.minIp[3];
                numericUpDown8.Value = ip.maxIp[0];
                numericUpDown7.Value = ip.maxIp[1];
                numericUpDown6.Value = ip.maxIp[2];
                numericUpDown5.Value = ip.maxIp[3];
            }
            catch
            {
                list.SelectedIndex = -1;
            }
        }
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            Move = 1;
            Mouse_X = e.X;
            Mouse_Y = e.Y;
        }
        int Move;
        int Mouse_X;
        int Mouse_Y;
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Move == 1)
            {
                this.SetDesktopLocation(MousePosition.X - Mouse_X, MousePosition.Y - Mouse_Y);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            Move = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (Convert.ToUInt64(textBox6.Text) > 0 && Convert.ToUInt64(textBox2.Text) > 0)
            {
                if (Directory.Exists(wifiName) == false)
                    Directory.CreateDirectory(wifiName);
                SettingsMemory settings = new SettingsMemory();
                settings.totalTrials = (long)Convert.ToUInt64(textBox6.Text);
                settings.wifiName = textBox7.Text;
                settings.ports = textBox1.Text;
                settings.maxThread = Convert.ToInt32(numericUpDown9.Value);
                settings.threadWait = Convert.ToInt32(numericUpDown10.Value);
                FileStream fs = new FileStream(textBox3.Text, FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                for (int i = 0;i < listBox1.Items.Count; i++)
                    sw.WriteLine(listBox1.Items[i].ToString() + "-" + listBox2.Items[i].ToString());
                sw.Flush();
                sw.Close();
                fs.Close();
                writeTxt(listBox4.Items.Cast<String>().ToList(), textBox1.Text);
                settings.ipAddrases = textBox3.Text;
                settings.saveSettings();
                this.Hide();
                Form1 frm = new Form1();
                frm.settings = settings;
                settings.ipLists = getAllIps();
                frm.Show();
            }
            else
                MessageBox.Show(new Form() { TopMost = true },"You forgot to add the minimum and maximum IP addresses and port numbers to the list.", "@kodzamani.tk");
        }

        private void portsChanged(object sender, EventArgs e)
        {
            NumericUpDown numeric = sender as NumericUpDown;
            if (checkBox1.Checked == true)
            {
                if (numeric.Name == "numericUpDown12")
                    numericUpDown11.Value = numericUpDown12.Value;
                else
                    numericUpDown12.Value = numericUpDown11.Value;
            }
        }

        private void listPortsSelected(object sender, EventArgs e)
        {
            try { numericUpDown12.Value = Convert.ToInt32(listBox4.Text); } catch { }
        }

        private void btn_ports_add_and_remove(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.Text == "ADD")
            {
                if (numericUpDown12.Value <= numericUpDown11.Value)
                {
                    for (int i = Convert.ToInt32(numericUpDown12.Value); i <= Convert.ToInt32(numericUpDown11.Value); i++)
                        if (listBox4.Items.Contains(i.ToString()) == false)
                            listBox4.Items.Add(i.ToString());
                    textBox2.Text = Convert.ToString(getAllPorts());
                    if (Convert.ToUInt64(textBox2.Text) > 65536)
                    {
                        listBox4.Items.Clear();
                        for (int i = 0; i <= 65535; i++)
                            if (listBox4.Items.Contains(i.ToString()) == false)
                                listBox4.Items.Add(i.ToString());
                        MessageBox.Show(new Form() { TopMost = true },"The maximum value range that can be taken is 0 to 65535. There is no need to try the same ports again.", "@kodzamani.tk");
                    }
                }
                else
                    MessageBox.Show(new Form() { TopMost = true },"The port number on the left must be less than or equal to the port number you specify on the right.", "@kodzamani.tk");
            }
            else
            {
                for (int i = Convert.ToInt32(numericUpDown12.Value); i <= Convert.ToInt32(numericUpDown11.Value);i++)
                    listBox4.Items.Remove(i.ToString());
            }
           textBox2.Text = Convert.ToString(getAllPorts());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Text Document | *.txt";
            file.Title = "Select the file hosting the ip addresses";

            if (file.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                List<string> readLines = readTxt(file.FileName);
                if (readLines != null)
                {
                    foreach (string line in readLines)
                    {
                        string[] ips = getIpFormatter(line);
                        if (ips != null)
                        {
                            listBox1.Items.Add(ips[0]);
                            listBox2.Items.Add(ips[1]);
                        }
                    }
                    textBox3.Text = file.FileName;
                }
                else
                {
                    writeTxt(new List<string> { "192.168.1.1-192.168.255.255", "172.16.0.0-172.31.255.255", "10.0.0.0-10.255.255.255" }, "Ip Address Range.txt");
                    listBox1.Items.Add("192.168.1.1");
                    listBox1.Items.Add("172.16.0.0");
                    listBox1.Items.Add("10.0.0.0");
                    listBox2.Items.Add("192.168.255.255");
                    listBox2.Items.Add("172.31.255.255");
                    listBox2.Items.Add("10.255.255.255");
                    textBox3.Text = "Ip Address Range.txt";
                }
                textBox6.Text = getAllTrials().ToString();
            }
            else
                MessageBox.Show(new Form() { TopMost = true },"The read operation has been cancelled.", "@kodzamani.tk");
        }

        private void Settings_FormClosed(object sender, FormClosedEventArgs e)
        {
            SettingsMemory settings = new SettingsMemory();
            settings.totalTrials = (long)Convert.ToUInt64(textBox6.Text);
            settings.wifiName = textBox7.Text;
            settings.ports = textBox1.Text;
            settings.maxThread = Convert.ToInt32(numericUpDown9.Value);
            settings.threadWait = Convert.ToInt32(numericUpDown10.Value);
            FileStream fs = new FileStream(textBox3.Text, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            for (int i = 0; i < listBox1.Items.Count; i++)
                sw.WriteLine(listBox1.Items[i].ToString() + "-" + listBox2.Items[i].ToString());
            sw.Flush();
            sw.Close();
            fs.Close();
            writeTxt(listBox4.Items.Cast<String>().ToList(), textBox1.Text);
            settings.ipAddrases = textBox3.Text;
            settings.ipLists = getAllIps();
            settings.saveSettings();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown2.Value = numericUpDown1.Value;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}