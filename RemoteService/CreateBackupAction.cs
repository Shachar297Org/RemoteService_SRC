using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumenisRemoteService
{
    class CreateBackupAction : Action
    {
        public CreateBackupAction(string[] args)
            : base(args)
        {
        }

        public static CreateBackupAction MakeAction(string[] args)
        {
            return new CreateBackupAction(args);
        }

        public override void Do()
        {
            
        }
    }
}
