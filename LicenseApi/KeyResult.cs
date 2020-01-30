using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lumenis.LicenseApi
{
    public enum KeyResult
    {
        Ok,

        KeyNotAvialable,
        InternalError,
        CompatibilityError,
        UsageError,
        InvalidInputFileFormat,
        FeatureNotAvailable,
        TooManyKeys,
        FileClosed,
        BufferTooShort,
        NotEnoughSpace,
        UpdateError,
        LicenseManagerError,
        TamperingError,
        HardwareModified,
    }

}
