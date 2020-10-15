using System.Runtime.InteropServices;

namespace COMM
{
    public interface ICom
    {
        event ComLib.StatusDel _newStatusArrived;
        bool RequestSupport();
        bool StopSupport();
    }
}