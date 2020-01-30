using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Aladdin.HASP;
using System.Xml.Linq;
using System.Diagnostics;

namespace Lumenis.LicenseApi
{
    /// <summary>
    /// Defines available feature types
    /// </summary>
    public enum FeatureType
    {
        None = 0,
        Countable,
        Trial,
        ExpirationDate,
        Perpetual,
    }

    /// <summary>
    /// Defines feature status
    /// </summary>
    public class FeatureStatus
    {
        /// <summary>
        /// The ID of the feature as defined by EMS
        /// </summary>
        public int FeatureId;

        /// <summary>
        /// The type of the feature
        /// </summary>
        public FeatureType FeatureType;

        /// <summary>
        /// The available feature counts, for non-countable features this value is MaxInt
        /// </summary>
        public int Count;

        /// <summary>
        /// Feature expiration date
        /// <remarks>For features that don't use expiration dates or
        /// uninitialized trial features this values is max date
        /// </remarks>
        /// </summary>
        public DateTime ExpirationDate;

        /// <summary>
        /// Internal constructor building the feature status from an XML node
        /// </summary>
        /// <param name="element">An XML node containing feature information</param>
        internal FeatureStatus(XElement element)
        {
            this.FeatureId = int.Parse(element.Attribute("id").Value);
            this.FeatureType = SecurityKey.FeatureTypeNameToType(element.Attribute("license_type").Value);
            Count = FeatureType == LicenseApi.FeatureType.Countable ?
                int.Parse(element.Attribute("counter_fix").Value) - int.Parse(element.Attribute("counter_var").Value) :
                int.MaxValue;

            this.ExpirationDate = DateTime.MaxValue;
            if (FeatureType == LicenseApi.FeatureType.ExpirationDate)
            {
                ExpirationDate = LicenseUtility.UnixSecondsToWindowsDate(int.Parse(element.Attribute("exp_date").Value));
            }
            else if (FeatureType == LicenseApi.FeatureType.Trial && element.Attribute("time_start").Value != "uninitialized")
            {
                ExpirationDate = LicenseUtility.UnixSecondsToWindowsDate(int.Parse(element.Attribute("time_start").Value) + 
                    int.Parse(element.Attribute("total_time").Value));
            }
        }

        public FeatureStatus(int featureId)
        {
            FeatureId = featureId;
            FeatureType = FeatureType.None;
            ExpirationDate = DateTime.MaxValue;
            Count = int.MaxValue;
        }
    }

    public class KeyStatus
    {
        public string Id;
        public string KeyModel;
        public FeatureStatus[] FeatureStatuses;
    }

    /// <summary>
    /// Class providing license protection usage functionality
    /// </summary>
    public class SecurityKey
    {
        public const int MAIN_FEATURE_ID = 1001;
        public const int PERSONAL_HASP_FEATURE_ID = 1003;

        private const int PART_NUMBER_POS = 0;
        private const int PART_NUMBER_SIZE = 20;
        private const int SERIAL_NUMBER_POS = 20;
        private const int SERIAL_NUMBER_SIZE = 20;
        private const int FULL_NAME_POS = 0;
        private const int FULL_NAME_SIZE = 20;
        private const int PERSONAL_ID_POS = 20;
        private const int PERSONAL_ID_SIZE = 20;
        private const int MANUFACTURING_DATE_POS = 40;
        private const int MANUFACTURING_DATE_SIZE = 20;


