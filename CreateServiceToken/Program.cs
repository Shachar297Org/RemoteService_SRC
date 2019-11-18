using System;
using Lumenis.RemoteServiceApi;

namespace Limenis.ServiceToken
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Threading.Thread.Sleep(30000);
            // Parse parameters
            if (args.Length < 1)
            {
                Usage();
                return;
            }

            bool create = true;
            int featureIndex = 0;
            if (args.Length > 1)
            {
                //if (args[0][0] != '-' && args[0][0] != '/')
                //{
                //    Usage();
                //    return;
                //}

                char val = args[0].ToLower().ToCharArray()[0];
                switch (val)
                {
                    case 'c':
                        create = true;
                        break;
                    case 'r':
                        create = false;
                        break;
                    default:
                        Usage();
                        return;
                }
                featureIndex++;
            }
            else
            {
                Usage();
                return;
            }

            ushort featureId = 10001;
            if (!ushort.TryParse(args[featureIndex], out featureId))
            {
                Usage();
                return;
            }

            // connect to remote service to setup or delete token

            try
            {
                RemoteAPI remoteApi = new RemoteAPI();
               
                remoteApi.StartClient();
                remoteApi.StartScreenConnect();
               // remoteApi.Open();
                if (create)
                {
                    remoteApi.CreateFeature(featureId);
                }
                else
                {
                    remoteApi.RemoveFeature(featureId);
                }
                remoteApi.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Cannot create service token: {0}", ex.Message);
            }
           // Console.ReadKey();
        }

        private static void Usage()
        {
            Console.Error.WriteLine("CreateServiceToken [/r|/c] <feature_id>");
        }
    }
}
