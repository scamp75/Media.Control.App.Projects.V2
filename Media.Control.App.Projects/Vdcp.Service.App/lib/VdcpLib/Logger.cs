using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace VdcpService.lib
{
    public enum EnuLoggerLevel { Off, Detail, All }

    public enum LogLevel
    {
        /// <summary>
        /// 모든 속성 로그 출력
        /// </summary>
        /// 
        All = -1,

        /// <summary>
        /// 모든 속성 로그 출력하지 않음
        /// </summary>
        Off = 0,

        /// <summary>
        /// Critical(심각한 오류) 속성 로그만 출력
        /// </summary>
        Critical = 1,

        /// <summary>
        /// Critical(심각한 오류), Error(복구 가능한 오류) 속성 로그만 출력
        /// </summary>
        Error = 3,

        /// <summary>
        /// Critical(심각한 오류), Error(복구 가능한 오류), Warning(단순한 문제) 속성 로그만 출력
        /// </summary>
        Warning = 7,

        /// <summary>
        /// Critical(심각한 오류), Error(복구 가능한 오류), Warning(단순한 문제), Information(알림) 속성 로그만 출력
        /// </summary>
        Information = 15,

        /// <summary>
        /// Critical(심각한 오류), Error(복구 가능한 오류), Warning(단순한 문제), Information(알림), Verbose(기본 추적 디버깅) 속성 로그만 출력
        /// </summary>
        Verbose = 31,

        /// <summary>
        /// Start(논리 작업 시작), Stop(논리 작업 중지), Suspend(논리 작업 일시 중지), Resume(논리 작업 다시 시작), Transfer(상관 관계 ID 변경) 속성 로그만 출력
        /// </summary>
        ActivityTracing = 65280,
    }

    public static class Logger
    {
        #region internal vriable
        private static TraceSource traceSource;

        private static RollingFileTraceListener file_listener = new RollingFileTraceListener("File_Listener");
        private static DefaultTraceListener outputDebugListener = new DefaultTraceListener() { Name = "OutputDebugListener" };
        private static ConsoleTraceListener consoleTraceListener = new ConsoleTraceListener() { Name = "ConsoleListener" };
        private static EventLogTraceListener eventLogTraceListener = new EventLogTraceListener("Application") { Name = "EventLogListener" };
        #endregion


        public static EnuLoggerLevel LoggerLevel { get; set; } = EnuLoggerLevel.Detail;

        /// <summary>
        /// 파일 출력 수신기 (접근하여 파일 로그 속성을 변경 할 수 있음)
        /// </summary>
        public static RollingFileTraceListener File_Listener { get { return file_listener; } }

        /// <summary>
        /// 출력되는 로그 수준을 변경할 수 있음
        /// </summary>
        public static LogLevel LogLevel
        {
            get { return (LogLevel)traceSource.Switch.Level; }
            set { traceSource.Switch.Level = (SourceLevels)value; }
        }

        static Logger()
        {
            traceSource = new TraceSource("TraceSource");
            traceSource.Switch = new SourceSwitch("Switch");
            traceSource.Switch.Level = SourceLevels.All;

            traceSource.Listeners.Clear();
        }

        public static void Add_File_Listener()
        {
            if (!traceSource.Listeners.Contains(File_Listener))
                traceSource.Listeners.Add(File_Listener);
        }

        public static void Remove_File_Listener()
        {
            traceSource.Listeners.Remove(File_Listener);
        }

        public static void Add_OutputDebug_Listener()
        {
            if (!traceSource.Listeners.Contains(outputDebugListener))
                traceSource.Listeners.Add(outputDebugListener);
        }

        public static void Remove_OutputDebug_Listener()
        {
            traceSource.Listeners.Remove(outputDebugListener);
        }

        public static void Add_Console_Listener()
        {
            if (!traceSource.Listeners.Contains(consoleTraceListener))
                traceSource.Listeners.Add(consoleTraceListener);
        }

        public static void Remove_Console_Listener()
        {
            traceSource.Listeners.Remove(consoleTraceListener);
        }

        public static void Add_EventLog_Listener()
        {
            if (!traceSource.Listeners.Contains(eventLogTraceListener))
                traceSource.Listeners.Add(eventLogTraceListener);
        }

        public static void Remove_EventLog_Listener()
        {
            traceSource.Listeners.Remove(eventLogTraceListener);
        }

        private static void Trace(TraceEventType eventType, string message, string callerName, string fileName, int lineNumber)
        {
            if (traceSource.Switch.ShouldTrace(eventType)) // 로그 수준 확인하는 거
            {
                foreach (TraceListener listener in traceSource.Listeners)
                {
                    listener.Write($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff")}] [{eventType.ToString()}] [{Path.GetFileName(fileName)}({lineNumber}) {callerName}] {message ?? string.Empty}");
                }

            }
        }

        private static void Trace(TraceEventType eventType, string message)
        {
            if (traceSource.Switch.ShouldTrace(eventType)) // 로그 수준 확인하는 거
            {
                foreach (TraceListener listener in traceSource.Listeners)
                {
                    listener.Write($"{DateTime.Now.ToString("HH:mm:ss:fff")} [{eventType.ToString()}] {message ?? string.Empty}");
                }

            }
        }

        private static void TraceLine(TraceEventType eventType, string message)
        {
            if (traceSource.Switch.ShouldTrace(eventType)) // 로그 수준 확인하는 거
            {
                foreach (TraceListener listener in traceSource.Listeners)
                {
                    listener.WriteLine($"{DateTime.Now.ToString("HH:mm:ss:fff")} {message ?? string.Empty}");
                }

            }
        }

        private static void TraceLine(TraceEventType eventType, string message, string callerName, string fileName, int lineNumber)
        {
            if (traceSource.Switch.ShouldTrace(eventType)) // 로그 수준 확인하는 거
            {
                foreach (TraceListener listener in traceSource.Listeners)
                {
                    listener.WriteLine($"{DateTime.Now.ToString("HH:mm:ss:fff")}  [{eventType.ToString()}] [{Path.GetFileName(fileName)}({lineNumber}) {callerName}] {message ?? string.Empty}");
                }

            }
        }

        private static void TraceLine_Indent(TraceEventType eventType, string callerName, string fileName, int lineNumber, params string[] messages) // 들여쓰기
        {
            if (traceSource.Switch.ShouldTrace(eventType)) // 로그 수준 확인하는 거
            {
                string indent = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff")}] [{eventType.ToString()}] [{Path.GetFileName(fileName)}({lineNumber}) {callerName}] ";

                List<string> lines = new List<string>();

                if (messages != null)
                {
                    foreach (string message in messages)
                    {
                        string[] subMessages = message?.Split('\n');
                        if (subMessages != null)
                        {
                            foreach (string subMessage in subMessages)
                                lines.Add(subMessage);
                        }
                    }
                }

                foreach (TraceListener listener in traceSource.Listeners)
                {
                    if (lines.Count == 0)
                        listener.WriteLine(indent);
                    else
                        listener.WriteLine(indent + lines[0] ?? string.Empty);

                    for (int i = 1; i < lines.Count; i++)
                        listener.WriteLine("".PadLeft(indent.Length) + lines[i] ?? string.Empty);
                }

                lines.Clear();
            }
        }

        private static void WriteLine_Exception(Exception exception, TraceEventType logType, string callerName, string fileName, int lineNumber)
        {
            StringBuilder messages = new StringBuilder();

            messages.Append($"--------------------- <<< Exception Thrown >>> ---------------------\n");
            messages.Append($"Type: {exception.GetType().Name}\n");
            messages.Append($"Message: {exception.Message}\n");
            messages.Append($"StackTrace: \n{exception.StackTrace}\n");

            Exception InnerException = exception.InnerException;
            while (InnerException != null)
            {
                messages.Append($"----------------- <<< Inner Exception Included >>> -----------------\n");
                messages.Append($"Type: {InnerException.GetType().Name}\n");
                messages.Append($"Message: {InnerException.Message}\n");
                messages.Append($"StackTrace: \n{InnerException.StackTrace}\n");
                InnerException = InnerException.InnerException;
            }

            TraceLine_Indent(logType, callerName, fileName, lineNumber, messages.ToString());
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Verbose, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Verbose, message, callerName, fileName, lineNumber);
        }


        public static void WriteLine(TraceEventType traceEventType, string message)
        {
            TraceLine(traceEventType, message);
        }
        public static void Write(TraceEventType traceEventType, string message)
        {
            Trace(traceEventType, message);
        }


        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Verbose, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Verbose, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Verbose, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 <c>Exception</c> 정보를 출력함
        /// </summary>
        /// <param name="exception">출력하고자 하는 예외</param>
        public static void WriteLine_CriticalException(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine_Exception(exception, TraceEventType.Critical, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 <c>Exception</c> 정보를 출력함
        /// </summary>
        /// <param name="exception">출력하고자 하는 예외</param>
        public static void WriteLine_ErrorException(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine_Exception(exception, TraceEventType.Error, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 <c>Exception</c> 정보를 출력함
        /// </summary>
        /// <param name="exception">출력하고자 하는 예외</param>
        public static void WriteLine_WarningException(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine_Exception(exception, TraceEventType.Warning, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Information(알림) 속성으로 <c>Exception</c> 정보를 출력함
        /// </summary>
        /// <param name="exception">출력하고자 하는 예외</param>
        public static void WriteLine_InformationException(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine_Exception(exception, TraceEventType.Information, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 <c>Exception</c> 정보를 출력함
        /// </summary>
        /// <param name="exception">출력하고자 하는 예외</param>
        public static void WriteLine_VerboseException(Exception exception, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            WriteLine_Exception(exception, TraceEventType.Verbose, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Critical(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Critical, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Critical(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Critical, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Critical(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Critical, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Critical(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Critical, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Critical(심각한 오류) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Critical(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Critical, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Error(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Error, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Error(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Error, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Error(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Error, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Error(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Error, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Error(복구 가능한 오류) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Error(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Error, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Warning(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Warning, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Warning(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Warning, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Warning(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Warning, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Warning(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Warning, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Warning(단순한 문제) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Warning(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Warning, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Information(알림) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Information(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Information, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Information(알림) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Information(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Information, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Information(알림) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Information(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Information, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Information(알림) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Information(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Information, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Information(알림) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name = "messages" > 로그 메시지 배열</param>
        public static void WriteLine_Indent_Information(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Information, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Verbose(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Verbose, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Verbose(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Verbose, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Verbose(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Verbose, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Verbose(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Verbose, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Verbose(기본 추적 디버깅) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Verbose(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Verbose, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Start(논리 작업 시작) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Start(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Start, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Start(논리 작업 시작) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Start(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Start, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Start(논리 작업 시작) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Start(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Start, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Start(논리 작업 시작) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Start(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Start, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Start(논리 작업 시작) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Start(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Start, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Stop(논리 작업 중지) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Stop(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Stop, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Stop(논리 작업 중지) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Stop(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Stop, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Stop(논리 작업 중지) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Stop(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Stop, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Stop(논리 작업 중지) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Stop(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Stop, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Stop(논리 작업 중지) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Stop(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Stop, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Suspend(논리 작업 일시 중지) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Suspend(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Suspend, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Suspend(논리 작업 일시 중지) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Suspend(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Suspend, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Suspend(논리 작업 일시 중지) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Suspend(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Suspend, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Suspend(논리 작업 일시 중지) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Suspend(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Suspend, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Suspend(논리 작업 일시 중지) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Suspend(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Suspend, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Resume(논리 작업 다시 시작) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Resume(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Resume, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Resume(논리 작업 다시 시작) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Resume(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Resume, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Resume(논리 작업 다시 시작) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Resume(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Resume, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Resume(논리 작업 다시 시작) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Resume(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Resume, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Resume(논리 작업 다시 시작) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Resume(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Resume, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }

        /// <summary>
        /// Transfer(상관 관계 ID 변경) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void Write_Transfer(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            Trace(TraceEventType.Transfer, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Transfer(상관 관계 ID 변경) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public static void WriteLine_Transfer(string message, [CallerMemberName] string callerName = "", [CallerFilePath] string fileName = "", [CallerLineNumber] int lineNumber = 0)
        {
            TraceLine(TraceEventType.Transfer, message, callerName, fileName, lineNumber);
        }

        /// <summary>
        /// Transfer(상관 관계 ID 변경) 속성으로 수신기에 로그 메시지를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void Write_Transfer(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            Trace(TraceEventType.Transfer, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Transfer(상관 관계 ID 변경) 속성으로 수신기에 로그 메시지를 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="format">args 배열에 들어 있는 개체에 해당하는 0개 이상의 서식 항목이 포함된 서식 문자열</param>
        /// <param name="args">형식을 지정할 개체가 0개 이상 포함된 <c>object</c> 배열</param>
        public static void WriteLine_Transfer(string format, params object[] args)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine(TraceEventType.Transfer, string.Format(format, args), callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber());
        }

        /// <summary>
        /// Transfer(상관 관계 ID 변경) 속성으로 수신기에 들여쓰기가 적용된 로그 메시지들을 출력하고 줄 바꿈 문자를 출력
        /// </summary>
        /// <param name="messages">로그 메시지 배열</param>
        public static void WriteLine_Indent_Transfer(params string[] messages)
        {
            StackFrame callstack = new StackFrame(1, true);
            TraceLine_Indent(TraceEventType.Transfer, callstack.GetMethod().Name ?? "", callstack.GetFileName() ?? "", callstack.GetFileLineNumber(), messages);
        }
    }

    /// <summary>
    /// Trace 및 Debug 출력을 모니터링 하는 파일 출력 수신기에 대한 클래스
    /// </summary>
    public sealed class RollingFileTraceListener : TraceListener // initializeData = "MyLogFile_{yyMMdd}.log"
    {
        private string location = "Logs";
        private string baseFileName = Process.GetCurrentProcess().ProcessName + "_log";
        private FileInfo fileInfo = null;
        private TextWriter textWriter = null;
        private int index = 0;
        private bool fileChange = true;
        private bool fileRolling = false;
        private Sorter sorter = new Sorter();
        private ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim();

        private string FileNamePattern { get { return $"{DateTime.Now.ToString("yyyyMMdd")}_{baseFileName}_*.log"; } }
        private string FileName { get { return Path.Combine(location, $"{DateTime.Now.ToString("yyyyMMdd")}_{BaseFileName}_{index}.log"); } }

        /// <summary>
        /// 스레드로부터 안전한 TextWriter.Synchronized 사용함
        /// </summary>
        public override bool IsThreadSafe { get { return true; } }

        /// <summary>
        /// <para>로그 파일이 생성될 위치</para>
        /// <para>기본값: BaseDirectory\Logs</para>
        /// </summary>
        /// <exception cref="System.Security.SecurityException">호출자에게 필요한 권한이 없음</exception>
        /// <exception cref="System.ArgumentException">경로에 아무 것도 없거나, 공백만 있거나, 잘못된 문자를 포함</exception>
        /// <exception cref="System.UnauthorizedAccessException">경로에 대한 액세스 거부</exception>
        /// <exception cref="System.IO.PathTooLongException">경로가 시스템에 정의된 최대 길이를 초과 (Windows 기준 경로는 248자, 파일 이름은 260자 미만이어야 함)</exception>
        /// <exception cref="System.NotSupportedException">경로에 볼륨 식별자(예: "C:\")가 아닌 콜론(":")을 포함</exception>
        public string Location
        {
            get { return location; }
            set
            {
                if (CheckValidPath(value))
                {
                    location = value;
                    fileChange = true;
                }
            }
        }

        /// <summary>
        /// <para>기본 로그 파일명</para>
        /// <para>"로그 형식: {yyMMdd}_{BaseFileName}_{N}.log"</para>
        /// <para>기본값: ProcessName_Log</para>
        /// </summary>
        /// <exception cref="System.ArgumentNullException">파일 이름이 null임</exception>
        /// <exception cref="System.Security.SecurityException">호출자에게 필요한 권한이 없음</exception>
        /// <exception cref="System.ArgumentException">파일 이름이 아무 것도 없거나, 공백만 있거나, 잘못된 문자를 포함</exception>
        /// <exception cref="System.UnauthorizedAccessException">파일 이름에 대한 액세스 거부</exception>
        /// <exception cref="System.IO.PathTooLongException">파일 이름이 시스템에 정의된 최대 길이를 초과 (Windows 기준 경로는 248자, 파일 이름은 260자 미만이어야 함)</exception>
        /// <exception cref="System.NotSupportedException">파일 이름에 볼륨 식별자(예: "C:\")가 아닌 콜론(":")을 포함</exception>
        public string BaseFileName
        {
            get { return baseFileName; }
            set
            {
                if (CheckValidFileName(value))
                {
                    baseFileName = value;
                    fileChange = true;
                }
            }
        }

        /// <summary>
        /// <para>최대 파일 크기</para>
        /// <para>0 혹은 음수일 경우 파일 롤링 하지 않음</para>
        /// <para>기본값: 0</para>
        /// </summary>
        public long MaxFileSize { get; set; } = 0L;

        /// <summary>
        /// <para>최대 파일 개수</para>
        /// <para>0 혹은 음수일 경우 파일 삭제 하지 않음</para>
        /// <para>기본값: 0</para>
        /// </summary>
        public int MaxFileCount { get; set; } = 0;

        /// <summary>
        /// <para>프로그램 시작할 때마다 새 로그를 쓸 것인지 기존 로그에 이어 쓸것인지 지정함</para>
        /// <para>기본값: false</para>
        /// </summary>
        public bool StartAppend { get; set; } = false;

        /// <summary>
        /// <para>날짜가 바뀔 때 파일을 롤링할 것인지 지정함</para>
        /// <para>기본값: false</para>
        /// </summary>
        public bool DailyRolling { get; set; } = false;

        /// <summary>
        /// <c>RollingFileTraceListener</c> 클래스의 새 인터턴스를 생성함
        /// </summary>
        public RollingFileTraceListener() : base() { }

        /// <summary>
        /// 지정된 이름으로 <c>RollingFileTraceListener</c> 클래스의 새 인터턴스를 생성함
        /// </summary>
        /// <param name="name"><c>RollingFileTraceListener</c>의 이름</param>
        public RollingFileTraceListener(string name) : base(name) { }

        /// <summary>
        /// 파일 스트림 출력 수신기에 지정된 메시지를 씀
        /// </summary>
        /// <param name="message">로그 메시지</param>
        public override void Write(string message)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock(); // thread safe
                CheckStreamWriter();
                textWriter.Write(message);
                textWriter.Flush();
            }
            catch { }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// 파일 스트림 출력 수신기에 지정된 메시지를 쓰고 줄 바꿈 문자를 씀
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            try
            {
                readerWriterLockSlim.EnterWriteLock(); // thread safe
                CheckStreamWriter();
                textWriter.WriteLine(message);
                textWriter.Flush();
            }
            catch { }
            finally
            {
                readerWriterLockSlim.ExitWriteLock();
            }
        }

        /// <summary>
        /// 출력 스트림 버퍼를 플러시함
        /// </summary>
        public override void Flush()
        {
            textWriter?.Flush();
            base.Flush();
        }

        /// <summary>
        /// 출력 스트림을 닫음
        /// </summary>
        public override void Close()
        {
            textWriter.Close();
            base.Close();
        }

        private void CreateStreamWriter()
        {
            if (fileRolling)
            {
                fileInfo = new FileInfo(FileName);
                fileRolling = false;
            }

            if (fileChange)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(fileInfo.DirectoryName);

                if (!directoryInfo.Exists)
                    directoryInfo.Create();
            }

            textWriter = TextWriter.Synchronized(new StreamWriter(File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))); // Thread Safe

            fileChange = false;

            CheckFileCount();
        }

        private void CheckStreamWriter()
        {
            if (fileChange)
            {
                CheckStartIndex();
                CheckRollingIndex();

                CreateStreamWriter();
            }
            else
            {
                if (CheckRollingIndex())
                {
                    CreateStreamWriter();
                }
            }
        }

        private bool CheckRollingIndex()
        {
            fileInfo = new FileInfo(FileName);

            if (!fileInfo.Exists)
                return false;

            if ((MaxFileSize != 0 && fileInfo.Length > MaxFileSize) || (DailyRolling && fileInfo.CreationTime.Day.CompareTo(DateTime.Today.Day) != 0))
            {
                ++index;
                return fileRolling = true;
            }

            return false;
        }

        private void CheckFileCount()
        {
            if (MaxFileCount > 0)
            {
                FileInfo[] logFileList = (new DirectoryInfo(Location).GetFiles(FileNamePattern, SearchOption.TopDirectoryOnly));

                int deleteCount = logFileList.Count() - MaxFileCount;

                if (deleteCount > 0)
                {
                    Array.Sort(logFileList, 0, logFileList.Count(), sorter);

                    for (int i = 0; i < deleteCount; ++i)
                    {
                        try
                        {
                            logFileList[i].Delete();
                        }
                        catch { }
                    }
                }
            }
        }

        private int CheckStartIndex()
        {
            try
            {
                FileInfo[] logFileList = (new DirectoryInfo(Location).GetFiles(FileNamePattern, SearchOption.TopDirectoryOnly));

                if (logFileList.Any())
                {

                    Array.Sort(logFileList, 0, logFileList.Count(), sorter);

                    string file;
                    int start;

                    for (int i = logFileList.Count() - 1; i > 0; --i)
                    {
                        file = logFileList[i].Name;
                        start = file.LastIndexOf('_');
                        try
                        {
                            int finalIndex = Convert.ToInt32(file.Substring(start + 1, file.LastIndexOf(".") - start - 1));

                            if (textWriter == null && StartAppend) // 로그 쓰는 중간에 폴더를 바꿨는데, 바꾼 폴더 안에 파일이 있을 경우 이어 붙이지 않기 위해서
                            {
                                return index = finalIndex;
                            }
                            else
                            {
                                return index = finalIndex + 1;
                            }
                        }
                        catch { continue; }
                    }
                }
            }
            catch
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Location);
                directoryInfo.Create();
            }

            return index = 1;
        }

        private bool CheckValidFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }

        private bool CheckValidPath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        }

        /// <summary>
        /// <c>FileInfo</c> 객체를 생성날짜에 대하여 오래된 순서로 정렬하기 위한 <c>IComparer</c> 구현
        /// </summary>
        public class Sorter : IComparer<FileInfo>
        {
            public int Compare(FileInfo x, FileInfo y)
            {
                return x.CreationTime.CompareTo(y.CreationTime);
            }
        }
    }
}
