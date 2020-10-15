using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace LumenisRemoteService
{
    class ActionDispatcher
    {
        // Constants
        private const string JUMP_CLIENT_SERVICE_NAME_PREFIX = "bomgar";

        // Instance object
        private static ActionDispatcher _instance = null;

        // Table for converting action name to action maker
        public delegate Action MakeActionDelegate(string[] args);
        private readonly Dictionary<string, MakeActionDelegate> _actions = new Dictionary<string, MakeActionDelegate>()
        {
            {"createbackup", CreateBackupAction.MakeAction},
            {"transferbackup", TransferBackupAction.MakeAction},
        };

        private ServiceController _bomgarService = null;
        private bool _isEnabled = true;
        private Thread _runActionsThread;
        private bool _isActionThreadRunning;
        private ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private ActionDispatcher()
        {
            FindJumpClientService();
            Enable(true);
        }

        public static ActionDispatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ActionDispatcher();
                }
                return _instance;
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
        }

        private void FindJumpClientService()
        {
            try
            {
                _bomgarService = ServiceController.GetServices().
                    FirstOrDefault(s => s.ServiceName.ToLower().StartsWith(JUMP_CLIENT_SERVICE_NAME_PREFIX));
            }
            catch (Exception ex)
            {
               // EventLog.WriteEntry(
                 //   LumenisService.Name, string.Format("FindJumpClietService Exception: {0}", ex.Message), EventLogEntryType.Error);
            }
        }

        public bool IsRunning()
        {
            if (_bomgarService == null)
            {
                return false;
            }
            _bomgarService.Refresh();
          //  EventLog.WriteEntry(
            //    LumenisService.Name, string.Format("Bomgar Service Status = {0}", _bomgarService.Status), EventLogEntryType.Information);
            return _bomgarService.Status == ServiceControllerStatus.Running;
        }

        public void StartService()
        {
            if (_bomgarService == null || IsRunning() || !_isEnabled)
            {
                return;
            }
            try
            {
                _bomgarService.Start();
            }
            catch (Exception)
            {
                //EventLog.WriteEntry(
                //    LumenisService.SERVICE_NAME, string.Format("StartService Exception: {0}", ex.Message), EventLogEntryType.Error);
            }
        }

        public void StopService()
        {
            if (_bomgarService == null || !IsRunning())
            {
                return;
            }
            try
            {
                _bomgarService.Stop();
            }
            catch (Exception)
            {
                //EventLog.WriteEntry(
                //    LumenisService.SERVICE_NAME, string.Format("StopService Exception: {0}", ex.Message), EventLogEntryType.Error);
            }
        }

        public void Enable(bool enable)
        {
            _isEnabled = enable;
            if (!_isEnabled)
            {
                StartActionThread();
            }
            else
            {
                // if connection is disabled - force service stopping
                StopService();
                StopActionThread();
            }
        }

        public void TakeAction(string actionName, string args)
        {
            try
            {
                MakeActionDelegate makeAction = _actions[actionName.ToLower()];
                Action action = makeAction(args.Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                _actionQueue.Enqueue(action);
            }
            catch
            {
                // action not found
            }
        }

        private void StartActionThread()
        {
            _isActionThreadRunning = true;
            _runActionsThread = new Thread(new ThreadStart(ActionThread));
            _runActionsThread.Start();
        }

        private void StopActionThread()
        {
            _isActionThreadRunning = false;
            if (_runActionsThread != null)
            {
                _runActionsThread.Join(500);
            }
        }

        private void ActionThread()
        {
            while (_isActionThreadRunning)
            {
                Action action;
                if (_actionQueue.TryDequeue(out action))
                {
                    // start action in a new task
                    Task.Factory.StartNew(action.Do);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
