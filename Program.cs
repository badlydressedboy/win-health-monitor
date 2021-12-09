using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Diagnostics;
using System.Data;
using System.IO;
using GNAutomationCommon;
using System.Management;
using System.Xml.Serialization;

namespace ServerHealthCheck
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static bool _sendEmails = false;
        private static bool _silentExecution = false;
        static string emailContent = "";
        static void Main(string[] args)
        {

            Helper.WriteInfo("Starting ServerHealthCheck...");
            
            try
            {
                foreach(var a in args)
                {
                    if (a.ToString().ToUpper() == "SENDEMAIL") _sendEmails = true;
                    if (a.ToString().ToUpper() == "SILENT") _silentExecution = true;
                }
                if (args.Length > 0)
                {
                    if (args[0].ToString().ToUpper() == "SENDEMAIL") _sendEmails = true;
                }
                       
                ServerList servers;
                XmlSerializer serializer = new XmlSerializer(typeof(ServerList));
                using (FileStream fileStream = new FileStream("MonitoredServers.xml", FileMode.Open))
                {
                    servers = (ServerList)serializer.Deserialize(fileStream);
                    Debug.WriteLine("");

                    foreach (var s in servers.Servers)
                    {
                        AnalServer(s);
                    }
                }

                // for office 365 username and from must be the same
                if(_sendEmails && (!string.IsNullOrEmpty(emailContent))){

                    Helper.WriteInfo("Sending Email...");

                    Helper.SendEmail(Properties.Settings.Default.SmtpServer
                        , Properties.Settings.Default.SmtpPort
                        , Properties.Settings.Default.SmtpSsl
                        , Properties.Settings.Default.SmtpUser
                        , Properties.Settings.Default.SmtpPassword
                        , Properties.Settings.Default.SmtpTo
                        , Properties.Settings.Default.SmtpCc
                        , "Drive Space Warning"
                        , emailContent + "<br/><br/><span style='font-size: 12px;'>Sent from ServerHealthCheck.exe running on " + Environment.MachineName + "</span>"
                        , Properties.Settings.Default.SmtpUser);
                }
            }
            catch(Exception ex)
            {
                Helper.WriteError(ex.ToString());
            }

            Helper.WriteInfo("Complete");
            if (_silentExecution)
            {
                Environment.Exit(0);
            }
            QuitApp();
        }

        private static void AnalServer(Server server)
        {
            string srvname = server.Name;
            Console.WriteLine("Server: " + srvname);
            try
            {
                //string srvname = "rsukazsvrsql4";
                string strNameSpace = @"\\";

                if (srvname != "")
                    strNameSpace += srvname;
                else
                    strNameSpace += ".";

                strNameSpace += @"\root\cimv2";

                ConnectionOptions oConn = new ConnectionOptions();

                //oConn.Username = @"rs\svcBarryAdmin";
                //oConn.Password = "a11-i-w4nted-w4s-tr4ining";
                //ManagementScope oMs = new ManagementScope(strNameSpace, oConn);
                ManagementScope oMs = new ManagementScope(strNameSpace);

                //get Fixed disk state
                ObjectQuery oQuery = new ObjectQuery("select FreeSpace,Size,Name, VolumeName from Win32_LogicalDisk where DriveType=3");

                //Execute the query
                ManagementObjectSearcher oSearcher = new ManagementObjectSearcher(oMs, oQuery);

                //Get the results
                ManagementObjectCollection oReturnCollection = oSearcher.Get();

                //loop through found drives and write out info

                foreach (ManagementObject oReturn in oReturnCollection)
                {
                    float freespace = 0;
                    float totalspace = 0;
                    float freeGb = 0;
                    freespace = float.Parse(oReturn["FreeSpace"].ToString());
                    freeGb = freespace / 1024 / 1024 / 1024;

                    totalspace = float.Parse(oReturn["Size"].ToString());
                    float pc = ((freespace / 1024 / 1024) / (totalspace / 1024 / 1024)) * 100;

                    // do we care about this drive?
                    string name = oReturn["Name"].ToString().ToUpper();
                    var monitoredDrive = server.Drives.FirstOrDefault(x => x.Letter == name);
                    string volName = oReturn["VolumeName"].ToString().ToUpper();

                    if (string.IsNullOrEmpty(volName))
                    {
                        volName = name + " Drive";
                    }

                    string statusLine = "Drive " + name + " " + FormatBytes((long)freespace) + " Free, " + FormatBytes((long)totalspace) + " Total (" + pc.ToString("F2") + "% Used)";
                    if (monitoredDrive != null)
                    {
                        if (freeGb < monitoredDrive.MinimumGb)
                        {
                            string warningLine = statusLine + " WARNING - "+volName+" Under " + monitoredDrive.MinimumGb + "Gb";
                            emailContent += server.Name + ": " + warningLine + "\n\n</br>";

                            Helper.WriteConsoleError(warningLine);
                            Logger.Info(server.Name + ": " + warningLine);
                        }
                        else
                        {
                            Console.WriteLine(statusLine);
                        }
                    }
                    else
                    {
                        Console.WriteLine(statusLine);
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message.ToUpper().Contains("RPC SERVER"))
                {
                    Helper.WriteError("No connection to target server, check WMI firewall rules/RPC service/WMI Permissions.\n\n " + ex.ToString());
                }
                else
                {
                    Helper.WriteError(ex.ToString());
                }
            }
            Console.WriteLine(" ");
        }

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

        static void QuitApp()
        {
            Console.WriteLine("Press any button to quit");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