        private const string vendorCode =
            "MTPAa6TFH6ZJU8uYGRDnAItugBQoCwPIs8IqfPiXZFJgi9o8WIwv715rZBdoZbDGpa8u/jYcmjbh3hnQ" +
            "pBjCNXMXW4pwyEmvwRWFWwxqEW/WKPiwA9p3z97+x/scjUlxaLSUP9OUm/CDL5wg3tnu3cp19M78HFeu" +
            "fVvYiZk418tLWM2fCVoOLkVpXh+OQ/MqGnGtaO+30glCS+UvqiDVSWoGcrOfDOpdyfOjf38PfC80Shz7" +
            "y+OYlr7YyKKjg1clFtqzH4LI7vlfJDfIKi/84yFiF8WIiVFkO/SrW6k6RWacyFjUrVSTo3cdkTL0CjMM" +
            "9eeIbg0SVyIdcSAD5zDIxQprctbXAs/QvFmitYxMxmN6bE2lT3wefh9e0M85cQJdIBMutKIxGoaX/RgT" +
            "An5VvNYB4kvyWL7uGd0BZIUyAQ7R2fFIdPMe1RUM8BOuknKsEbVQSAS/0WzJ4PQp8Ceb2TGZowSZh7kQ" +
            "Jd34DsTNfX1KxQPQGNC7izLUsTk9KeRP7MvYMdzzpERnrHg5pbl2dglxAUlkdrn2bhmnoYYfNQxtsbdM" +
            "YhPdF8jtFboYDcINqU1Wx+rEBYChu/PG8mkJ9khBqmQxM/y7A2UA82oaYy49Jw2Kj2EZhRNX2F9JMSiX" +
            "NMrjVWMa8iuB0Sc3lH6ZHFSNcEt51WD1dExjAvjUvQUSLWIzEvWMJjLGPrPMrCEBV2hwi9vuBC7fucM/" +
            "f2Lmt0aqxifAUWlthQGmFFsEhJPePIvEuL/ebRuRINFQmh+4Lg3YzU+44JpkFyUUgbvdcwSwdWkplSff" +
            "VyNG/c/v3lB8VPPiOLWUSYgkvmgM6WmzTDFpdzbiP5ZqByLyomMwik4mKa7lDahgAVtJNesp2JBHCqIW" +
            "u+oFE3aInUM1x3AWz6uoLthi6lQkMCPWtDFZ97ur/odqmpn3G/he3DE/65yL/EFR/ZO0R1WIjmr97/ds" +
            "cOj0GPCzJLxeNseRGMDS8A==";
        //private const string vendorCode =
        //    "AzIceaqfA1hX5wS+M8cGnYh5ceevUnOZIzJBbXFD6dgf3tBkb9cvUF/Tkd/iKu2fsg9wAysYKw7RMAsV" +
        //    "vIp4KcXle/v1RaXrLVnNBJ2H2DmrbUMOZbQUFXe698qmJsqNpLXRA367xpZ54i8kC5DTXwDhfxWTOZrB" +
        //    "rh5sRKHcoVLumztIQjgWh37AzmSd1bLOfUGI0xjAL9zJWO3fRaeB0NS2KlmoKaVT5Y04zZEc06waU2r6" +
        //    "AU2Dc4uipJqJmObqKM+tfNKAS0rZr5IudRiC7pUwnmtaHRe5fgSI8M7yvypvm+13Wm4Gwd4VnYiZvSxf" +
        //    "8ImN3ZOG9wEzfyMIlH2+rKPUVHI+igsqla0Wd9m7ZUR9vFotj1uYV0OzG7hX0+huN2E/IdgLDjbiapj1" +
        //    "e2fKHrMmGFaIvI6xzzJIQJF9GiRZ7+0jNFLKSyzX/K3JAyFrIPObfwM+y+zAgE1sWcZ1YnuBhICyRHBh" +
        //    "aJDKIZL8MywrEfB2yF+R3k9wFG1oN48gSLyfrfEKuB/qgNp+BeTruWUk0AwRE9XVMUuRbjpxa4YA67SK" +
        //    "unFEgFGgUfHBeHJTivvUl0u4Dki1UKAT973P+nXy2O0u239If/kRpNUVhMg8kpk7s8i6Arp7l/705/bL" +
        //    "Cx4kN5hHHSXIqkiG9tHdeNV8VYo5+72hgaCx3/uVoVLmtvxbOIvo120uTJbuLVTvT8KtsOlb3DxwUrwL" +
        //    "zaEMoAQAFk6Q9bNipHxfkRQER4kR7IYTMzSoW5mxh3H9O8Ge5BqVeYMEW36q9wnOYfxOLNw6yQMf8f9s" +
        //    "JN4KhZty02xm707S7VEfJJ1KNq7b5pP/3RjE0IKtB2gE6vAPRvRLzEohu0m7q1aUp8wAvSiqjZy7FLaT" +
        //    "tLEApXYvLvz6PEJdj4TegCZugj7c8bIOEqLXmloZ6EgVnjQ7/ttys7VFITB3mazzFiyQuKf4J6+b/a/Y";

        private const string keySearchScope =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
            "<haspscope>" +
            "    <hasp type=\"HASP-HL\" >" +
            "        <license_manager hostname=\"localhost\" />" +
            "    </hasp>" +
            "</haspscope>";

