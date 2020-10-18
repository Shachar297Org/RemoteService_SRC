using Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LumenisRemoteService
{
    class revokeFeature
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);

        private static readonly ILogger Logger = LoggerFactory.Default.GetCurrentClassLogger();


        private const string MASTERVALUE = "EB0EA102-5808-4C31-B745-A2ABCBFFC4F6";//if inserted it clear the featurToMonitor

        private const int DEFULTFEATURE = 10001;

        private const string SERVICEUSER = "Service";

        private const string CLINICALUSER = "Clinical";

        private static bool _clinicUserWasUsed;

        /// <summary>
        /// feature that add during remoteSerice start time and requested by the platform to be monitored
        /// </summary>
        private static List<int> existingFeature = new List<int>();

        /// <summary>
        /// feature that add during support session
        /// </summary>
        private static List<int> featureToMonitore = new List<int>();
       

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
                string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
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
            Logger.Debug("is in time out");
            //get current user
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            Logger.Debug($"user name is {userName}");
            if (userName.Contains(SERVICEUSER))//verify that currently service user is in used
            {
                Logger.Debug("user name is  service user");
                if (CheckIfShouldLogOff())
                {
                    
                    if (ExitWindowsEx(0, 0))
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
