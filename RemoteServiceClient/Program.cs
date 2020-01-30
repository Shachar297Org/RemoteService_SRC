using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lumenis.RemoteServiceApi
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RemoteAPI remoteApi = new RemoteAPI();
                remoteApi.Open();
            }
            catch (Exception ex)
            {

               
            }
        }
    }
}
