using System;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Net;
using Lumenis.LicenseApi;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.Reflection;

namespace RemoteServiceInstallWixCustom
{
    public class CustomActions
    {
       
        // private const string VOLUME = @"\\?\GLOBALROOT\Device\HarddiskVolume1";
        // const string INSTALL_DIR = @"D:\Program Files\Lumenis\";
        // const string TEMP_APP_DIR = "LSI.tmp";
        //[CustomAction]
        //public static ActionResult CopyToX86Folder(Session session)
        //{
        //    try
        //    {
        //        if (!System.IO.Directory.Exists(@"D:\Program Files\Lumenis\Remote Service\x86"))
        //        {

        //            System.IO.Directory.CreateDirectory(@"D:\Program Files\Lumenis\Remote Service\x86");
        //        }

        //        if (!System.IO.File.Exists(@"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.dll"))
        //        {
        //            //copy
        //            System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\KernelTraceControl.dll", @"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.dll");
        //            System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\KernelTraceControl.Win61.dll", @"D:\Program Files\Lumenis\Remote Service\x86\KernelTraceControl.Win61.dll");
        //            System.IO.File.Copy(@"D:\Program Files\Lumenis\Remote Service\msdia140.dll", @"D:\Program Files\Lumenis\Remote Service\x86\msdia140.dll");
        //        }
        //        return ActionResult.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        return ActionResult.Success;
        //    }
        //}

        public static void Test()
        {
            string error = string.Empty;
            if (CheckHasp())
            {

              
                //session.Log("Hasp OK");
                string compName = BuildComputerName();
             
                // session.Log(string.Format("Comp Name is {0}", compName));

            }
            else
            {
              

            }

            string version;
            string versionCheck;
            CheckImageVersion(null, out version, out versionCheck);
        }

        [CustomAction]
        public static ActionResult CheckPrerequisites(Session session )
        {
            string error = string.Empty;
            //session["HASP_INSERTED"] = "1";
            //session["IMAGE_VERSION_OK"] = "1";
           // return ActionResult.Success;

           // session.Log("Begin CheckPrerequisites");
            //LogRecord("Begin CheckPrerequisites", session);

            //if (UwfApi.IsUwfEnabled())//doesn't work at all on WIN7
            //{
            //    session.Log("Disabling UWF...");
            //    TurnUWFOff();
            //    return ActionResult.SkipRemainingActions;
            //}



            if (CheckHasp(session))
            {

                session["HASP_INSERTED"] = "1";
                //session.Log("Hasp OK");
                string compName = BuildComputerName(session);
                session["COMPUTER_NAME"] = compName;
               // session.Log(string.Format("Comp Name is {0}", compName));
                
            }
            else
            {
                session.Log("Hasp Failed");
                //session["HASP_INSERTED"] = "1";//should be used to debug because when the value is 1 Computer Name label is shown.
                session["HASP_INSERTED"] = "0";
                session["COMPUTER_NAME"] = "Hasp Failed";

            }


            //session["HASP_INSERTED"] = "1";
            //session["COMPUTER_NAME"] = "Test";


            string version;
            string versionCheck;
            CheckImageVersion(session, out version, out versionCheck);
            if(error != string.Empty)
            {
                version = error;
            }
            session["IMAGE_VERSION"] = version;
            session["IMAGE_VERSION_OK"] = versionCheck;
            session["PREREQ_FINISHED"] = "1";

            //session["IMAGE_VERSION"] = "1.1.1";
            //session["IMAGE_VERSION_OK"] = "1";
           // session["HASP_INSERTED"] = "1";
            //session["PREREQ_FINISHED"] = "1";
           



            return ActionResult.Success;
        }

        internal static void TurnUWFOff()
        {
            UwfApi.DisableUWF();
            
            WaitForm waitForm = new WaitForm();
            waitForm.Show();
            Application.DoEvents();

            for (int i = 0, j = 5; i <= 5; i++, j--)
            {
                Thread.Sleep(1000);

                if (i == 1)
                {
                    waitForm.ShowCountdownInfo();
                }

                if (i > 1)
                {
                    waitForm.Countdown(j);
                }

                Application.DoEvents();
            }
            
            System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
        }

