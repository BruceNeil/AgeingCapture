using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgeingCapture.Utils
{

    public class TraceInfo
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static void LogOut(LogLevels level, string msg, bool callback = false)
        {
            switch (level)
            {
                case LogLevels.Fatal:
                    logger.Fatal(msg);
                    break;
                case LogLevels.Error:
                    logger.Error(msg);
                    break;
                case LogLevels.SettingChange:
                    logger.Info("SettingChange: " + msg);
                    break;  
                case LogLevels.Sucess:
                    logger.Info("Sucess: " + msg);
                    break;
                case LogLevels.Warn:
                    logger.Warn(msg);
                    break;
                default:
                case LogLevels.Info:
                    logger.Info(msg);
                    break;
            }
        }

        public enum LogLevels
        {
            Sucess = 0,
            Info,
            Warn,
            Error,
            Fatal,
            SettingChange,
            MasterRunChange,
        }
    }
}
