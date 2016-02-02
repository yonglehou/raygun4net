using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mindscape.Raygun4Net.Messages;

namespace Mindscape.Raygun4Net.Builders
{
  public class RaygunErrorMessageBuilder : RaygunErrorMessageBuilderBase
  {
    public static RaygunErrorMessage Build(Exception exception)
    {
      RaygunErrorMessage message = new RaygunErrorMessage();

      var exceptionType = exception.GetType();

      message.Message = exception.Message;
      message.ClassName = FormatTypeName(exceptionType, true);

      message.StackTrace = BuildStackTrace(exception);
      message.Data = exception.Data;

      AggregateException ae = exception as AggregateException;
      if (ae != null && ae.InnerExceptions != null)
      {
        message.InnerErrors = new RaygunErrorMessage[ae.InnerExceptions.Count];
        int index = 0;
        foreach (Exception e in ae.InnerExceptions)
        {
          message.InnerErrors[index] = Build(e);
          index++;
        }
      }
      else if (exception.InnerException != null)
      {
        message.InnerError = Build(exception.InnerException);
      }

      return message;
    }

    private static RaygunErrorStackTraceLineMessage[] BuildStackTrace(Exception exception)
    {
      var lines = new List<RaygunErrorStackTraceLineMessage>();

      string stackTraceStr = exception.StackTrace;
      if (stackTraceStr == null)
      {
        var line = new RaygunErrorStackTraceLineMessage { FileName = "none", LineNumber = 0 };
        lines.Add(line);
        return lines.ToArray();
      }
      try
      {
        string[] stackTraceLines = stackTraceStr.Split('\n');
        foreach (string stackTraceLine in stackTraceLines)
        {
          int lineNumber = 0;
          string fileName = null;
          string methodName = null;
          string className = null;
          string stackTraceLn = stackTraceLine.Trim();
          // Line number
          int index = stackTraceLn.LastIndexOf(":");
          if (index > 0)
          {
            bool success = int.TryParse(stackTraceLn.Substring(index + 1), out lineNumber);
            if (success)
            {
              stackTraceLn = stackTraceLn.Substring(0, index);
            }
          }

          // File name
          index = stackTraceLn.LastIndexOf("] in ");
          if (index > 0)
          {
            fileName = stackTraceLn.Substring(index + 5);
            if ("<filename unknown>".Equals(fileName))
            {
              fileName = null;
            }
            stackTraceLn = stackTraceLn.Substring(0, index);
          }

          if (!stackTraceLn.StartsWith("at (wrapper") && !stackTraceLn.StartsWith("(wrapper"))
          {
            // Method name
            index = stackTraceLn.LastIndexOf("(");
            if (index > 0)
            {
              index = stackTraceLn.LastIndexOf(".", index);
              if (index > 0)
              {
                int endIndex = stackTraceLn.IndexOf("[0x");
                if (endIndex < 0)
                {
                  endIndex = stackTraceLn.IndexOf("<0x");
                  if (endIndex < 0)
                  {
                    endIndex = stackTraceLn.Length;
                  }
                }

                methodName = stackTraceLn.Substring(index + 1, endIndex - index - 1).Trim();
                methodName = methodName.Replace(" (", "(");
                stackTraceLn = stackTraceLn.Substring(0, index);
              }
            }
            // Class name
            index = stackTraceLn.IndexOf("at ");
            if (index >= 0)
            {
              className = stackTraceLn.Substring(index + 3);
            }
          }
          
          if (lineNumber != 0 || !String.IsNullOrWhiteSpace(methodName) || !String.IsNullOrWhiteSpace(fileName) || !String.IsNullOrWhiteSpace(className))
          {
            var line = new RaygunErrorStackTraceLineMessage
            {
              FileName = fileName,
              LineNumber = lineNumber,
              MethodName = methodName,
              ClassName = className
            };

            lines.Add(line);
          }
          else if (!String.IsNullOrWhiteSpace(stackTraceLn))
          {
            if (stackTraceLn.StartsWith("at "))
            {
              stackTraceLn = stackTraceLn.Substring(3);
            }
            var line = new RaygunErrorStackTraceLineMessage
            {
              FileName = stackTraceLn
            };

            lines.Add(line);
          }
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(String.Format("Failed to parse stack trace: {0}", ex.Message));
      }

      return lines.ToArray();
    }
  }
}
