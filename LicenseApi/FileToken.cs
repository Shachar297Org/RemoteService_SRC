using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ServiceProcess;
using Lumenis.RemoteServiceApi;

namespace Lumenis.LicenseApi
{
    internal class FileToken
    {
        public static KeyResult CheckFeature(int featureId, out bool checkFeature)
        {
            checkFeature = false;

            try
            {
                ServiceController controller = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "RemoteService");

                if (controller == null || controller.Status != ServiceControllerStatus.Running)
                {
                    return KeyResult.KeyNotAvialable;
                    //throw new InvalidOperationException("The Lumenis Remote Service is not installed");
                }

                RemoteAPI remoteApi = new RemoteAPI();
                remoteApi.Open();
                checkFeature = remoteApi.HasFeature(featureId);
                remoteApi.Close();

                return checkFeature ? KeyResult.Ok : KeyResult.KeyNotAvialable;
            }
            catch
            {
                return KeyResult.KeyNotAvialable;
            }
        }

        public static KeyResult RemoveFeature(int featureId)
        {
            var result = KeyResult.Ok;
            try
            {
                ServiceController controller = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == "Lumenis Remote Service");

                if (controller == null || controller.Status != ServiceControllerStatus.Running)
                {
                    result = KeyResult.KeyNotAvialable;
                }
                else
                {
                    var remoteApi = new RemoteAPI();
                    remoteApi.Open();
                    if (remoteApi.HasFeature(featureId))
                    {
                        remoteApi.RemoveFeature(featureId);
                        result = KeyResult.Ok;
                    }
                    else
                    {
                        result = KeyResult.KeyNotAvialable;
                    }
                    remoteApi.Close();
                }
            }
            catch
            {
                result = KeyResult.KeyNotAvialable;
            }
            return result;
        }
    }
}
