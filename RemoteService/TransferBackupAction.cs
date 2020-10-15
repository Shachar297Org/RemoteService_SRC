using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WinSCP;

namespace LumenisRemoteService
{
    class TransferBackupAction : Action
    {
        private const string HOST_NAME = "support.lumenis.com";
        private const string USER_NAME = "deviceuser";
        private const string PASSWORD = "Dev@User1";

        private string fileToUpload;

        public TransferBackupAction(string[] args)
            : base(args)
        {
            if (args.Length == 0)
            {
                throw new ArgumentException("No input file is specified");
            }

            // set file name
            fileToUpload = args[0];
        }

        public static TransferBackupAction MakeAction(string[] args)
        {
            return new TransferBackupAction(args);
        }


        public override void Do()
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                FtpSecure = FtpSecure.ExplicitSsl,
                HostName = HOST_NAME,
                UserName = USER_NAME,
                Password = PASSWORD
            };

            using (Session session = new Session())
            {
                // Connect
                session.Open(sessionOptions);

                // Upload files
                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;

                TransferOperationResult transferResult;
                transferResult = session.PutFiles(@"d:\toupload\*", "/home/user/", false, transferOptions);

                // Throw on any error
                transferResult.Check();

                // Print results
                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                }
            }
        }
    }
}
