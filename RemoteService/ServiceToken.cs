using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace LumenisRemoteService
{
    public class ServiceToken
    {
        private readonly TimeSpan _defaultTokenLeaseTime = new TimeSpan(1, 0, 0);
        private Dictionary<int, DateTime> _installedFeatureDates = new Dictionary<int, DateTime>();
        private static ServiceToken _instance = null;

        private ServiceToken()
        {
        }

        public static ServiceToken Instance()
        {
            if (_instance == null)
            {
                _instance = new ServiceToken();
            }
            return _instance;
        }

        public void Create(int featureId)
        {
            _installedFeatureDates[featureId] = DateTime.Now;
            revokeFeature.AddFeature(featureId);
        }

        public bool Exists(int featureId)
        {
            if (_installedFeatureDates.Count(f => f.Key == featureId) == 0)
            {
                return false;
            }
            if (_installedFeatureDates[featureId] + _defaultTokenLeaseTime > DateTime.Now)
            {
                return true;
            }

            // if feature expired remove it
            _installedFeatureDates.Remove(featureId);
            return false;
        }

        public void Remove(int featureId)
        {
            try
            {
                _installedFeatureDates.Remove(featureId);
                revokeFeature.RemoveFeature(featureId);
            }
            catch
            {
                // ignore
            }
        }

        public bool Extend(int featureId)
        {
            if (_installedFeatureDates.Count(f => f.Key == featureId) == 0)
            {
                return false;
            }
            _installedFeatureDates[featureId] = DateTime.Now;
            return true;
        }
    }
}
