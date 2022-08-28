using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Policy;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Lan_Address___Port_Scanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        public SettingsMemory settings { get; set; }
        int processCount = 0;
        int ipError = 0;
        int ipSuccess = 0;
        int ipTrials = 0;
        int portTrials = 0;
        List<int> testPorts = new List<int>();
        private void Form1_Load(object sender, EventArgs e)
        {
            Thread th = new Thread(() =>
            {
                Thread.Sleep(3000);
                readPorts();
                progressBar1.Maximum = Convert.ToInt32(settings.totalTrials);
                progressBar2.Maximum = Convert.ToInt32(settings.maxThread);
                progressBar3.Maximum = testPorts.Count;
                label14.ForeColor = Color.Blue;
                label14.Text = "Proccessing";
                Thread th2 = new Thread(startProcces);
                th2.Start();
            });
            th.Start();

        }

        bool setProccessCount = true;
        bool portsNotFinished = false;
        private void startProcces()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int ipCounter = 0; ipCounter  < settings.ipLists.Count; ipCounter++)
            {
                Parallel.For(settings.ipLists[ipCounter].minIp[0], settings.ipLists[ipCounter].maxIp[0] + 1, Slot1 =>
                {
                    Parallel.For(settings.ipLists[ipCounter].minIp[1], settings.ipLists[ipCounter].maxIp[1] + 1, Slot2 =>
                    {
                        Parallel.For(settings.ipLists[ipCounter].minIp[2], settings.ipLists[ipCounter].maxIp[2] + 1, Slot3 =>
                        {
                            Parallel.For(settings.ipLists[ipCounter].minIp[3], settings.ipLists[ipCounter].maxIp[3] + 1, Slot4 =>
                            {
                                for (; ; )
                                {
                                    if (processCount < settings.maxThread)
                                    {
                                        listBox1.Items.Insert(0, Slot1 + "." + Slot2 + "." + Slot3 + "." + Slot4);
                                        ipTrials++;
                                        label6.Text = ipTrials.ToString();
                                        processCount++;
                                        trialIp(Slot1 + "." + Slot2 + "." + Slot3 + "." + Slot4);
                                        Thread.Sleep(settings.threadWait);
                                        setProccessCount = true;
                                        break;
                                    }
                                    else
                                    {
                                        setProccessCount = false;
                                        Thread.Sleep(3000);
                                    }
                                }
                            });
                        });
                    });
                });
            }
            stopwatch.Stop();
            if (portTrials <= 0)
            {
                label14.ForeColor = Color.Green;
                label14.Text = "Completed";
                try
                {
                    Process.Start("LAPS Viewer.exe", settings.wifiName.Replace(' ', '|'));
                }
                catch { }
                MessageBox.Show(new Form() { TopMost = true },"All transactions completed successfully transactions took a total of " + stopwatch.Elapsed.TotalSeconds.ToString() + " seconds", "@kodzamani.tk");
            }
            else
            {
                label14.ForeColor = Color.DeepSkyBlue;
                label14.Text = "Port Scannig";
                portsNotFinished = true;
                MessageBox.Show(new Form() { TopMost = true },"The IP scanning process took a total of " + stopwatch.Elapsed.TotalSeconds.ToString() + " seconds. port scan in progress.", "@kodzamani.tk");
            }
        }

        private void readPorts()
        {
            FileStream fs = new FileStream(settings.ports, FileMode.Open, FileAccess.Read);
            StreamReader sw = new StreamReader(fs);
            string yazi = sw.ReadLine();
            while (yazi != null)
            {
                try { testPorts.Add(Convert.ToInt32(yazi)); } catch { }
                yazi = sw.ReadLine();
            }
            sw.Close();
            fs.Close();
        }
        private void trialIp(string ip)
        {
            Thread thread = new Thread(() =>
            {
                Stopwatch time = new Stopwatch();
                time.Start();
                try
                {
                    Stopwatch time2 = new Stopwatch();
                    time2.Start();
                    Ping ping = new Ping();
                    PingReply reply = ping.Send(ip, 100);
                    if (reply.Status == IPStatus.Success)
                    {
                        FileStream fs2 = new FileStream(settings.wifiName + "/" + ip + ".txt", FileMode.Create, FileAccess.Write);
                        StreamWriter sw2 = new StreamWriter(fs2);
                        time2.Stop();
                        sw2.WriteLine("Connection: True");
                        sw2.Flush();
                        sw2.Close();
                        fs2.Close();
                        ipSuccess++;
                        ListViewItem item = new ListViewItem(ip);
                        item.SubItems.Add("True");
                        item.SubItems.Add("Testing");
                        item.SubItems.Add("Calculating");
                        listView2.Items.Add(item);
                        Thread thread1 = new Thread(() =>
                        {
                            progressBar3.Value = 0;
                            bool index = false;
                            List<int> ports = new List<int>();
                            portTrials++;
                            Parallel.ForEach(testPorts, port =>
                            {
                                using (TcpClient tcpClient = new TcpClient())
                                {
                                    try
                                    {
                                        progressBar3.Value++;
                                        tcpClient.Connect(ip, port);
                                        ports.Add(port);
                                        if (port == 80)
                                            index = true;
                                    }
                                    catch { }
                                }
                            });
                            try
                            {
                                StreamWriter SW = File.AppendText(settings.wifiName + "/" + ip + ".txt");
                                SW.WriteLine("Index: " + index.ToString());
                                SW.WriteLine("PortsCount: " + ports.Count.ToString());
                                time.Stop();
                                SW.WriteLine("TimeToFindAddress: " + time2.Elapsed.TotalMilliseconds + "ms");
                                SW.WriteLine("TotalTime: " + time.Elapsed.TotalMilliseconds + "ms");
                                Parallel.ForEach(ports, port => { SW.WriteLine(port); });
                                SW.Flush();
                                SW.Close();
                                ListViewItem searchItem = listView2.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Text == ip);
                                searchItem.SubItems[2].Text = index.ToString();
                                searchItem.SubItems[3].Text = ports.Count.ToString();
                            }
                            catch {  }
                            portTrials--;
                            if (portsNotFinished == true && portTrials <= 0)
                            {
                                label14.ForeColor = Color.Green;
                                label14.Text = "Completed";
                                try
                                {
                                    Process.Start("LAPS Viewer.exe", settings.wifiName.Replace(' ','|'));
                                }
                                catch { }
                                MessageBox.Show(new Form() { TopMost = true },"All transactions completed successfully.", "@kodzamani.tk");
                            }
                            label10.Text = portTrials.ToString();
                        });
                        thread1.Start();

                    }
                    else
                    {
                        time.Stop();
                        ipError++;
                    }
                }
                catch { time.Stop(); ipError++; }
                label10.Text = portTrials.ToString();
                label4.Text = ipSuccess.ToString();
                label5.Text = ipError.ToString();
                progressBar1.Value = ipTrials;
                if (setProccessCount == true && processCount < settings.maxThread && processCount >= 0)
                    progressBar2.Value = processCount;
                else
                    progressBar2.Value = settings.maxThread;
                processCount--;
            });
            thread.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            LAPSIcon.Visible = true;
            LAPSIcon.BalloonTipIcon = ToolTipIcon.Info;
            LAPSIcon.BalloonTipText = "I'm hiding here, you can continue to view transactions by double-clicking on my logo whenever you want.";
            LAPSIcon.BalloonTipTitle = "Lan Address - Port Scanner";
            LAPSIcon.ShowBalloonTip(3000);
        }

        private void LAPSIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            LAPSIcon.Visible = false;
            this.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel2.Visible = false;
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
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listView2.SelectedItems[0].SubItems[3].Text == "Calculating")
                    MessageBox.Show(new Form() { TopMost = true },"Port scan on ip address is in progress, ports are not calculated yet.", "@kodzamani.tk");
                else if (Convert.ToInt32(listView2.SelectedItems[0].SubItems[3].Text) > 0)
                {
                    textBox1.Text = listView2.SelectedItems[0].SubItems[0].Text;
                    listBox2.Items.Clear();
                    List<string> readLines = readTxt(settings.wifiName + "/" + textBox1.Text + ".txt");
                    if (readLines != null)
                    {
                        foreach (string line in readLines)
                        {
                            int port = getPortFormatter(line);
                            if (port != -1)
                                listBox2.Items.Add(port.ToString());
                        }
                    }
                    else
                        MessageBox.Show(new Form() { TopMost = true },"Could not read from file.", "@kodzamani.tk");
                    panel2.Visible = true;
                }
                else
                {
                    panel2.Visible = false;
                    MessageBox.Show(new Form() { TopMost = true },"An open port for this ip address was not found.", "@kodzamani.tk");
                }
            } catch { }
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

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://" + textBox1.Text + ":" + listBox2.Text);
            }catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (File.Exists("LAPS Viewer.exe") == false)
                MessageBox.Show(new Form() { TopMost = true },"The LAPS Viewer application is not in the file's directory, please try to install the program again.", "@kodzamani.tk");
            else
            {
                try
                {
                    Process.Start("LAPS Viewer.exe", settings.wifiName.Replace(' ', '|'));
                }
                catch
                {
                    MessageBox.Show(new Form() { TopMost = true },"The application could not be started.", "@kodzamani.tk");
                }
            }
        }
    }
}
