using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LumenisRemoteService
{
    abstract class Action
    {
        protected string[] _args;

        protected Action(string[] args)
        {
            _args = args;
        }

        public string Name { get; set; }

        public abstract void Do();
    }
}
