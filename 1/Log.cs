﻿using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace BMWPaint;

public static class Log
{
    private static Logger? Logger { get; }
    public static LogBuffer Buffer { get; } = new();
    static Log() => Logger = new LoggerConfiguration().WriteTo.Sink(Buffer).CreateLogger();
    public static void Trace(this object obj, params object[] param)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in param)
            sb.Append(item?.ToString() + " ");
        Logger?.Information($"{obj.GetType().Name} {new StackTrace().GetFrame(1)?.GetMethod()?.Name} {sb}");
    }
}

public class LogBuffer : ILogEventSink
{
    public event EventHandler<string>? CanRead = null;
    private MessageTemplateTextFormatter formater = new("[{Timestamp:HH:mm:ss}] {Message}{Exception}");
    public void Emit(LogEvent logEvent)
    {
        var renderSpace = new StringWriter();
        formater.Format(logEvent, renderSpace);
        CanRead?.Invoke(this, renderSpace.ToString());
    }
}