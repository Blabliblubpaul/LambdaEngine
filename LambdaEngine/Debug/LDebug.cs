// #define DEBUG_LOGGER

using System.Collections.Concurrent;
using static LambdaEngine.Debug.LogLevel;

namespace LambdaEngine.Debug;

/// <summary>
/// Default implementation of the DebugSystem.
/// </summary>
public static class LDebug {
    private static readonly Dictionary<LogLevel, ConsoleColor> _logColors = new() {
        { TRACE, ConsoleColor.Gray },
        { DEBUG, ConsoleColor.DarkBlue },
        { INFO, ConsoleColor.Green },
        { WARNING, ConsoleColor.Yellow },
        { ERROR, ConsoleColor.DarkRed },
        { FATAL, ConsoleColor.Red }
    };
    
    private static readonly ConcurrentQueue<(string Message, LogLevel logLevel)> _logQueue = new();
    
    private static bool _debugRunning = true;
    private static Thread? _debugThread;
    
    public static LogLevel LogLevel { get; set; } 

    public static void Initialize() {
        _debugThread = new Thread(DebuggerThread) {
            IsBackground = true
        };
    }

    public static void Start() {
        if (_debugThread == null) {
            throw new InvalidOperationException("Logger not initialized");
        }
        
        _debugRunning = true;
        
        _debugThread.Start();
    }
    
    public static void Stop() {
        _debugRunning = false;
        _debugThread?.Join();
    }

    public static void Log(string message, LogLevel logLevel = INFO) {
        _logQueue.Enqueue((message, logLevel));
    }

    /// <summary>
    /// Handle log messages and print them to the currently active console.
    /// </summary>
    private static void DebuggerThread() {
        while (_debugRunning) {
            while (_logQueue.TryDequeue(out (string Message, LogLevel logLevel) logEntry)) {
                if (Console.ForegroundColor != _logColors[logEntry.logLevel]) {
                    Console.ForegroundColor = _logColors[logEntry.logLevel];
                }
                
                string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] [{logEntry.logLevel}] {logEntry.Message}";
                Console.WriteLine(formattedMessage);
            }

            Thread.Sleep(10);
        }

        Console.WriteLine("Debugger stopped.");
    }
}