        private static void RunApplicationOnceTime()
        { 
            string key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce";

            using (RegistryKey subkey = Registry.LocalMachine.OpenSubKey(key, true))
            {
                string fullName = Assembly.GetEntryAssembly().Location;
                subkey.SetValue("RemoteServiceInstallation", fullName);
            }
        }
        //[CustomAction]
        //public static ActionResult RegisterComponents(Session session)
        //{
        //    try
        //    {
        //        RegisterToGAC();

        //        RegisterCom();

        //        return ActionResult.Success;
        //    }
        //    catch
        //    {

        //        return ActionResult.Failure;
        //    }
        //}
        [CustomAction]
        public static ActionResult ChangeComputerName(Session session)
        {
           
            string currentComputerName = Environment.GetEnvironmentVariable("COMPUTER_NAME");
            string newComputerName = session["COMPUTER_NAME"];
            session.Log($"session COMPUTER_NAME is {newComputerName}. and current computer name is {currentComputerName}");
            if (string.IsNullOrEmpty(currentComputerName) ||
                currentComputerName.ToLower() != newComputerName.ToLower())
            {
                if (WinApi.SetComputerNameEx(WinApi.COMPUTER_NAME_FORMAT.ComputerNamePhysicalDnsHostname, newComputerName))
                {
                    return ActionResult.Success;
                }
                else
                {
                    return ActionResult.Failure;
                }
            }
            return ActionResult.Failure;
        }

        [CustomAction]
        public static ActionResult WcfCommit(Session session)
        {
            // EwfApi.DoEwfCommit(@"\\?\GLOBALROOT\Device\HarddiskVolume1");
            //return ActionResult.Success;

            // UwfApi.UWF_CommitFile(@"C:\");
            // return ActionResult.Success;
            return ActionResult.NotExecuted;
        }

        // THIS VERSION DOES NOT INCLUDE BOMGAR SO IT IS CLOSED
        //[CustomAction]
        //public static ActionResult PrintSession(Session session)
        //{
        //    string bomgar = session["BOMGAR_INSTALLED"];
        //    MessageBox.Show(bomgar);
        //    return ActionResult.Success;
        //}
        
        private static void LogRecord(string message, Session session, InstallMessage installMessage = InstallMessage.Info)
        {
            session?.Message(installMessage, new Record() { FormatString = message });
        }

        // THIS VERSION DOES NOT INCLUDE BOMGAR SO IT IS CLOSED
        //private static bool ConnectionAvailable(string strServer)
        //{
        //    try
        //    {
        //        HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create(strServer);
 
        //        HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponse();
        //        if (HttpStatusCode.OK == rspFP.StatusCode)
        //        {
        //            // HTTP = 200 - Internet connection available, server online
        //            rspFP.Close();
        //            return true;
        //        }
        //        else
        //        {
        //            // Other status - Server or connection not available
        //            rspFP.Close();
        //            return false;
        //        }
        //    }
        //    catch (WebException)
        //    {
        //        // Exception - connection not available
        //        return false;
        //    }
        //}

        private static bool CheckHasp(Session session = null)
        {
          
            try
            {
                SecurityKey securityKey = new SecurityKey();
                if (!securityKey.UseFeature(SecurityKey.MAIN_FEATURE_ID))
                {
                      session["COMPUTER_NAME"] = string.Format("feature {0} not exis", SecurityKey.MAIN_FEATURE_ID) ;
                 
                    return false;
                }
               // session["COMPUTER_NAME"] = "true";
                return true;
            }
            catch (Exception ex)
            {
                session["COMPUTER_NAME"] = ex.Message;
              
                return false;
            }
        }

        private static string BuildComputerName(Session session = null)
        {
            try
            {
                StringBuilder computerName = new StringBuilder(40);
                computerName.Append(SecurityKey.PartNumber).Append(SecurityKey.SerialNumber);
                string partNumber = SecurityKey.PartNumber.TrimEnd(new char[] { '\0' });
                string serialNumber = SecurityKey.SerialNumber.TrimEnd(new char[] { '\0' });
                session.Log($"computer name is {serialNumber + "_" + partNumber}");
                return serialNumber + "_" + partNumber;
            }
            catch(Exception ex)
            {
              
                return ex.Message;
            }
        }