        private const string rtcSearchPrefix =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
            "<haspscope>" +
            "    <hasp id=\"";
        private const string rtcSearchPostfix =
            // hasp id string + concat the next lines
            "\" />" +
            "</haspscope>" +
            "";

        // the feature ID used for detecting the HASP
        private int _mainFeatureId;

        public SecurityKey()
        {
            _mainFeatureId = 0;
        }

        public SecurityKey(int featureId)
        {
            if (featureId < 0)
            {
                throw new ArgumentException("The feature ID cannot be negative", "fetureId");
            }
            _mainFeatureId = featureId;
        }

        /// <summary>
        /// Returns the part number stored on the device
        /// </summary>
        public static string PartNumber
        {
            get
            {
                byte[] buffer = new byte[PART_NUMBER_SIZE];

                if (new SecurityKey(MAIN_FEATURE_ID).ReadRom(buffer, PART_NUMBER_POS) == KeyResult.Ok)
                {
                    return ASCIIEncoding.Default.GetString(buffer).TrimEnd(new char[] { '\0' });
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the serial number of the device
        /// </summary>
        public static string SerialNumber
        {
            get
            {
                byte[] buffer = new byte[SERIAL_NUMBER_SIZE];

                if (new SecurityKey(MAIN_FEATURE_ID).ReadRom(buffer, SERIAL_NUMBER_POS) == KeyResult.Ok)
                {
                    return ASCIIEncoding.Default.GetString(buffer).TrimEnd(new char[] { '\0' });
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the manufacturing date of the device
        /// </summary>
        public static string ManufacturingDate
        {
            get
            {
                byte[] buffer = new byte[MANUFACTURING_DATE_SIZE];

                if (new SecurityKey(MAIN_FEATURE_ID).ReadRom(buffer, MANUFACTURING_DATE_POS) == KeyResult.Ok)
                {
                    return ASCIIEncoding.Default.GetString(buffer).TrimEnd(new char[] { '\0' });
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the full name stored on the personal hasp
        /// </summary>
        public static string FullName
        {
            get
            {
                byte[] buffer = new byte[FULL_NAME_SIZE];

                if (new SecurityKey(PERSONAL_HASP_FEATURE_ID).ReadRom(buffer, FULL_NAME_POS) == KeyResult.Ok)
                {
                    return ASCIIEncoding.Default.GetString(buffer);
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the personal ID stored on the personal hasp
        /// </summary>
        public static string PersonalId
        {
            get
            {
                byte[] buffer = new byte[PERSONAL_ID_SIZE];

                if (new SecurityKey(PERSONAL_HASP_FEATURE_ID).ReadRom(buffer, PERSONAL_ID_POS) == KeyResult.Ok)
                {
                    return ASCIIEncoding.Default.GetString(buffer);
                }
                return "";
            }
        }

        /// <summary>
        /// Returns the array of available feature IDs found on all attached keys
        /// </summary>
        /// <returns>The array of available feature IDs</returns>
        public int[] GetFeatureList()
        {
            KeyResult keyResult;
            XElement resultRoot;

            GetFeaturesRoot(out keyResult, out resultRoot);
            if (keyResult != KeyResult.Ok)
            {
                return new int[0];
            }

            try
            {
                return resultRoot.Elements("feature").
                    Select(elem => int.Parse(elem.Attribute("id").Value)).Distinct().ToArray();
            }
            catch (Exception ex)
            {
                Debug.Print("SecurityKey.GetFeatureList exception: {0}", ex.Message);
                // return an empty array in case of any exception
                return new int[0];
            }
        }

        /// <summary>
        /// Gets a type of a requested feature.
        /// If more then one feature with the same ID is found
        /// the returned type corresponds to the maximal usage type
        /// </summary>
        /// <param name="featureId">An ID of a feature</param>
        /// <returns>A type of a feature</returns>
        public FeatureType GetFeatureType(int featureId)
        {
            FeatureType featureType = FeatureType.None;

            KeyResult keyResult;
            XElement resultRoot;

            GetFeaturesRoot(out keyResult, out resultRoot);
            if (keyResult != KeyResult.Ok)
            {
                return featureType;
            }

            var featureTypeNames = resultRoot.Elements("feature").
                Where(elem => elem.Attribute("id").Value == featureId.ToString()).
                Select(elem => elem.Attribute("license_type").Value);

            foreach (string featureTypeName in featureTypeNames)
            {
                FeatureType currFeatureType = FeatureTypeNameToType(featureTypeName);
                featureType = (featureType < currFeatureType) ? currFeatureType : featureType;
            }

            return featureType;
        }

        /// <summary>
        /// Checks whether specific feature exists
        /// </summary>
        /// <param name="featureId">An ID of a feature</param>
        /// <param name="checkFeature">True if feature with given ID exists, false - otherwise</param>
        /// <returns>An operation status</returns>
        public KeyResult CheckFeature(int featureId, out bool checkFeature)
        {
            KeyResult keyResult;
            XElement resultRoot;

            GetFeaturesRoot(out keyResult, out resultRoot);
            if (keyResult != KeyResult.Ok)
            {
                checkFeature = false;
            }
            else
            {
                checkFeature = resultRoot.Elements("feature").
                    Count(elem => elem.Attribute("id").Value == featureId.ToString()) > 0;
            }

            if (keyResult != KeyResult.Ok || !checkFeature)
            {
                // if HASP not inserted or has no required feature try file token
                KeyResult keyResult2 = FileToken.CheckFeature(featureId, out checkFeature);
                if (keyResult2 == KeyResult.Ok)
                {
                    keyResult = keyResult2;
                }
            }
            
            return keyResult;
        }

        /// <summary>
        /// Checks if a feature is available for usage and uses it (e.g. reduces available usage count)
        /// </summary>
        /// <param name="featureId">An ID of the feature</param>
        /// <returns>True if a feature exists and is available for usage</returns>
        public bool UseFeature(int featureId)
        {
            HaspFeature feature = HaspFeature.FromFeature(featureId);
            Hasp hasp = new Hasp(feature);
            HaspStatus haspStatus = hasp.Login(vendorCode);

            bool result = haspStatus == HaspStatus.StatusOk;
            hasp.Logout();

            if (!result)
            {
                // check file token . currently for Service Mode Usage
                if (FileToken.CheckFeature(featureId, out result) != KeyResult.Ok)
                {
                    return false;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a feature is available for usage and uses it (e.g. reduces available usage count)
        /// NOT USING THE REMOTE SERVICE HERE!
        /// </summary>
        /// <param name="featureId">An ID of the feature</param>
        /// <param name="result">out param - HASP status - in string format (not to force the HASP windows dll to be forced in reference</param>
        public void UseFeature(int featureId, out string result)
        {
            HaspFeature feature = HaspFeature.FromFeature(featureId);
            Hasp hasp = new Hasp(feature);
            HaspStatus haspStatus = hasp.Login(vendorCode);
            hasp.Logout();
            
            result = haspStatus.ToString();
        }

        /// <summary>
        /// Tries to use the real time clock of the hasp, and return the status in string (not to force reference to the hasp windows dll
        /// added to test the hasp battary status.
        /// </summary>
        
        public string GetTimeHaspStatus(string haspSerialNumber)
        {
            var feature = HaspFeature.FromFeature(0);
            var hasp = new Hasp(feature);
            var scope = rtcSearchPrefix + haspSerialNumber + rtcSearchPostfix;
            var haspStatus = hasp.Login(vendorCode, scope);

            DateTime time = DateTime.Now;
            var status = hasp.GetRtc(ref time);
            hasp.Logout();
            return status.ToString();
        }

        /// <summary>
        /// Gets the status of the feature
        /// </summary>
        /// <param name="featureId">An ID of the feature</param>
        /// <param name="featureStatus">The feature status or null if feature does not exist</param>
        /// <returns>An operation status</returns>
        public KeyResult GetFeatureStatus(int featureId, out FeatureStatus featureStatus)
        {
            featureStatus = null;

            KeyResult keyResult;
            XElement resultRoot;

            GetFeaturesRoot(out keyResult, out resultRoot);
            if (keyResult != KeyResult.Ok)
            {
                bool checkFeature;
                // if HASP not inserted or has no required feature try file token
                KeyResult keyResult2 = FileToken.CheckFeature(featureId, out checkFeature);
                if (keyResult2 == KeyResult.Ok)
                {
                    featureStatus = new FeatureStatus(featureId);
                    keyResult = keyResult2;
                }
                return keyResult;
            }

            try
            {
                var featureStatuses = resultRoot.Elements("feature").
                    Where(elem => elem.Attribute("id").Value == featureId.ToString()).
                    Select(elem => new FeatureStatus(elem));

                foreach (FeatureStatus tempFatureStatus in featureStatuses)
                {
                    if (featureStatus == null || featureStatus.FeatureType < tempFatureStatus.FeatureType)
                    {
                        featureStatus = tempFatureStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("SecurityKey.GetFeatureStatus exception: {0}", ex.Message);
                return KeyResult.InternalError;
            }

            return featureStatus == null ? KeyResult.FeatureNotAvailable : KeyResult.Ok;
        }

        /// <summary>
        /// Returns the feature statuses of all accessible security key 
        /// </summary>
        /// <returns></returns>
        public KeyStatus[] GetKeyStatuses()
        {
            XElement haspRoot;
            KeyResult keyResult;
            GetHaspRoot(out keyResult, out haspRoot);

            if (keyResult != KeyResult.Ok)
            {
                return new KeyStatus[0];
            }

            List<KeyStatus> keyStatuses = new List<KeyStatus>();
            foreach (XElement haspElement in haspRoot.Elements())
            {
                KeyStatus keyStatus = new KeyStatus();
                keyStatus.Id = haspElement.Attribute("id").Value;
                keyStatus.KeyModel = haspElement.Attribute("key_model").Value;
                int[] featureIds = haspElement.Elements("feature").
                    Select(elem => int.Parse(elem.Attribute("id").Value)).Distinct().ToArray();
                keyStatus.FeatureStatuses = GetFeturesStatuses(featureIds);

                keyStatuses.Add(keyStatus);
            }
            return keyStatuses.ToArray();
        }

        /// <summary>
        /// Returns statuses of all features 
        /// </summary>
        /// <param name="featureIds">An array with feature ID's</param>
        /// <returns>The array of statuses of all found features</returns>
        public FeatureStatus[] GetFeturesStatuses(int[] featureIds)
        {
            List<FeatureStatus> featureStatuses = new List<FeatureStatus>(featureIds.Length);
            for (int i = 0; i < featureIds.Length; i++)
            {
                FeatureStatus featureStatus;
                GetFeatureStatus(featureIds[i], out featureStatus);
                if (featureStatus != null)
                {
                    featureStatuses.Add(featureStatus);
                }
            }
            return featureStatuses.ToArray();
        }

        /// <summary>
        /// Gets the memory file corresponding to a specific feature
        /// </summary>
        /// <param name="featureId">An ID of a feature </param>
        /// <param name="memoryFile">An object of the memory file</param>
        /// <returns>An operation status</returns>
        public KeyResult GetMemoryFile(out KeyMemoryFile memoryFile)
        {
            return GetMemoryFileImpl(out memoryFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryFile"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private KeyResult GetMemoryFileImpl(out KeyMemoryFile memoryFile)
        {
            memoryFile = null;
            if (_mainFeatureId == 0)
            {
                return KeyResult.UsageError;
            }

            HaspFeature feature = HaspFeature.FromFeature(_mainFeatureId);
            Hasp hasp = new Hasp(feature);
            HaspStatus haspStatus = hasp.Login(vendorCode);
            if (haspStatus != HaspStatus.StatusOk)
            {
                return LicenseUtility.ApiStatusToKeyResult(haspStatus);
            }

            memoryFile = new KeyMemoryFile(hasp, 0);
            return KeyResult.Ok;
        }

        /// <summary>
        /// Gets the XML root element containing the features for the key containing the specific feature
        /// </summary>
        /// <param name="keyResult"></param>
        /// <param name="resultRoot"></param>
        private void GetHaspFeaturesRoot(int featureId, out KeyResult keyResult, out XElement resultRoot)
        {
            GetHaspRoot(out keyResult, out resultRoot);
            if (resultRoot != null)
            {
                resultRoot = resultRoot.Elements().SelectMany(elem => elem.Elements("feature")).
                    Where(elem => elem.Attribute("id").Value == _mainFeatureId.ToString()).Ancestors().FirstOrDefault();
            }
            if (resultRoot == null)
            {
                keyResult = KeyResult.KeyNotAvialable;
            }
        }

        /// <summary>
        /// Gets the XML root element containing the HASP keys
        /// </summary>
        /// <param name="keyResult"></param>
        /// <param name="resultRoot"></param>
        private static void GetHaspRoot(out KeyResult keyResult, out XElement resultRoot)
        {
            string featureRequestFormat =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                "<haspformat root=\"hasp_info\">" +
                "    <hasp>" +
                "        <attribute name=\"id\" />" +
                "        <attribute name=\"key_model\" />" +
                "        <feature>" +
                "            <attribute name=\"id\" />" +
                "            <attribute name=\"license\" />" +
                "        </feature>" +
                "    </hasp>" +
                "</haspformat>";

            string featureInfo = null;

            HaspStatus status = Hasp.GetInfo(keySearchScope, featureRequestFormat, vendorCode, ref featureInfo);
            keyResult = LicenseUtility.ApiStatusToKeyResult(status);

            resultRoot = null;
            if (keyResult == KeyResult.Ok)
            {
                resultRoot = XElement.Parse(featureInfo);
            }
        }

        /// <summary>
        /// Gets the XML root element containing the features for all connected keys
        /// </summary>
        /// <param name="keyResult"></param>
        /// <param name="resultRoot"></param>
        private void GetGenericFeaturesRoot(out KeyResult keyResult, out XElement resultRoot)
        {
            string featureRequestFormat =
                "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>" +
                "<haspformat root=\"hasp_info\">" +
                "    <feature>" +
                "        <attribute name=\"id\" />" +
                "        <attribute name=\"license\" />" +
                "    </feature>" +
                "</haspformat>";

            string featureInfo = null;

            HaspStatus status = Hasp.GetInfo(keySearchScope, featureRequestFormat, vendorCode, ref featureInfo);
            keyResult = LicenseUtility.ApiStatusToKeyResult(status);

            resultRoot = null;
            if (keyResult == KeyResult.Ok)
            {
                resultRoot = XElement.Parse(featureInfo);
            }
        }

        /// <summary>
        /// Gets the XML root element containing the features
        /// </summary>
        /// <param name="keyResult"></param>
        /// <param name="resultRoot"></param>
        private void GetFeaturesRoot(out KeyResult keyResult, out XElement resultRoot)
        {
            if (_mainFeatureId == 0)
            {
                GetGenericFeaturesRoot(out keyResult, out resultRoot);
            }
            else
            {
                GetHaspFeaturesRoot(_mainFeatureId, out keyResult, out resultRoot);
            }
        }

        /// <summary>
        /// Reads the ROM
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="filePos"></param>
        /// <returns></returns>
        internal KeyResult ReadRom(byte[] buffer, int filePos)
        {
            Hasp hasp = new Hasp(HaspFeature.FromFeature(_mainFeatureId));
            HaspStatus haspStatus = hasp.Login(vendorCode);
            if (haspStatus != HaspStatus.StatusOk)
            {
                return LicenseUtility.ApiStatusToKeyResult(haspStatus);
            }

            HaspFile haspFile = hasp.GetFile(HaspFileId.ReadOnly);
            haspFile.FilePos = filePos;
            haspFile.Read(buffer, 0, buffer.Length);
            haspFile.Dispose();
            return KeyResult.Ok;
        }

        /// <summary>
        /// Gets a feature type by a feature name
        /// </summary>
        /// <param name="featureTypeName"></param>
        /// <returns></returns>
        internal static Lumenis.LicenseApi.FeatureType FeatureTypeNameToType(string featureTypeName)
        {
            switch (featureTypeName)
            {
                case "perpetual":
                    return LicenseApi.FeatureType.Perpetual;

                case "expiration":
                    return LicenseApi.FeatureType.ExpirationDate;
                
                case "trial":
                    return LicenseApi.FeatureType.Trial;
                
                case "executions":
                    return LicenseApi.FeatureType.Countable;
                
                default:
                    return LicenseApi.FeatureType.None;
            }
        }

        public void StopVirtualFeature(int featureId)
        {
            string result;
            UseFeature(featureId, out result);
            // feature is not found locally
            if (result != HaspOkStatus)
            {
                FileToken.RemoveFeature(featureId);
            }
        }

        public string HaspOkStatus
        {
            get
            {
                return HaspStatus.StatusOk.ToString();
            }
        }
    }
}
