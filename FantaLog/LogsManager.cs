//using CommonTypes.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace FantaLog
{
    public enum Period
    {
        None,
        Day,
        TwoWeeks,
        Month,
        All,
    }

    public abstract class Log
    {
        public virtual string DirectoryName { get; set; }
        public virtual string FileExtension { get; set; }
        public virtual string FileName { get; set; }
        public virtual string FileNameWOExtension { get; set; }
        public virtual string FullPath { get; set; }
    }

    public class TextLog : Log
    {
        public string Year { get; set; }
        public string Month { get; set; }
        public string Date { get; set; }
        public int NumericMonth { get; set; }
    }

    public class ZippedLog : Log
    {
        public DateTime CreationTime { get; set; }
        public string HumanReadableName { get; set; }
    }

    public class LogsManager
    {
        // Each start up:
        // Check date and in case there is first day of month - zip all logs of prev month (e.g. logs of December) to one array (e.g. January)
        // Remove not needed logs (logs from December)
        // and create new daily log

        // Export:
        // Ability to export selected zipped month (from list) and number of non-zipped daily logs (current day, 15 days, 30 days)
        // Automation logs not included (yet)

       // public event EventHandler<OperationFinishedEventArgs> OperationFinished;
        public event EventHandler DiskMissing;
        public event EventHandler LogsNotSelected;
        static LogsManager m_logManager;
        List<Log> m_textLogs = new List<Log>();
        List<Log> m_zips = new List<Log>();
        string TmpPath = @"D:\Temp\Logs";
        string ZipPath = @"D:\Temp";
        string LogPath = @"D:\Logs";
        private Period m_period;

        private LogsManager()
        {
        }

        public static LogsManager Instance
        {
            get
            {
                if (m_logManager == null)
                {
                    m_logManager = new LogsManager();
                }

                return m_logManager;
            }
        }

        public void LoadLogs()
        {
            List<string> filenames = Directory.GetFiles(@"D:\Logs").ToList();

            foreach (string fileName in filenames)
            {
                if (Path.GetExtension(fileName).Contains("log"))
                {
                    AddTextLogToList(fileName);
                }
                else if (Path.GetExtension(fileName).Contains("zip"))
                {
                    AddZipToList(fileName);
                }
            }

            BackupLogs();
        }

        public void UpdateLogsList()
        {
            try
            {
                m_textLogs.Clear();
                List<string> filenames = Directory.GetFiles(@"D:\Logs").Select(f => f).Where(f => f.Contains("log")).ToList();

                foreach (string fileName in filenames)
                {
                    AddTextLogToList(fileName);
                }
            }
            catch (Exception ex)
            {
                SystemLog.Instance.LogError(ex.Message, ex, SystemLog.LOG_LEVEL.LOG_ERROR);
            }
        }

        private void AddTextLogToList(string fileName)
        {
            Log l = new TextLog();

            AddGenericInformation(fileName, ref l);

            string[] subs = fileName.Substring(fileName.IndexOf('_') + 1).Split(new char[] { '.', '-' });

            string month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(subs[1]));
            string year = subs[0];
            string date = month + ", " + year;

            ((TextLog)l).Month = month;
            ((TextLog)l).Year = year;
            ((TextLog)l).Date = date;
            ((TextLog)l).NumericMonth = Convert.ToInt32(subs[1]);

            m_textLogs.Add(l);
        }

        private void AddZipToList(string fileName)
        {
            Log l = new ZippedLog();

            AddGenericInformation(fileName, ref l);

            ((ZippedLog)l).CreationTime = File.GetCreationTime(fileName);
            ((ZippedLog)l).HumanReadableName = l.FileNameWOExtension.Replace('_', ' ');

            m_zips.Add(l);
        }

        private void AddGenericInformation(string fileName, ref Log l)
        {
            l.FileExtension = Path.GetExtension(fileName);
            l.DirectoryName = Path.GetDirectoryName(fileName);
            l.FileName = Path.GetFileName(fileName);
            l.FileNameWOExtension = Path.GetFileNameWithoutExtension(fileName);
            l.FullPath = Path.GetFullPath(fileName);
        }

        private void BackupLogs()
        {
            DateTime today = DateTime.Today;
            DateTime month = new DateTime(today.Year, today.Month, 1);
            DateTime prev = month.AddMonths(-1);

            string smonth = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(prev.Month);
            string year = prev.Year.ToString();
            string name = smonth + "_" + year;

            string file = name + ".zip";
            string zipPath = Path.Combine(LogPath, file);
            string folderPath = Path.Combine(LogPath, name);

            List<Log> ltemp = m_textLogs.Select(l => l).Where(l => ((TextLog)l).NumericMonth == prev.Month).ToList(); 

            //Create dir, copy files, zip them, remove dir, remove files
            if (!File.Exists(zipPath) && ltemp.Count != 0)
            {
                Directory.CreateDirectory(folderPath);

                foreach (Log l in ltemp)
                {
                    File.Copy(l.FullPath, Path.Combine(folderPath, l.FileName), true);
                }

                ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Optimal, false);
                Directory.Delete(folderPath, true);

                foreach (Log l in ltemp)
                {
                    File.Delete(l.FullPath);
                    m_textLogs.Remove(l);
                }

                // Add new zip to zip list
                AddZipToList(zipPath);
            }
        }

        public List<Log> MonthlyLogs
        {
            get { return m_zips; }
        }

        public Log SelectedLog { get; set; }

        public void Archive()
        {
            if (SelectedLog == null && Period == Period.None)
            {
                LogsNotSelected?.Invoke(this, EventArgs.Empty);
                return;
            }

            Task.Run(() => ArchiveHelper(SelectedLog));
        }

        private void ArchiveHelper(Log log)
        {
            try
            {
                string now = DateTime.Now.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture).Replace('/', '_');
                string zipFile = now + ".zip";
                string zipPath = Path.Combine(ZipPath, zipFile);

                //Check USB inserted. In case it has not been inserted notify user and abort opeation
                List<string> letters = FindAttachedUsbDrives();
                if (letters.Count == 0)
                {
                    // Notify user about missing USB
                    DiskMissing?.Invoke(this, EventArgs.Empty);
                    return;
                }

                if (!Directory.Exists(TmpPath))
                {
                    Directory.CreateDirectory(TmpPath);
                }
                else
                {
                    Directory.Delete(TmpPath, true);
                    Directory.CreateDirectory(TmpPath);
                }

                if (log != null)
                {
                    File.Copy(log.FullPath, Path.Combine(TmpPath, log.FileName), true);
                }

                ArchiveDailyLogs();

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                ZipFile.CreateFromDirectory(TmpPath, zipPath, CompressionLevel.Optimal, false);

                Directory.Delete(TmpPath, true);

                string targetPath = Path.Combine(letters[0] + "\\", zipFile);

                File.Copy(zipPath, targetPath, true);
                File.Delete(zipPath);

               // OperationFinished?.Invoke(this, new OperationFinishedEventArgs(1));
            }
            catch (Exception ex)
            {

                SystemLog.Instance.LogError(ex.Message, ex, SystemLog.LOG_LEVEL.LOG_ERROR);
            }
        }

        private void ArchiveAll()
        {
            foreach (Log l in m_textLogs)
            {
                File.Copy(l.FullPath, Path.Combine(TmpPath, l.FileName), true);
            }
        }

        private void ArchiveNow()
        {
            string d = DateTime.Now.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture).Replace('/', '-');

            Log log = m_textLogs.Select(l => l).Where(l => l.FileNameWOExtension.Contains(d)).FirstOrDefault();

            if (log != null)
            {
                File.Copy(log.FullPath, Path.Combine(TmpPath, log.FileName), true);
            }
        }

        private void ArchivePeriod(int days)
        {
            for (int i = 0; i < days; i++)
            {
                string requiredDay = DateTime.Now.Subtract(TimeSpan.FromDays(i)).ToString("yyyy/MM/dd", CultureInfo.InvariantCulture).Replace('/', '-');

                Log log = m_textLogs.Select(l => l).Where(l => l.FileNameWOExtension.Contains(requiredDay)).FirstOrDefault();

                if (log != null)
                {
                    File.Copy(log.FullPath, Path.Combine(TmpPath, log.FileName), true);
                }
            }
        }

        private void ArchiveDailyLogs()
        {
            if (Period == Period.Day)
            {
                ArchiveNow();
            }
            else if (Period == Period.TwoWeeks)
            {
                ArchivePeriod(15);
            }
            else if (Period == Period.All)
            {
                ArchiveAll();
            }
        }

        private List<string> FindAttachedUsbDrives()
        {
            var foundDriveLetter = String.Empty;
            List<string> disks = new List<string>();

            try
            {
                var searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive where InterfaceType='USB'").Get();

                foreach (ManagementObject drive in searcher)
                {
                    foreach (ManagementObject partition in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition").Get())
                    {
                        foreach (ManagementObject disk in new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                        {
                            foundDriveLetter = disk["Name"].ToString();
                            disks.Add(foundDriveLetter);
                        }
                    }
                }
            }
            catch (ManagementException) { }
            return disks;
        }

        public Period Period
        {
            get { return m_period; }
            set { m_period = value; }
        }
    }
}

