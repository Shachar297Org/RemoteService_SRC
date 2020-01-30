using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Aladdin.HASP;
using System.Diagnostics;


namespace Lumenis.LicenseApi
{
    public class KeyInstaller
    {
        private const int MAIN_FEATURE_ID = 1001;
        private const int INSTALL_FEATURE_ID = 1002;
        private const int UPDATE_FEATURE_ID = 1005;
        private const int SMALL_HL_SIZE = 112;

        /// <summary>
        /// Installs/Updates a feature license to a key using license file name
        /// </summary>
        /// <param name="v2cFileName"></param>
        /// <returns></returns>
        public bool InstallLicense(string v2cFileName)
        {
            if (!File.Exists(v2cFileName))
            {
                // license file does not exist
                return false;
            }

            using (FileStream fileStream = File.OpenRead(v2cFileName))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                return InstallLicense(reader);
            }
        }

        /// <summary>
        /// Installs/Updates a feature license to a key using stream
        /// </summary>
        /// <param name="v2cStream"></param>
        /// <returns></returns>
        public bool InstallLicense(StreamReader v2cStream)
        {
            try
            {
                string ack = null;
                HaspStatus haspStatus = Hasp.Update(v2cStream.ReadToEnd(), ref ack);
                if (haspStatus != HaspStatus.StatusOk)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                //Debug.Print("SecurityKey.InstallLicense exception: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Copies memory from the installation / upgrade HASP to the main HASP
        /// </summary>
        /// <returns></returns>
        public KeyResult CopyMemory()
        {
            byte[] buffer = new byte[SMALL_HL_SIZE];
            KeyMemoryFile memoryFile = null;

            // Read RO memory from the install HASP
            SecurityKey securityKey = new SecurityKey(INSTALL_FEATURE_ID);
            KeyResult keyResult = securityKey.ReadRom(buffer, 0);
            if (keyResult == KeyResult.FeatureNotAvailable)
            {
                // if Install feature is not available try to read from update feature RO memory
                securityKey = new SecurityKey(UPDATE_FEATURE_ID);
                keyResult = securityKey.ReadRom(buffer, 0);
            }
            
            if (keyResult != KeyResult.Ok)
            {
                return keyResult;
            }

            // Copy to the main HASP
            securityKey = new SecurityKey(MAIN_FEATURE_ID);
            keyResult = securityKey.GetMemoryFile(out memoryFile);
            if (keyResult != KeyResult.Ok)
            {
                return keyResult;
            }
            memoryFile.Write(buffer);
            memoryFile.Close();

            return KeyResult.Ok;
        }
    }
}
