using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace TestFileSync
{
    class Program
    {

        public class GetPathFromXML
        {
            public string SourcePathFromXML { get; set; }
            public string DestinationPathFromXML { get; set; }
        }

        class MaWacha
        {

            public ObservableCollection<GetPathFromXML> getDDBs = new ObservableCollection<GetPathFromXML>();
            public ObservableCollection<GetPathFromXML> getD { get { return getDDBs; } }
            string configurationList = @"config_test.xml";
            //[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
            public MaWacha()
            {
                getDDBs = GetPathXML(configurationList);
            }

            public void Start()
            {
                foreach (var pathPair in getD)
                {
                    MaWachacha(pathPair);
                }
                while (true)
                {
                    Thread.Sleep(500);
                }
            }

            private static void MaWachacha(GetPathFromXML maPatha)
            {
                FileSystemWatcher maWacha = new FileSystemWatcher();
                maWacha = new FileSystemWatcher();
                maWacha.Path = maPatha.SourcePathFromXML;
                maWacha.IncludeSubdirectories = true;
                maWacha.Filter = "*.*";
                maWacha.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
                maWacha.Changed += (source, e) => OnChanged(source, e, maPatha.DestinationPathFromXML, maPatha.SourcePathFromXML);
                maWacha.Created += (source, e) => OnChanged(source, e, maPatha.DestinationPathFromXML, maPatha.SourcePathFromXML);
                maWacha.Renamed += (source, e) => OnChanged(source, e, maPatha.DestinationPathFromXML, maPatha.SourcePathFromXML);

                maWacha.EnableRaisingEvents = true;
            }

            private static void OnChanged(object source, FileSystemEventArgs e, string myPath, string mySourcePath)
            {
                //EventLog fileSyncOnchangeLog = new EventLog();
                //fileSyncOnchangeLog.Source = "MaWacha";
                //fileSyncOnchangeLog.Log = "WachaLoga";
                //bool chkRemoteHost = CheckPath(myPath); ;
                //bool chkDestFolder = CheckDestinationFolder(myPath);
                //string sourcefilePath = e.FullPath;
                //FileInfo fileInfo = new FileInfo(sourcefilePath);
                //string destFilePath = myPath;
                //string dFP = Path.Combine(destFilePath, e.Name);
                //string bodymsg = $"File copied from {e.FullPath} to {dFP}";

                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    if (Directory.Exists(e.FullPath))
                    {
                        foreach (var file in Directory.GetFiles(e.FullPath))
                        {
                            Console.WriteLine(Path.GetFileName(file));
                            var eventArgs = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(file), Path.GetFileName(file));
                            OnChanged(source, eventArgs, myPath, mySourcePath);
                        }
                    }
                    else
                    {
                        FileAttributes attributes = File.GetAttributes(e.FullPath);

                        if (attributes.HasFlag(FileAttributes.Directory))
                        {

                            Console.WriteLine($"{e.FullPath} is directory", EventLogEntryType.Information);
                        }
                        else
                        {
                            FileInfo file = new FileInfo(e.FullPath);
                            DirectoryInfo directory = file.Directory;
                            
                            if (file.DirectoryName.Equals(mySourcePath))
                            {
                                while (IsFileLocked(file))
                                {
                                    Thread.Sleep(500);
                                }
                                file.CopyTo(Path.Combine(myPath, file.Name));
                            }
                            else
                            {
                                while (IsFileLocked(file))
                                {
                                    Thread.Sleep(500);
                                }
                                DirectoryCopy(directory.FullName, myPath, true);
                            }
                        }
                    }
                }


                #region test


                //DirectoryInfo[] destDirs = new DirectoryInfo(myPath).GetDirectories();
                //DirectoryInfo[] sourceDirs = new DirectoryInfo(e.FullPath).GetDirectories();

                //FileAttributes attributes = File.GetAttributes(e.FullPath);

                //if (attributes.HasFlag(FileAttributes.Directory))
                //{

                //    if (!Directory.Exists(dFP))
                //    {
                //        Directory.CreateDirectory(dFP);

                //        //File.Copy(e.FullPath, dFP, true);
                //        Console.WriteLine($"Directory {dFP} is created", EventLogEntryType.Information);
                //    }
                //}

                #endregion

                //while (IsFileLocked(fileInfo))
                //{
                //    Thread.Sleep(500);
                //}

                //try
                //{
                //    File.Copy(e.FullPath, dFP, true);
                //    SendMailMsg(bodymsg);
                //}
                //catch (DirectoryNotFoundException)
                //{
                //    if (chkRemoteHost != true)
                //    {
                //        //fileSyncOnchangeLog.WriteEntry($"Remote host is not available {myPath}", EventLogEntryType.Information);
                //        Console.WriteLine($"Remote host is not available {myPath}", EventLogEntryType.Information);
                //    }
                //    else if (chkRemoteHost && chkDestFolder != true)
                //    {
                //        Directory.CreateDirectory(myPath);
                //        File.Copy(e.FullPath, dFP, true);
                //        //fileSyncOnchangeLog.WriteEntry($"File copied from {e.FullPath} to {dFP}", EventLogEntryType.Information);
                //        Console.WriteLine($"File copied from {e.FullPath} to {dFP}", EventLogEntryType.Information);
                //    }
                //}

            }

            private static bool IsFileLocked(FileInfo file)
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException)
                {
                    return true;
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }
                return false;
            }

            private static void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(sourceDir);

                if (!directoryInfo.Exists)
                {
                    throw new DirectoryNotFoundException();
                }

                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                Directory.CreateDirectory(destDir);

                FileInfo[] files = directoryInfo.GetFiles();

                foreach (FileInfo file in files)
                {
                    string tempPath = Path.Combine(destDir, file.Name);

                    file.CopyTo(tempPath, false);
                }

                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in directories)
                    {
                        string tempPath = Path.Combine(destDir, subdir.Name);
                        DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                    }
                }
            }

            private static ObservableCollection<GetPathFromXML> GetPathXML(string confPath)
            {
                ObservableCollection<GetPathFromXML> getPaths = new ObservableCollection<GetPathFromXML>();
                string confXMLPath = confPath;
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(confXMLPath);
                XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/configuration/watchFolderPath/folder");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    string sourcePath = xmlNode.SelectSingleNode("sourcePath").InnerText.Trim();
                    string destPath = xmlNode.SelectSingleNode("destinationPath").InnerText.Trim();
                    getPaths.Add(new GetPathFromXML { SourcePathFromXML = sourcePath, DestinationPathFromXML = destPath });
                }
                return getPaths;
            }

            private static List<string> GetFilterConfiguration(string confPath)
            {
                List<string> filterCollection = new List<string>();
                string confXMLPath = confPath;
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(confXMLPath);
                XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/configuration/watchFolderFilter/filter");
                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    string filterValue = xmlNode.InnerText.Trim();
                    filterCollection.Add(filterValue);
                }
                return filterCollection;
            }

            private static void SendMailMsg(string bodymsg)
            {
                string to = "k.noskov@5-tv.ru";
                string from = "autocopy@5-tv.ru";
                string smtpServer = "mail.5-tv.ru";
                MailMessage msg = new MailMessage(from, to);
                msg.Subject = "test msg";
                msg.Body = bodymsg;
                SmtpClient client = new SmtpClient(smtpServer);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("anonymous", "anonymous");

                try
                {
                    client.Send(msg);
                }
                catch (Exception ex)
                {

                }
            }

            private static bool CheckPath(string remotePath)
            {
                bool pingStatus = false;
                Ping checkRemotePC = null;
                if (remotePath != null)
                {
                    string[] rootPath = remotePath.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        checkRemotePC = new Ping();
                        PingReply pingReply = checkRemotePC.Send(rootPath[0]);
                        pingStatus = pingReply.Status == IPStatus.Success;
                    }
                    catch (PingException)
                    {

                    }
                    finally
                    {
                        if (checkRemotePC != null)
                        {
                            checkRemotePC.Dispose();
                        }
                    }
                    return pingStatus;
                }
                return pingStatus;
            }
            private static bool CheckDestinationFolder(string remotePath)
            {
                if (remotePath != null && Directory.Exists(remotePath))
                {
                    return true;
                }
                return false;
            }
        }

        static void Main(string[] args)
        {
            EventLog fileSyncEventLog = new EventLog();
            if (!EventLog.SourceExists("MaWacha"))
            {
                EventLog.CreateEventSource(
                    "MaWacha", "WachaLoga");
            }
            fileSyncEventLog.Source = "MaWacha";
            fileSyncEventLog.Log = "WachaLoga";
            MaWacha maWacha = new MaWacha();
            Thread maWatchaThread = new Thread(new ThreadStart(maWacha.Start));
            maWatchaThread.Start();
        }
    }
}