        private static void CheckImageVersion(Session session,out string version, out string versionCheck)
        {
            // versionCheck = "0";
            versionCheck = "1"; // Ignore version check on Windows 10
            session?.Log("Check image version");
            try
            {
                object ver = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).
                    OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion").GetValue("ImageVersion");
                
                if (ver == null)
                {
                    version = "Not set";
                    return;
                }
                else
                {
                    version = ver.ToString();
                }

                //if (version.Length < 6)
                //{
                //    version = "Incompatible version";
                //}
                //else if (version.Substring(6).CompareTo("1.5") < 0)
                //{
                //    return;
                //}
                //versionCheck = "1";
            }
            catch
            {
                version = "Not set";
            }
            session?.Log(string.Format("checked version is {0}",version));
        }

        /// <summary>
        /// Register COM dll as COM object 
        /// </summary>
        private static void RegisterCom()
        {
            try
            {

            }
            catch
            {


            }
        }

       


        // THIS VERSION DOES NOT INCLUDE BOMGAR SO IT IS CLOSED
        //private static bool IsBomgarInstalled()
        //{
        //    bool result = false;
        //    try
        //    {
        //        string[] dirs = Directory.GetDirectories(@"C:\ProgramData", @"bomgar*");

        //        if (dirs.Length > 0)
        //        {
        //            result = true;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        result = false;
        //    }

