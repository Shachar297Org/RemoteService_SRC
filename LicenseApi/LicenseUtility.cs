using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aladdin.HASP;

namespace Lumenis.LicenseApi
{
    internal static class LicenseUtility
    {
        private const long WINDOWS_TICK = 10000000;
        private const long UNIX_SEC_TO_WINDOWS = 62135596800;

        /// <summary>
        /// Conversion UNIX seconds to .Net DateTime conversion function
        /// </summary>
        internal static DateTime UnixSecondsToWindowsDate(int unixSeconds)
        {
            return new DateTime((unixSeconds + UNIX_SEC_TO_WINDOWS) * WINDOWS_TICK);
        }

        internal static KeyResult ApiStatusToKeyResult(HaspStatus status)
        {
            System.Diagnostics.Debug.Print("Hasp Error: {0}", status);
            switch (status)
            {
                case HaspStatus.InvalidAddress:
                case HaspStatus.HaspNotFound:
                //case HaspStatus.ContainerNotFound:
                case HaspStatus.KeyIdNotFound:
                    return KeyResult.KeyNotAvialable;

                case HaspStatus.NotEnoughMemory:
                case HaspStatus.SystemError:
                case HaspStatus.DriverNotFound:
                case HaspStatus.InvalidUpdateCounter:
                case HaspStatus.NoBatteryPower:
                case HaspStatus.UpdateNoAckSpace:
                case HaspStatus.FeatureNotImplemented:
                case HaspStatus.NoLog:
                case HaspStatus.LocalCommErr:
                case HaspStatus.UnknownVcode:
                case HaspStatus.InvalidXmlSpec:
                case HaspStatus.InvalidXmlScope:
                case HaspStatus.NoApiDylib:
                case HaspStatus.InvApiDylib:
                case HaspStatus.InvalidObject:
                case HaspStatus.InvalidParameter:
                case HaspStatus.InternalError:
                case HaspStatus.HaspDotNetDllBroken:
                case HaspStatus.NotImplemented:
                case HaspStatus.DeviceError:
                case HaspStatus.SecureChannelError:
                case HaspStatus.CorruptStorage:
                case HaspStatus.VendorLibNotFound:
                case HaspStatus.InvalidVendorLib:
                case HaspStatus.EmptyScopeResults:
                case HaspStatus.VendorlibOld:
                case HaspStatus.NoExtensionBlock:
                case HaspStatus.InvalidPortType:
                case HaspStatus.InvalidPort:
                case HaspStatus.UserDenied:
                case HaspStatus.OperationFailed:
                case HaspStatus.TooManyProducts:
                case HaspStatus.InvalidProduct:
                    return KeyResult.InternalError;

                case HaspStatus.InvalidFeature:
                case HaspStatus.IncompatibleFeature:
                case HaspStatus.DriverTooOld:
                    return KeyResult.CompatibilityError;

                case HaspStatus.TooManyOpenFeatures:
                case HaspStatus.AccessDenied:
                case HaspStatus.BufferTooShort:
                case HaspStatus.NoTime:
                case HaspStatus.InvalidFile:
                case HaspStatus.RequestNotSupported:
                case HaspStatus.InvalidUpdateData:
                case HaspStatus.UpdateNotSupported:
                case HaspStatus.InvalidVendorCode:
                case HaspStatus.EncryptionNotSupported:
                case HaspStatus.InvalidTime:
                case HaspStatus.TooManyUsers:
                case HaspStatus.VMDetected:
                case HaspStatus.TerminalServiceDetected:
                    return KeyResult.UsageError;

                case HaspStatus.InvalidFormat:
                case HaspStatus.InvalidUpdateObject:
                case HaspStatus.UnknownAlgorithm:
                    return KeyResult.InvalidInputFileFormat;

                case HaspStatus.FeatureNotFound:
                case HaspStatus.FeatureExpired:
                case HaspStatus.InvalidSignature:
                    return KeyResult.FeatureNotAvailable;

                case HaspStatus.TooManyKeys:
                    return KeyResult.TooManyKeys;

                case HaspStatus.BrokenSession:
                    return KeyResult.FileClosed;

                case HaspStatus.UpdateBlocked:
                case HaspStatus.UpdateTooOld:
                case HaspStatus.UpdateTooNew:
                    return KeyResult.UpdateError;

                case HaspStatus.TooOldLM:
                case HaspStatus.RemoteCommErr:
                case HaspStatus.RecipientOldLm:
                    return KeyResult.LicenseManagerError;

                case HaspStatus. TimeError:
                case HaspStatus.HardwareModified:
                case HaspStatus.CloneDetected:
                    return KeyResult.TamperingError;

                case HaspStatus.UploadError:
                case HaspStatus.InvalidRecipient:
                case HaspStatus.InvalidDetachAction:
                case HaspStatus.UnknownRecipient:
                case HaspStatus.InvalidDuration:
                case HaspStatus.UpdateAlreadyAdded:
                case HaspStatus.HaspInactive:
                case HaspStatus.NoDetachableFeature:
                case HaspStatus.TooManyHosts:
                case HaspStatus.RehostNotAllowed:
                case HaspStatus.LicenseRehosted:
                case HaspStatus.RehostAlreadyApplied:
                case HaspStatus.CannotReadFile:
                case HaspStatus.ExtensionNotAllowed:
                case HaspStatus.DetachDisabled:
                case HaspStatus.RehostDisabled:
                case HaspStatus.DetachedLicenseFound:
                case HaspStatus.SecureStoreIdMismatch:
                case HaspStatus.AlreadyLoggedIn:
                case HaspStatus.AlreadyLoggedOut:
                    return KeyResult.InternalError;

                case HaspStatus.StatusOk:
                    return KeyResult.Ok;

                default:
                    return KeyResult.InternalError;
            }
        }
    }
}
