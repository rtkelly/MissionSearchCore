using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLog.Config;
using NLog.Targets;
using NLog;

namespace MissionSearch.Util
{
  public class MissionLogger : ILogger
  {
     private Logger _logger;

     public enum LoggerLevel
     {
         Debug,
         Info,
         Error
     }

     public MissionLogger(string logPath, LoggerLevel logLevel)
     {
         if(!string.IsNullOrEmpty(logPath))
            IntializeLogger(logPath, logLevel);
     }

     public MissionLogger(string logPath)
     {
         if (!string.IsNullOrEmpty(logPath))
            IntializeLogger(logPath, LoggerLevel.Error);
     }

     private void IntializeLogger(string logPath, LoggerLevel logLevel)
     {
         var logFile = "MissionSearch.log";

         var config = new LoggingConfiguration();
                  
         var fileTarget = new FileTarget();
         config.AddTarget("file", fileTarget);
         
         fileTarget.FileName = string.Format(@"{0}\{1}", logPath, logFile);
         fileTarget.Layout = "${message}";

         switch(logLevel)
         {
          
             case LoggerLevel.Debug:
                 config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, fileTarget));
                 break;

             case LoggerLevel.Info:
                 config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, fileTarget));
                 break;

             case LoggerLevel.Error:
                 config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Error, fileTarget));
                 break;


         }
         
         /*
         if (logLevel == LogLevel.Info)
         {
             var rule2 = new LoggingRule("*", LogLevel.Info, fileTarget);
             config.LoggingRules.Add(rule2);
         }
         else if (logLevel == LogLevel.Trace)
         {
             var rule2 = new LoggingRule("*", LogLevel.Info, fileTarget);
             config.LoggingRules.Add(rule2);

             var rule3 = new LoggingRule("*", LogLevel.Trace, fileTarget);
             config.LoggingRules.Add(rule3);
         }
          * */

         LogManager.Configuration = config;

         _logger = LogManager.GetLogger("MissionSearch");
         
     }

     public void Trace(string message)
     {
         if (_logger != null)
             _logger.Trace("Trace: " + message);
     }
 
     public void Info(string message)
     {
         if(_logger != null)
            _logger.Info("Info: " + message);
     }
 
     public void Warn(string message)
     {
         if (_logger != null)
            _logger.Warn("Warning: " + string.Format("{0:yyyy-MM-dd hh:mm:ss tt}-{1}", DateTime.Now, message));
     }
 
     public void Debug(string message)
     {
         if (_logger != null)
            _logger.Debug("Debug: " + string.Format("{0:yyyy-MM-dd hh:mm:ss tt}-{1}", DateTime.Now, message));
     }
 
     public void Error(string message)
     {
         if (_logger != null)
            _logger.Error("Error: " + string.Format("{0:yyyy-MM-dd hh:mm:ss tt}-{1}", DateTime.Now, message));
     }

    
  }
}
