using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using FantaLog.Properties;
using CommonTypes.Objects;
using System.Collections.ObjectModel;
using CommonTypes.ValueTypes;

namespace FantaLog
{
    public interface IErrorData
    {
    }

    internal class PartialErrorMessage : IErrorData
    {
        public string Message { get; private set; }
        public PartialErrorMessage(string p_message)
        {
            Message = p_message;
        }
    }

    internal class FullErrorMEssage : IErrorData
    {
        public string Message { get; private set; }
        public Exception Error { get; private set; }
        public FullErrorMEssage(string p_message, Exception p_error)
        {
            Message = p_message;
            Error = p_error;
        }
    }


    public class LogBase
    {
        public enum LOG_LEVEL { LOG_DEBUG = 0, LOG_INFO, LOG_WARN, LOG_ERROR };

        public enum LOG_MSG { ERROR_EVENT = 0, EXPIRED_ERROR_EVENT };
    }

    public abstract class AbsLog: LogBase
    {
        protected static readonly object instanceLock = new object();
        protected const string LOG_CONFIGURATION_NAME = "SystemLog.config";
      
       
        protected  ILog logger;
        protected  string LOG_FILE;
        protected Queue<Tuple<LOG_LEVEL, IErrorData, bool>> logQueu;
        protected Thread logThread;

        protected AbsLog()
        {
            string applicationName = Environment.GetCommandLineArgs()[0];
            string currentFolder = Environment.CurrentDirectory;
            string pathToLogConfig = Path.Combine(currentFolder, LOG_CONFIGURATION_NAME);
            XmlConfigurator.Configure(new FileInfo(pathToLogConfig));

            logQueu = new Queue<Tuple<LOG_LEVEL, IErrorData, bool>>();
            InitThread();

        }

        public abstract void LogString(string message, LOG_LEVEL messageLevel, bool ShowtoUser);


        public abstract  void LogString(string message, LOG_LEVEL messageLevel);

        public abstract void InitThread();

        protected abstract void AddToLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple);
        

