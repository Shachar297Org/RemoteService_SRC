using Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace LumenisRemoteService
{
    [Flags]
    public enum ExitWindows : uint
    {
        // ONE of the following:
        LogOff = 0x00,
        ShutDown = 0x01,
        Reboot = 0x02,
        PowerOff = 0x08,
        RestartApps = 0x40,
        // plus AT MOST ONE of the following two:
        Force = 0x04,
        ForceIfHung = 0x10,
    }

    [Flags]
    public enum ShutdownReason : uint
    {
        None = 0,

        MajorApplication = 0x00040000,
        MajorHardware = 0x00010000,
        MajorLegacyApi = 0x00070000,
        MajorOperatingSystem = 0x00020000,
        MajorOther = 0x00000000,
        MajorPower = 0x00060000,
        MajorSoftware = 0x00030000,
        MajorSystem = 0x00050000,

        MinorBlueScreen = 0x0000000F,
        MinorCordUnplugged = 0x0000000b,
        MinorDisk = 0x00000007,
        MinorEnvironment = 0x0000000c,
        MinorHardwareDriver = 0x0000000d,
        MinorHotfix = 0x00000011,
        MinorHung = 0x00000005,
        MinorInstallation = 0x00000002,
        MinorMaintenance = 0x00000001,
        MinorMMC = 0x00000019,
        MinorNetworkConnectivity = 0x00000014,
        MinorNetworkCard = 0x00000009,
        MinorOther = 0x00000000,
        MinorOtherDriver = 0x0000000e,
        MinorPowerSupply = 0x0000000a,
        MinorProcessor = 0x00000008,
        MinorReconfig = 0x00000004,
        MinorSecurity = 0x00000013,
        MinorSecurityFix = 0x00000012,
        MinorSecurityFixUninstall = 0x00000018,
        MinorServicePack = 0x00000010,
        MinorServicePackUninstall = 0x00000016,
        MinorTermSrv = 0x00000020,
        MinorUnstable = 0x00000006,
        MinorUpgrade = 0x00000003,
        MinorWMI = 0x00000015,

        FlagUserDefined = 0x40000000,
        FlagPlanned = 0x80000000
    }
    public sealed class TokenAdjuster
    {
        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();
        // PInvoke stuff required to set/enable security privileges
        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_ADJUST_PRIVILEGES = 0X00000020;
        private const int TOKEN_QUERY = 0X00000008;
        private const int TOKEN_ALL_ACCESS = 0X001f01ff;
        private const int PROCESS_QUERY_INFORMATION = 0X00000400;

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
        private static extern int OpenProcessToken(
            IntPtr ProcessHandle, // handle to process
            int DesiredAccess, // desired access to process
            ref IntPtr TokenHandle // handle to open access token
            );

        [DllImport("kernel32", SetLastError = true),
         SuppressUnmanagedCodeSecurity]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int AdjustTokenPrivileges(
            IntPtr TokenHandle,
            int DisableAllPrivileges,
            IntPtr NewState,
            int BufferLength,
            IntPtr PreviousState,
            ref int ReturnLength);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool LookupPrivilegeValue(
            string lpSystemName,
            string lpName,
            ref LUID lpLuid);

        public static bool EnablePrivilege(string lpszPrivilege, bool bEnablePrivilege)
        {
            Logger.Debug("EnablePrivilege");
            bool retval = false;
            int ltkpOld = 0;
            IntPtr hToken = IntPtr.Zero;
            TOKEN_PRIVILEGES tkp = new TOKEN_PRIVILEGES();
            tkp.Privileges = new int[3];
            TOKEN_PRIVILEGES tkpOld = new TOKEN_PRIVILEGES();
            tkpOld.Privileges = new int[3];
            LUID tLUID = new LUID();
            tkp.PrivilegeCount = 1;
            try
            {
                if (bEnablePrivilege)
                    tkp.Privileges[2] = SE_PRIVILEGE_ENABLED;
                else
                    tkp.Privileges[2] = 0;
                if (LookupPrivilegeValue(null, lpszPrivilege, ref tLUID))
                {
                    Logger.Debug("LookupPrivilegeValuer");
                    Process proc = Process.GetCurrentProcess();
                    if (proc.Handle != IntPtr.Zero)
                    {
                        Logger.Debug("LookupPrivilegeValuer not zero");
                        if (OpenProcessToken(proc.Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
                            ref hToken) != 0)
                        {
                            tkp.PrivilegeCount = 1;
                            tkp.Privileges[2] = SE_PRIVILEGE_ENABLED;
                            tkp.Privileges[1] = tLUID.HighPart;
                            tkp.Privileges[0] = tLUID.LowPart;
                            const int bufLength = 256;
                            IntPtr tu = Marshal.AllocHGlobal(bufLength);
                            Marshal.StructureToPtr(tkp, tu, true);
                            if (AdjustTokenPrivileges(hToken, 0, tu, bufLength, IntPtr.Zero, ref ltkpOld) != 0)
                            {
                                Logger.Debug("successful AdjustTokenPrivileges");
                                // successful AdjustTokenPrivileges doesn't mean privilege could be changed
                                if (Marshal.GetLastWin32Error() == 0)
                                {
                                    retval = true; // Token changed
                                }
                            }
                            TOKEN_PRIVILEGES tokp = (TOKEN_PRIVILEGES)Marshal.PtrToStructure(tu, typeof(TOKEN_PRIVILEGES));
                            Marshal.FreeHGlobal(tu);
                        }
                    }
                }
                if (hToken != IntPtr.Zero)
                {
                    Logger.Debug("LookupPrivilegeValuer not zero");
                    CloseHandle(hToken);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return retval;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            internal int LowPart;
            internal int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES
        {
            private LUID Luid;
            private int Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TOKEN_PRIVILEGES
        {
            internal int PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            internal int[] Privileges;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct _PRIVILEGE_SET
        {
            private int PrivilegeCount;
            private int Control;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)] // ANYSIZE_ARRAY = 1
            private LUID_AND_ATTRIBUTES[] Privileges;
        }

    }
        class revokeFeature
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ExitWindowsEx(ExitWindows uFlags, ShutdownReason dwReason);

        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();


        private const string MASTERVALUE = "EB0EA102-5808-4C31-B745-A2ABCBFFC4F6";//if inserted it clear the featurToMonitor

        private const int DEFULTFEATURE = 10001;

        private const string SERVICEUSER = "serviceuser";

        private const string CLINICALUSER = "clinicaluser";

        private static bool _clinicUserWasUsed;

        /// <summary>
        /// feature that add during remoteSerice start time and requested by the platform to be monitored
        /// </summary>
        private static List<int> existingFeature = new List<int>();



        /// <summary>
        /// feature that add during support session
        /// </summary>
        private static List<int> featureToMonitore = new List<int>();


        public static event Action<bool> _ClearAllFeatures;

        public static string GetProcessOwner(int processId)
        {
            try
            {
                string query = "Select * From Win32_Process Where ProcessID = " + processId;
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        return argList[1] + "\\" + argList[0];
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return "NO OWNER";
        }
        public static string GetProcessOwner(string processName)
        {
            try
            {
                string query = "Select * from Win32_Process Where Name = \"" + processName + "\"";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
                ManagementObjectCollection processList = searcher.Get();

                foreach (ManagementObject obj in processList)
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        string owner = argList[1] + "\\" + argList[0];
                        return owner;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return "NO OWNER";
        }

        public static string GetUserName()
        {
            Logger.Debug("starting ConnectWiseController");
            System.Diagnostics.Process[] proces = System.Diagnostics.Process.GetProcesses();
            if (proces != null && proces.Length > 0)
            {
                foreach (System.Diagnostics.Process p in proces)
                {
                    Logger.Debug($"process name is {p.ProcessName}");
                    var owner = GetProcessOwner(p.Id);
                    Logger.Debug($"owner  name is {owner}");
                    if (owner.ToLower().Contains(SERVICEUSER))
                    {
                        Logger.Information($"service user owner");
                        return SERVICEUSER;
                        
                    }
                    if (owner.ToLower().Contains(CLINICALUSER))
                    {
                        Logger.Information($"clinic user owner");
                        return CLINICALUSER;
                    }

                }
            }
            return string.Empty;
        }

        public static void revokeFeatureInit()
        {
            existingFeature.Add(DEFULTFEATURE);
            Logger.Debug($"add default feature {DEFULTFEATURE}");
            Logger.Information($"feature {DEFULTFEATURE} is monitored");
            //form config fill featureToMonitore
            try
            {
               
                var result = Convert.ToString(ConfigurationManager.AppSettings["RevokeFeature"]);
                Logger.Debug($"RevokeFeature value in config is {result}");
                if (result != null)
                {
                    if (result.Contains(','))//expect to more then 1 feature to monitor
                    {
                        try
                        {
                            string[] list = result.Split(',');
                            if (list != null)
                            {
                                foreach (string s in list)
                                {
                                    if (int.TryParse(s, out int val))
                                    {
                                        if (!existingFeature.Contains(val))
                                        {
                                            existingFeature.Add(val);
                                            Logger.Information($"feature {val} is monitored");
                                        }

                                    }
                                }
                            }
                            else
                            {
                                Logger.Debug("monitor feature list from config is  empty");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "fail to parse RevokeFeature values. only value 10001 will be monitored");
                        }
                    }
                    else if (result.Equals(MASTERVALUE))
                    {
                        existingFeature.Clear();// no feature will be monitored and no log off will take place
                    }
                    else// in case only 1 feature should be monitored
                    {
                        if (int.TryParse(result, out int val))
                        {
                            if (!existingFeature.Contains(val))
                                existingFeature.Add(val);
                            Logger.Information($"feature {val} is monitored");
                        }
                    }
                }
                else
                {
                    Logger.Error("no feature is monitored except default 10001");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

        }
        public static void AddFeature(int p_feature)
        {
            Logger.Debug($"add feature {p_feature}");
            if (!_clinicUserWasUsed)// when first feature is injected via serviceToken we check if clinic user is active
            {
                Logger.Debug("not clinic user");
                string userName = GetUserName();
                Logger.Debug($"user name is {userName}");
                if (userName.Contains(CLINICALUSER))//verify that currently clinic user is in used
                {
                    _clinicUserWasUsed = true;
                    Logger.Information("injecting feature while in clinicUser. the system will activate log off mechanism");
                }
            }
            if (!featureToMonitore.Contains(p_feature))
            {
                Logger.Debug($"feature to monitor contains {p_feature}");
                featureToMonitore.Add(p_feature);
            }
        }

        public static void RemoveFeature(int p_feature)
        {
            Logger.Debug($"remove feature {p_feature}");
            if (p_feature == DEFULTFEATURE) return;// default feature can't be removed
            if (featureToMonitore.Contains(p_feature))
            {
                featureToMonitore.Remove(p_feature);
            }
        }

        /// <summary>
        /// signal that remote service session started
        /// </summary>
        

        public static void IsInTimeOut()
        {
            return;
            Logger.Debug("is in time out");
            //get current user
            string userName = GetUserName();
            Logger.Debug($"user name is {userName}");
            if (userName.Contains(SERVICEUSER))//verify that currently service user is in used
            {
                Logger.Debug("user name is  service user");
                if (CheckIfShouldLogOff())
                {

                    if (ExitWindows(LumenisRemoteService.ExitWindows.Reboot,LumenisRemoteService.ShutdownReason.MajorOther))
                    {
                        Logger.Debug("exit service account");
                        featureToMonitore.Clear();
                        _clinicUserWasUsed = true;
                        Logger.Information("log off");
                    }

                    else
                        Logger.Error("failed to log off");
                }

               
            }
            //logout if in service and 10001 is monitored

        }

        public static bool ExitWindows(ExitWindows uFlags, ShutdownReason dwReason)
        {
            if (TokenAdjuster.EnablePrivilege("SeShutdownPrivilege", true))
            {
                Logger.Debug("SeShutdownPrivilege success");
                
                return ExitWindowsEx(uFlags, dwReason);
                
            }
            return false;


        }

        private static bool CheckIfShouldLogOff()
        {
           // bool _isServiceHaspInserted = false;
            //if (!_isServiceHaspInserted)// if service hasp inserted this process is redundant
            //{
                if (_clinicUserWasUsed)// if clinicuser was never in used this process is redundant
                {
                    foreach (int feature in existingFeature)
                    {
                        if (featureToMonitore.Contains(feature))
                        {
                        _ClearAllFeatures(true);
                        Logger.Debug("need to log off");
                        return true;
                        }
                    }
                    Logger.Information("no feature was found to be monitored for logoff operation");
                    return false;// if no feature was found in featureToMonitore list
                }
            Logger.Debug("no need to log off. clinic user was not in used");
            return false;
            //  }

            // return false;
        }

      

        
    }

   

    }
