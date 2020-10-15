using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aladdin.HASP;

namespace Lumenis.LicenseApi
{
    /// <summary>
    /// A class defining work API to a key memory
    /// </summary>
    public class KeyMemoryFile : IDisposable
    {
        private Hasp _hasp;
        private HaspFile _haspFile;
        private int _size;
        private bool _isLoggedIn;
        private int _startingOffset;
        
        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="hasp"></param>
        /// <param name="startingOffset"></param>
        internal KeyMemoryFile(Hasp hasp, int startingOffset)
        {
            _isLoggedIn = true;
            _hasp = hasp;
            _haspFile = hasp.GetFile(HaspFileId.ReadWrite);
            HaspStatus status = hasp.GetFile(HaspFileId.ReadWrite).FileSize(ref _size);
            _startingOffset = startingOffset;
            _haspFile.FilePos = _startingOffset;
        }

        public KeyResult Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        public KeyResult Read(byte[] buffer, int offset, int size)
        {
            KeyResult result = CheckParameters(buffer, offset, size);
            if (result != KeyResult.Ok)
            {
                return result;
            }

            HaspStatus status = _haspFile.Read(buffer, offset, size);
            result = LicenseUtility.ApiStatusToKeyResult(status);
            //result = LicenseUtility.ApiStatusToKeyResult(_haspFile.Read(buffer, offset, size));
            if (result == KeyResult.Ok)
            {
                _haspFile.FilePos += size;
            }

            //return KeyResult.Ok;
            return result;
        }

        public KeyResult Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        public KeyResult Write(byte[] buffer, int offset, int size)
        {
            KeyResult result = CheckParameters(buffer, offset, size);
            if (result != KeyResult.Ok)
            {
                return result;
            }

            result = LicenseUtility.ApiStatusToKeyResult(_haspFile.Write(buffer, offset, size));
            if (result == KeyResult.Ok)
            {
                _haspFile.FilePos += size;
            }

            return KeyResult.Ok;
        }

        public int FilePos
        {
            get { return _haspFile.FilePos - _startingOffset; }
            set { _haspFile.FilePos = value + _startingOffset; }
        }

        public int Size
        {
            get { return _size - _startingOffset; }
        }

        public void Close()
        {
            Dispose();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_isLoggedIn)
            {
                _haspFile.Dispose();
                _hasp.Logout();
                _isLoggedIn = false;
            }
        }

        #endregion

        private KeyResult CheckParameters(byte[] buffer, int offset, int size)
        {
            if (!_isLoggedIn)
            {
                return KeyResult.FileClosed;
            }

            if (buffer.Length - offset < size)
            {
                return KeyResult.BufferTooShort;
            }
            if (size > _size - _haspFile.FilePos)
            {
                return KeyResult.NotEnoughSpace;
            }
            return KeyResult.Ok;
        }
    }
}