        protected abstract void AddToSystemLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple);



        protected abstract void AddToLog(Tuple<LOG_LEVEL, IErrorData> logTuple);
       

        protected virtual void ThreadSystemWork(object sender)
        {
            try
            {
                while (true)
                {
                    while (logQueu.Count > 0)
                    {
                        try
                        {
                            AddToSystemLog(logQueu.Dequeue());
                        }
                        catch (Exception)
                        {
                            break;
                        }
                    }
                    System.Threading.Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("System Log thread exception: {0}", ex);
            }
        }

        protected  string GetMessage(Tuple<LOG_LEVEL, IErrorData, bool> logTuple)
        {
            LOG_LEVEL level = logTuple.Item1;
            string message = null;
            if (logTuple.Item2 is PartialErrorMessage)
            {
                var obj = logTuple.Item2 as PartialErrorMessage;
                if (obj != null)
                {
                  return  message = obj.Message;
                }
            }
            else
            {
                var obj = logTuple.Item2 as FullErrorMEssage;
                logger.Error(obj.Message, obj.Error);
               
            }
            return null;
        }

        protected string GetMessage(Tuple<LOG_LEVEL, IErrorData> logTuple)
        {
            LOG_LEVEL level = logTuple.Item1;
            string message = null;
            if (logTuple.Item2 is PartialErrorMessage)
            {
                var obj = logTuple.Item2 as PartialErrorMessage;
                if (obj != null)
                {
                    return message = obj.Message;
                }
            }
            else
            {
                var obj = logTuple.Item2 as FullErrorMEssage;
                logger.Error(obj.Message, obj.Error);

            }
            return null;
        }

        protected virtual void WriteTraceLog(LOG_LEVEL p_level, string p_message)
        {
            switch (p_level)
            {
                case LOG_LEVEL.LOG_DEBUG:
                    logger.Debug(p_message);
                    break;
                case LOG_LEVEL.LOG_INFO:
                    logger.Info(p_message);
                    break;
                case LOG_LEVEL.LOG_WARN:
                    logger.WarnFormat(p_message);
                    break;
                case LOG_LEVEL.LOG_ERROR:
                    logger.ErrorFormat(p_message);
                    break;
                default:
                    logger.Debug(p_message);
                    break;
            }
        }
    }

    public sealed class DebugLog : AbsLog
    {
       
        private static DebugLog lsLog = null;
        private readonly string LOGGER_NAME;
        private DebugLog()
        {
            LOGGER_NAME = "DebugLog";
            LOG_FILE = "D:\\Logs\\DebugLogFile_";

            logger = LogManager.GetLogger(LOGGER_NAME);
            log4net.GlobalContext.Properties["DebugLogName"] = LOG_FILE;
           

        }

        public static DebugLog Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (lsLog == null)
                        lsLog = new DebugLog();
                }
                return lsLog;
            }
        }
        public override void InitThread()
        {
            logThread = new Thread(ThreadSystemWork);
            logThread.Name = "Debug Log Thread";
            logThread.IsBackground = true;
            logThread.Start();
        }

        

        public override void LogString(string message, LOG_LEVEL messageLevel)
        {
            var obj = new PartialErrorMessage(message);
            logQueu.Enqueue(new Tuple<LOG_LEVEL, IErrorData, bool>(messageLevel, obj, false));
        }

        public override void LogString(string message, LOG_LEVEL messageLevel, bool ShowtoUser)
        {
            var obj = new PartialErrorMessage(message);
            logQueu.Enqueue(new Tuple<LOG_LEVEL, IErrorData, bool>(messageLevel, obj, ShowtoUser));
        }

        protected override void AddToLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple)
        {
            throw new NotImplementedException();
        }

        protected override void AddToLog(Tuple<LOG_LEVEL, IErrorData> logTuple)
        {
            throw new NotImplementedException();
        }

        protected override void AddToSystemLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple)
        {
            LOG_LEVEL level = logTuple.Item1;
            string message = GetMessage(logTuple);
            if (message == null)
            {
                return;
            }
            WriteTraceLog(level, message);
        }


    }

    public sealed class SystemLog : AbsLog
    {
        public event EventHandler<RecordsEventArgs> RecordsArrived;

       
       // private const string LOG_FILE = "D:\\Logs\\FantaLogFile_";

       

      //  private const string LOGGER_NAME = "FantaLog";
        private const int TIMEOUT = 10000;  // 10 seconds.

        private static SystemLog lsLog = null;
       
        private List<Tuple<LOG_LEVEL, IErrorData>> msgList;
        private List<Tuple<LOG_LEVEL, IErrorData>> msgRequestList;
        //private List<Tuple<LOG_LEVEL, string,bool>> systemErrorList;
       
        private object lockRequestBuffer;
        private Thread loggingThread;
       
        private AutoResetEvent autoEvent;
        private ObservableCollection<LogRecord> m_records = new ObservableCollection<LogRecord>();
        private readonly string LOGGER_NAME;

        private SystemLog() 
        {
            LOGGER_NAME = "FantaLog";
            LOG_FILE = "D:\\Logs\\FantaLogFile_";

            logger = LogManager.GetLogger(LOGGER_NAME);
            log4net.GlobalContext.Properties["LogName"] = LOG_FILE;
          
            msgList = new List<Tuple<LOG_LEVEL, IErrorData>>();
            msgRequestList = new List<Tuple<LOG_LEVEL, IErrorData>>();
            // systemErrorList = new List<Tuple<FantaLog.SystemLog.LOG_LEVEL, string, bool>>();
          
        }

        public static SystemLog Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (lsLog == null)
                        lsLog = new SystemLog();
                }
                return lsLog;
            }
        }

        public void LogError(string p_message, Exception p_ex, LOG_LEVEL messageLevel)
        {
            var obj = new FullErrorMEssage(p_message, p_ex);
            logQueu.Enqueue(new Tuple<LOG_LEVEL, IErrorData, bool>(messageLevel, obj, false));
        }

        /// <summary>
        /// write the log only to the log file without displaying it to the user. default parameters can't be used in c++ cli.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageLevel"></param>
        /// <param name="ShowtoUser"></param>
        public override  void LogString(string message, LOG_LEVEL messageLevel, bool ShowtoUser)
        {
           
            if (ShowtoUser)
            {
                LogString(message,messageLevel);
            }
            else
            {
                var obj = new PartialErrorMessage(message);
                logQueu.Enqueue(new Tuple<LOG_LEVEL, IErrorData, bool>(messageLevel, obj, ShowtoUser));
            }
          
        }

        public void LogMessage(LOG_MSG msgNum, LOG_LEVEL messageLevel, bool ShowtoUser)
        {
            if (ShowtoUser)
            {
                LogString(Resources.ResourceManager.GetString("LOG" + msgNum.ToString()), messageLevel);
            }
            else
            {
                LogString(Resources.ResourceManager.GetString("LOG" + msgNum.ToString()), messageLevel, ShowtoUser);

            }
           
        }



        public override void LogString(string message, LOG_LEVEL messageLevel)
        {
           // return;
            lock (lockRequestBuffer)
            {
                var obj = new PartialErrorMessage(message);
                msgRequestList.Add(new Tuple<LOG_LEVEL, IErrorData>(messageLevel, obj));
            }
            autoEvent.Set();
        }

        public void LogMessage(LOG_MSG msgNum, LOG_LEVEL messageLevel)
        {
            LogString(Resources.ResourceManager.GetString("LOG" + msgNum.ToString()), messageLevel);
        }

        public void LogWithParameterBefore(int param, LOG_MSG msgNum, LOG_LEVEL messageLevel)
        {
            LogString(param.ToString() + Resources.ResourceManager.GetString("LOG" + ((int)msgNum).ToString()), messageLevel);
        }

        public void LogWithParameterAfter(LOG_MSG msgNum, int param, LOG_LEVEL messageLevel)
        {
            LogString(Resources.ResourceManager.GetString("LOG" + ((int)msgNum).ToString()) + param.ToString(), messageLevel);
        }

        public override void InitThread()
        {
            try
            {
                lockRequestBuffer = new object();
                autoEvent = new AutoResetEvent(false);
                loggingThread = new Thread(ThreadWork);
                loggingThread.Name = "Log Thread";
                loggingThread.IsBackground = true;
                loggingThread.Start();

                logThread = new Thread(ThreadSystemWork);
                logThread.Name = "System Log Thread";
                logThread.IsBackground = true;
                logThread.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
       
        private void ThreadWork(object sender)
        {
            try
            {
                while (true)
                {
                    // Waits for incoming log message. 
                    // There is timeout in order to flush messages that came while another message was handled.
                    autoEvent.WaitOne(TIMEOUT);
                    
                    lock (lockRequestBuffer)
                    {
                        msgList.AddRange(msgRequestList);
                        
                        msgRequestList.Clear();
                    }

                    foreach (var logTuple in msgList)
                        AddToLog(logTuple);

                    
                   
                    msgList.Clear();
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Log thread exception: {0}", e);
            }
        }
        protected override void AddToLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple)
        {
            AddToLog(new Tuple<FantaLog.SystemLog.LOG_LEVEL, IErrorData>(logTuple.Item1, logTuple.Item2));
        }

        protected override void AddToSystemLog(Tuple<LOG_LEVEL, IErrorData, bool> logTuple)
        {
            LOG_LEVEL level = logTuple.Item1;
            string message = GetMessage(logTuple);
            if (message == null)
            {
                return;
            }

            WriteTraceLog(level,message);
        }

        protected override void AddToLog(Tuple<LOG_LEVEL, IErrorData> logTuple)
        {
            LOG_LEVEL level = logTuple.Item1;
            string message = GetMessage(logTuple);
            if (message == null)
            {
                return;
            }

            m_records.Add(new LogRecord((int)level, message));

            if (RecordsArrived != null)
            {
                    RecordsArrived(this, new RecordsEventArgs(m_records));// pass the record to user log page 
                    m_records.Clear(); 
            }

            WriteTraceLog(level,message);
        }

       

       
    }
}