        //    return result;
        //}
    }

    internal static class WinApi
    {
        public enum COMPUTER_NAME_FORMAT
        {
            ComputerNameNetBIOS,
            ComputerNameDnsHostname,
            ComputerNameDnsDomain,
            ComputerNameDnsFullyQualified,
            ComputerNamePhysicalNetBIOS,
            ComputerNamePhysicalDnsHostname,
            ComputerNamePhysicalDnsDomain,
            ComputerNamePhysicalDnsFullyQualified,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetComputerNameEx(COMPUTER_NAME_FORMAT NameType, string lpBuffer);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern uint GetLastError();
    }

    internal static class EwfApi
    {
        internal enum EWF_CMD
        {
            EWF_NO_CMD = 0,
            EWF_ENABLE,
            EWF_DISABLE,
            EWF_SET_LEVEL,
            EWF_COMMIT
        }

        internal enum EWF_STATE
        {
            EWF_ENABLED,
            EWF_DISABLED
        }

        internal enum EWF_TYPE
        {
            EWF_DISK,
            EWF_RAM,
            EWF_RAM_REG,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct EWF_VOLUME_DESC
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DeviceName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            internal byte[] VolumeID;
        }
        
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct EWF_VOLUME_CONFIG
        {
            internal EWF_TYPE Type;
            internal EWF_STATE State;
            internal EWF_CMD BootCommand;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            internal byte[] PersistentData;
            internal ushort MaxLevels;
            internal uint ClumpSize;
            internal ushort CurrentLevel;

            internal long DiskMapSize;
            internal long DiskDataSize;

            internal long MemMapSize;
            internal EWF_VOLUME_DESC VolumeDesc;
            internal IntPtr LevelDescArray;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct EWF_VOLUME_NAME_ENTRY
        {
            public IntPtr Next;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string Name;
        }

        // define externs for EWF manage functions
        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static IntPtr EwfMgrGetProtectedVolumeConfig(IntPtr hVolume);

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static IntPtr EwfMgrOpenProtected(string volumeName);

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static bool EwfMgrCommit(IntPtr hVolume);

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static bool EwfMgrClose(IntPtr hVolume);

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static IntPtr EwfMgrGetProtectedVolumeList();

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static bool EwfMgrVolumeNameListIsEmpty(IntPtr volumeNameEntryPtr);

        [DllImport("Ewfapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        extern internal static void EwfMgrVolumeNameEntryPop(ref IntPtr volumeNameEntryPtr);

        [DllImport("Kernel32.dll")]
        extern internal static uint GetLastError();

        internal static void DoEwfCommit(string volumeName)
        {
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            IntPtr hVolume = INVALID_HANDLE_VALUE;

            IntPtr volumeNameEntryPtr = EwfApi.EwfMgrGetProtectedVolumeList();
            if (volumeNameEntryPtr == IntPtr.Zero)
            {
                //EventLog.WriteEntry(ServiceName, "EWF failed get volume name entry list", EventLogEntryType.Error);
                return;
            }

            while (!EwfApi.EwfMgrVolumeNameListIsEmpty(volumeNameEntryPtr))
            {
                EwfApi.EWF_VOLUME_NAME_ENTRY volumeNameEntry =
                    (EwfApi.EWF_VOLUME_NAME_ENTRY)Marshal.PtrToStructure(volumeNameEntryPtr, typeof(EwfApi.EWF_VOLUME_NAME_ENTRY));

                // Use the volume name to open a handle to this protected volume.
                hVolume = EwfApi.EwfMgrOpenProtected(volumeNameEntry.Name);
                if (hVolume == INVALID_HANDLE_VALUE)
                {
                    //EventLog.WriteEntry(ServiceName, string.Format("EWF open protected failed, error: {0}", EwfApi.GetLastError()), EventLogEntryType.Error);
                    return;
                }

                // if volume is opened perform commit
                bool bResult = EwfApi.EwfMgrCommit(hVolume);
                if (!bResult)
                {
                    //EventLog.WriteEntry(ServiceName, string.Format("EWF Commit failed, error: {0}", EwfApi.GetLastError()), EventLogEntryType.Error);
                }
                else
                {
                    //EventLog.WriteEntry(ServiceName, "EWF Commit succeeded", EventLogEntryType.Information);
                }

                EwfApi.EwfMgrClose(hVolume);

                // get next volume entry
                EwfApi.EwfMgrVolumeNameEntryPop(ref volumeNameEntryPtr);
            }
        }

        internal static bool IsEnabled(string volumeName)
        {
            IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            IntPtr hVolume = INVALID_HANDLE_VALUE;

            IntPtr volumeNameEntryPtr = EwfApi.EwfMgrGetProtectedVolumeList();
            if (volumeNameEntryPtr == IntPtr.Zero)
            {
                return false;
            }

            while (!EwfMgrVolumeNameListIsEmpty(volumeNameEntryPtr))
            {
                EWF_VOLUME_NAME_ENTRY volumeNameEntry =
                    (EWF_VOLUME_NAME_ENTRY)Marshal.PtrToStructure(volumeNameEntryPtr, typeof(EWF_VOLUME_NAME_ENTRY));

                // Use the volume name to open a handle to this protected volume.
                hVolume = EwfMgrOpenProtected(volumeNameEntry.Name);
                if (hVolume == INVALID_HANDLE_VALUE)
                {
                    return false;
                }

                IntPtr ewfVolumeConfigPtr = EwfMgrGetProtectedVolumeConfig(hVolume);
                if (ewfVolumeConfigPtr == IntPtr.Zero)
                {
                    return false;
                }

                EWF_VOLUME_CONFIG ewfVolumeConfig =
                    (EWF_VOLUME_CONFIG)Marshal.PtrToStructure(ewfVolumeConfigPtr, typeof(EWF_VOLUME_CONFIG));

                // close volume
                EwfMgrClose(hVolume);

                return ewfVolumeConfig.State == EWF_STATE.EWF_ENABLED;
            }
            
            return false;
        }
    }

    internal static class UwfApi
    {
        #region Private Vars
        private static EventLog applicationLog = new EventLog("Application");
        
        private static ManagementScope wmiScope = new ManagementScope(@"\\localhost\root\StandardCimv2\embedded")
        {
            Options = new ConnectionOptions
            {
                Impersonation = ImpersonationLevel.Impersonate,
                Authentication = AuthenticationLevel.Default,
                EnablePrivileges = true
            }
        };
        #endregion

        static UwfApi()
        {
            wmiScope.Connect();
            applicationLog.Source = "Application";
        }

        internal static void DisableUWF()
        {
            try
            {
                if (wmiScope.IsConnected)
                {
                    using (ManagementClass mc = new ManagementClass(wmiScope.Path.Path, "UWF_Filter", null))
                    {
                        //next line failes with Access Denied under normal user account
                        ManagementObjectCollection moc = mc.GetInstances();
                        foreach (ManagementObject mo in moc)
                        {
                            uint hresult = (uint)mo.InvokeMethod("ResetSettings", null);
                            if (hresult != 0)
                            {
                                throw new InvalidOperationException("UWF Disable failed.");
                            }

                            hresult = (uint)mo.InvokeMethod("Disable", null);
                            if (hresult != 0)
                            {
                                throw new InvalidOperationException("UWF ResetSettings failed.");
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("WMI is disconnected.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool IsVolumeProtected(string volume)
        {
            try
            {
                if (wmiScope.IsConnected)
                {
                    using (ManagementClass mc = new ManagementClass(wmiScope.Path.Path, "UWF_Volume", null))
                    {
                        //next line failes with Access Denied under normal user account
                        ManagementObjectCollection moc = mc.GetInstances();
                        foreach (ManagementObject mo in moc)
                        {
                            string DriveLetter = (string)mo.GetPropertyValue("DriveLetter");
                            string VolumeName = (string)mo.GetPropertyValue("VolumeName");
                            bool Protected = (bool)mo.GetPropertyValue("Protected");

                            applicationLog.WriteEntry(string.Format("VolumeName: {0} , Protected: {1}", VolumeName, Protected), EventLogEntryType.Information);
                            
                            if (VolumeName.Equals(volume, StringComparison.OrdinalIgnoreCase))
                            {
                                return Protected;
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("WMI is disconnected.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }

        internal static bool UWF_CommitFile(string fileFullPath)
        {
            bool res = false;
            
            try
            {
                string driveLetter = Path.GetPathRoot(fileFullPath).Substring(0, 2);

                if (string.IsNullOrWhiteSpace(driveLetter))
                {
                    return false;
                }

                if (wmiScope.IsConnected)
                {
                    using (ManagementClass mc = new ManagementClass(wmiScope.Path.Path, "UWF_Volume", null))
                    {
                        //next line failes with Access Denied under normal user account
                        ManagementObjectCollection moc = mc.GetInstances();
                        foreach (ManagementObject mo in moc)
                        {
                            string UWF_DriveLetter = (string)mo.GetPropertyValue("DriveLetter");
                            
                            if (UWF_DriveLetter.Equals(driveLetter, StringComparison.OrdinalIgnoreCase))
                            {
                                uint hresult = 0;

                                if (Path.IsPathRooted(fileFullPath))
                                {
                                    hresult = (uint)mo.InvokeMethod("CommitFile", new object[] { fileFullPath.Substring(2) });
                                }
                                else
                                {
                                    hresult = (uint)mo.InvokeMethod("CommitFile", new object[] { fileFullPath });
                                }
                                
                                if (hresult != 0)
                                {
                                    applicationLog.WriteEntry(string.Format("UWF CommitFile failed. File: {0} Error: {1}", fileFullPath, hresult), EventLogEntryType.Error);
                                    res = false;
                                }
                                else
                                {
                                    applicationLog.WriteEntry(string.Format("CommitFile succeded. File: {0}", fileFullPath), EventLogEntryType.Information);
                                    res = true;
                                }
                                break;
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("WMI is disconnected.");
                }
            }
            catch (Exception ex)
            {
                applicationLog.WriteEntry(string.Format("UWF_CommitFile Error occured: {0} ", ex.Message), EventLogEntryType.Error);
                throw ex;
            }

            return res;
        }
        
       
        internal static bool IsUwfEnabled()
        {
            try
            {
                if (wmiScope.IsConnected)
                {
                    using (ManagementClass mc = new ManagementClass(wmiScope.Path.Path, "UWF_Filter", null))//this line cause to exception in win10
                    {
                        //next line failes with Access Denied under normal user account
                        ManagementObjectCollection moc = mc.GetInstances();
                        foreach (ManagementObject mo in moc)
                        {
                            bool UWFstate = (bool)mo.GetPropertyValue("CurrentEnabled");
                            return UWFstate;
                        }
                    }
                }
                else
                {
                    //throw new InvalidOperationException("WMI is disconnected.");
                }
            }
            catch (Exception ex)
            {
                // applicationLog.WriteEntry(string.Format("IsUwfEnabled error: {0}", ex.Message), EventLogEntryType.Error);
                //throw ex;
            }

            return false;
        }
    }
}
