// Copyright (c) Microsoft Corporation.  All rights reserved.
// This source code is made available under the terms of the Microsoft Public License (MS-PL)

using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace NostreetsExtensions.Helpers
{

    //+ IMPORTANT!!!IMPORTANT!!!IMPORTANT!!!IMPORTANT!!!
    // --------------------------------------------------
    // Dont write any debugging statements anywhere in this
    // class such as Debug.WriteLine or Trace.WriteLine
    // simply because it would go into an endless loop
    // --------------------------------------------------
    public class DBTraceListener : TraceListener
    {
        public DBTraceListener() : this(string.Empty) { }

        public DBTraceListener(string connectionString)
            : base(connectionString)
        {
            // Initialize connection object
            _cn = new SqlConnection();
            _cn.ConnectionString = connectionString;
            ConnectionString = connectionString;

            try
            {
                _cn.Open();
            }
            catch (Exception ex)
            {
                // Write to internal 
                WriteEntryToInternalLog(string.Format("Could not connect to database from the provided connection string. Exception: {0}", ex.ToString()));

                // Let the caller know that this listener object cannot do its 
                // work because it cannot establish connection to database
                //
                // Since Tracing framework is initialized by CLR, you would 
                // in all likelihood get Could not create type... error
                throw;
            }

            // Setup command object
            _cm = _cn.CreateCommand();
            _cm.CommandText = STORED_PROC_NAME;
            _cm.CommandType = CommandType.StoredProcedure;
            _cm.Parameters.Add(new SqlParameter(STORED_PROC_MESSAGE_PARAM_NAME, DBNull.Value));
            _cm.Parameters.Add(new SqlParameter(STORED_PROC_MESSAGETYPE_PARAM_NAME, DBNull.Value));
            _cm.Parameters.Add(new SqlParameter
                            (STORED_PROC_COMPONENTNAME_PARAM_NAME, DBNull.Value));
        }

        public event DBTraceFailedHandler DBTraceFailed;
        
        #region Fields

        private const string STORED_PROC_NAME = "prc_writetraceentry";
        private const string STORED_PROC_MESSAGE_PARAM_NAME = "@message";
        private const string STORED_PROC_MESSAGETYPE_PARAM_NAME = "@type";
        private const string STORED_PROC_COMPONENTNAME_PARAM_NAME = "@componentname";
        private const string TRACE_SWITCH_NAME = "DBTraceSwitch";
        private const string TRACE_SWITCH_DESCRIPTION = "Trace switch defined in config file for configuring trace output to database";

        // Not defining it as readonly string so that in future it could come
        // from an external source and we can provide initializer for it
        private static readonly string DEFAULT_TRACE_TYPE = "Verbose"; 

        // Database connection object
        private SqlConnection _cn;

        // Database command object
        private SqlCommand _cm;

        // Connection string for database
        private string _connectionString;

        // Flag for DBTraceListener object disposal status
        private bool _disposed = false;

        // Trace Switch object for controlling trace output, defaulting to Verbose
        private TraceSwitch _traceSwitch =  new TraceSwitch(TRACE_SWITCH_NAME, TRACE_SWITCH_DESCRIPTION, DEFAULT_TRACE_TYPE);

        // Delegate to point to the method which would do actual operation of logging
        private LogIt _loggingMethod;

        // Component Name 
        private string _componentName;

        // Lock object
        private object _traceLockObject = new object();
        private object _fileLockObject = new object();

        // Timer to refresh trace configuration information
        private Timer _traceSwitchTimer;

        // Flag to indicate whether trace configuration data needs to be refreshed
        private bool _refreshTraceConfig = false;
        #endregion

        #region Properties

        public override bool IsThreadSafe
        {
            // TODO: We are logging to database and the core method responsible
            // for this places lock on the core code responsible for executing
            // database command to ensure that only one thread can access it at
            // a time. Considering this, we can really return true for this 
            // property but before doing that, just need to do some testing.
            get { return false; }
        }

        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    LoadAttributes();
                }
                return _connectionString;
            }
            set { _connectionString = value; }
        }

        public string ComponentName
        {
            get
            {
                if (string.IsNullOrEmpty(_componentName))
                {
                    LoadAttributes();
                }
                return _componentName;
            }
            set { _componentName = value; }
        }

        /// <summary>
        /// Setting this property to True would refresh Trace configuration
        /// data from system.diagnostics section in your application configuration
        /// file. It is important to note that it would only refresh trace configuration
        /// data and not entire configuration file. For example, while your application
        /// is running, if you change one of the appSettings values, it would not be 
        /// refreshed but changing trace switch value would be refreshed.
        /// </summary>
        public bool RefreshTraceConfig
        {
            get
            {
                LoadAttributes();
                return _refreshTraceConfig;
            }
            set
            {
                if (value)
                {
                    // Refresh trace section every 15 minutes
                    if (!_refreshTraceConfig)
                    {
                        // i.e. If timer is not already active
                        _refreshTraceConfig = true;
                        _traceSwitchTimer = new Timer(new TimerCallback(RefreshSwitch),
                                                 null, new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0));
                    }
                }
                else
                {
                    // If timer is active, stop it
                    _refreshTraceConfig = false;
                    _traceSwitchTimer.Dispose();
                    _traceSwitchTimer = null;
                }
            }
        }

        #endregion

        #region Methods

        #region Trace Methods

        /// <summary>
        /// Another method useful for testing if DBTraceListener is
        /// able to establish connection to database
        /// </summary>
        /// <returns>void</returns>

        internal bool ShouldLogTrace(TraceEventType eventType)
        {
            bool shouldLog = true;

            switch (eventType)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    shouldLog = _traceSwitch.TraceError;
                    break;

                case TraceEventType.Warning:
                    shouldLog = _traceSwitch.TraceWarning;
                    break;

                case TraceEventType.Information:
                    shouldLog = _traceSwitch.TraceInfo;
                    break;

                case TraceEventType.Start:
                case TraceEventType.Stop:
                case TraceEventType.Suspend:
                case TraceEventType.Resume:
                case TraceEventType.Transfer:
                case TraceEventType.Verbose:
                    shouldLog = _traceSwitch.TraceVerbose;
                    break;
            }

            return shouldLog;
        }

        public override void TraceEvent(TraceEventCache eventCache,
                                        string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, string.Empty);
        }

        public override void TraceEvent(TraceEventCache eventCache,
                                string source, TraceEventType eventType, int id, string message)
        {
            XElement msg;

            try
            {
                if (!ShouldLogTrace(eventType))
                    return;

                if (eventType == TraceEventType.Critical ||
                        eventType == TraceEventType.Error ||
                        eventType == TraceEventType.Warning)
                {
                    msg = new XElement("TraceLog",
                                            new XElement("Message", message),
                                            new XElement("Id", id),
                                            new XElement("CallStack", eventCache.Callstack.ToString()),
                                            new XElement("ThreadId", eventCache.ThreadId),
                                            new XElement("ProcessId", eventCache.ProcessId)
                                       );
                }
                else
                {
                    msg = new XElement("TraceLog",
                                            new XElement("Message", message));
                }

                WriteLineInternal(msg.ToString(), eventType.ToString(), null);
            }
            catch (Exception ex)
            {
                WriteLine(
                    string.Format("AdvancedTracing::DBTraceListener - Trace.TraceEvent failed", ex.ToString(), message),
                    "Error",
                    "DBTraceListener"
                );

                WriteEntryToInternalLog(string.Format("Trace.TraceEvent failed with following exception: {0} ", ex.ToString()));
            }
        }

        public override void TraceEvent(TraceEventCache eventCache, string source,
                TraceEventType eventType, int id, string format, params object[] args)
        {
            XElement msg;

            try
            {
                if (!ShouldLogTrace(eventType))
                    return;

                if (eventType == TraceEventType.Critical ||
                        eventType == TraceEventType.Error ||
                        eventType == TraceEventType.Warning)
                {
                    msg = new XElement("TraceLog",
                                            new XElement("Message", string.Format(format, args)),
                                            new XElement("Id", id),
                                            new XElement("CallStack", eventCache.Callstack.ToString()),
                                            new XElement("ThreadId", eventCache.ThreadId),
                                            new XElement("ProcessId", eventCache.ProcessId)
                                       );
                }
                else
                {
                    msg = new XElement("TraceLog",
                                            new XElement("Message", string.Format(format, args)));
                }

                WriteLineInternal(msg.ToString(), eventType.ToString(), null);
            }
            catch (Exception ex)
            {
                WriteLine(
                     string.Format("AdvancedTracing::DBTraceListener - Trace.TraceEvent failed  with following exception: {0}, for message {1}  "
                                    , ex.ToString(), format)
                                    , "Error"
                                    , "DBTraceListener"
                );

                WriteEntryToInternalLog(string.Format("Trace.TraceEvent failed with following exception: {0} ", ex.ToString()));
            }
        }


        public override void TraceTransfer(TraceEventCache eventCache, string source,
                                           int id, string message, Guid relatedActivityId)
        {
            try
            {
                if (ShouldLogTrace(TraceEventType.Transfer))
                {
                    XElement msg = new XElement("TraceLog",
                            new XElement("Message", message),
                            new XElement("Source", source),
                            new XElement("Id", id),
                            new XElement("RelatedActivityId", relatedActivityId.ToString()),
                            new XElement("CallStack", eventCache.Callstack.ToString()),
                            new XElement("ThreadId", eventCache.ThreadId),
                            new XElement("ProcessId", eventCache.ProcessId));

                    WriteLine(msg.ToString(), TraceEventType.Verbose.ToString(), null);
                }
            }
            catch (Exception ex)
            {
                WriteLine(
                    string.Format("AdvancedTracing::DBTraceListener - Trace.TraceTransfer failed with following exception: {0}, for message {1} ", ex.ToString(), message), "Error", "DBTraceListener"
                );

                WriteEntryToInternalLog(string.Format("Trace.TraceTransfer failed with following exception: {0}", ex.ToString()));
            }
        } 
        #endregion

        #region Write Methods

        public override void Write(object o)
        {
            if (o != null)
            {
                WriteLine(o.ToString(), null);
            }
        }

        public override void Write(string message)
        {
            WriteLine(message, null);
        }

        public override void Write(object o, string category)
        {
            if (o != null)
            {
                WriteLine(o.ToString(), category);
            }
        }

        public override void Write(string message, string category)
        {
            WriteLine(message, category);
        }

        public override void WriteLine(object o)
        {
            if (o != null)
            {
                WriteLine(o.ToString(), null);
            }
        }

        public override void WriteLine(object o, string category)
        {
            if (o != null)
            {
                WriteLine(o.ToString(), category);
            }
        }

        public override void WriteLine(string message)
        {
            WriteLine(message, null);
        }

        override public void WriteLine(string message, string category)
        {
            try
            {
                if (!ShouldLogTrace(TraceEventType.Verbose))
                    return;

                // IMPORTANT!!!!
                // DO NOT WRITE ANY Debug.WriteLine or Trace.WriteLine statements in this method
                XElement msg = new XElement("TraceLog",
                                   new XElement("Message", message));

                WriteLineInternal(msg.ToString(), category, null);
            }
            catch (Exception ex)
            {
                WriteEntryToInternalLog(string.Format
                     ("WriteLine failed with following exception: {0}", ex.ToString()));
            }
        }

        /// <summary>
        /// This is a non-standard WriteLine method i.e. Trace class does not provide
        /// a WriteLine method taking three parameters. It is used by internal implementation
        /// of this class to provide functionality to log a different component name,
        /// **primarily aimed towards helping in debugging some particular scenario**
        /// </summary>
        /// <param name="message" />
        /// <param name="type" />
        /// <param name="componentName" />
        public void WriteLine(string message, string category, string componentName)
        {
            try
            {
                if (!ShouldLogTrace(TraceEventType.Verbose))
                    return;

                // IMPORTANT!!!!
                // DO NOT WRITE ANY Debug.WriteLine or Trace.WriteLine statements in this method
                XElement msg = new XElement("TraceLog",
                                   new XElement("Message", message));

                WriteLineInternal(msg.ToString(), category, componentName);
            }
            catch (Exception ex)
            {
                WriteEntryToInternalLog(string.Format
                     ("WriteLine failed with following exception: {0}", ex.ToString()));
            }
        }

        private void WriteLineInternal(string message, string category, string componentName)
        {
            // Perform the actual operation of logging **asynchronously**
            _loggingMethod = SaveLogEntry;
            _loggingMethod.BeginInvoke(message, category, componentName, null, null);
        }

        private void WriteEntryToInternalLog(string msg)
        {
            lock (_fileLockObject)
            {
                try
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + @"\DBTraceListener.log",
                        string.Format("{0}{1}: {2}", Environment.NewLine, DateTime.Now.ToString(), msg));
                }
                catch
                {
                    // Do nothing
                }
            }
        }

        private void SaveLogEntry(string message, string category, string componentName)
        {
            // IMPORTANT!!!!
            // DO NOT WRITE ANY Debug.WriteLine or Trace.WriteLine statements in this method
            lock (_traceLockObject)
            {
                try
                {
                    // save trace message to database
                    if (_cn.State == ConnectionState.Broken ||
                        _cn.State == ConnectionState.Closed)
                    {
                        _cn.ConnectionString = ConnectionString;
                        _cn.Open();
                    }

                    _cm.Parameters[STORED_PROC_MESSAGE_PARAM_NAME].Value = message;
                    _cm.Parameters[STORED_PROC_MESSAGETYPE_PARAM_NAME].Value = category;

                    if (string.IsNullOrEmpty(componentName))
                    {
                        // No value provided by caller. Look for the value defined 
                        // in application configuration file
                        if (string.IsNullOrEmpty(ComponentName))
                        {
                            _cm.Parameters[STORED_PROC_COMPONENTNAME_PARAM_NAME].Value =
                                                                                   DBNull.Value;
                        }
                        else
                        {
                            _cm.Parameters[STORED_PROC_COMPONENTNAME_PARAM_NAME].Value =
                                                                                ComponentName;
                        }
                    }
                    else
                    {
                        // Specific value provided by caller for this specific trace/log
                        // Need to use the same
                        _cm.Parameters[STORED_PROC_COMPONENTNAME_PARAM_NAME].Value = componentName;
                    }

                    _cm.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Raise event to let others know, just in case 
                    // someone interested
                    if (DBTraceFailed != null)
                    {
                        DBTraceFailed(ex.ToString());
                    }

                    // Write entry to internal log file
                    WriteEntryToInternalLog(ex.ToString());
                }
                finally
                {
                    // Nothing to dispose in case of exception
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("DBTraceListener for Component: {0} using ConnectionString: {1}",
                                  ComponentName, ConnectionString);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_cn != null)
                        _cn.Dispose();

                    if (_cm != null)
                        _cm.Dispose();

                    if (_traceSwitchTimer != null)
                        _traceSwitchTimer.Dispose();
                }

                _disposed = true;
            }

            _cm = null;
            _cn = null;

            base.Dispose(disposing);
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "connectionString", "componentName", "refreshTraceConfig" };
        }

        public bool TestDBConnection()
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(ConnectionString))
                {
                    cn.Open();
                }
                return true;
            }
            catch
            {
                // In case of any exception just return false
                return false;
            }
        }

        private void LoadAttributes()
        {
            if (Attributes.ContainsKey("connectionString"))
            {
                ConnectionString = Attributes["connectionString"];
            }

            if (Attributes.ContainsKey("componentName"))
            {
                ComponentName = Attributes["componentName"];
            }

            if (Attributes.ContainsKey("refreshTraceConfig"))
            {
                bool val;
                bool.TryParse(Attributes["refreshTraceConfig"], out val);
                RefreshTraceConfig = val;
            }
        }

        void RefreshSwitch(object o)
        {
            // Trace.Refresh call is not expected to throw any exception, but if it DOES
            // catch the exception and do nothing
            try
            {
                if (RefreshTraceConfig)
                {
                    Trace.Refresh();
                }
            }
            catch (Exception ex)
            {
                WriteLine(
                    string.Format("Trace.Refresh failed with following exception: {0}, ", ex.ToString()),
                    "Error",
                    "DBTraceListener"
                );

                WriteEntryToInternalLog(string.Format
                  ("Trace.Refresh failed with following exception: {0}, ", ex.ToString()));
            }
        }

        #endregion

    }

    // Delegate definition for the real method responsible for logging
    internal delegate void LogIt(string message, string type, string componentName);

    // Delegate for DBTraceFailed event 
    public delegate void DBTraceFailedHandler(string exceptionText);
}