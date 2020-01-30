using System.Runtime.InteropServices;

namespace COM
{
   // [Guid("3B74840F-8E16-4C17-888C-F6F2AC5EEC8A"), InterfaceType(ComInterfaceType.InterfaceIsDual)]
   // [ComVisible(true)]
    public interface ICom
    {
        event ComLib.StatusDel _newStatusArrived;

        bool RequestSupport();
        bool StopSupport();
    }
}