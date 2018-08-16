using PostSharp.Aspects;
using System;
using System.Diagnostics;
using System.Reflection;

namespace NostreetsExtensions.DataControl.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class TraceAttribute : OnMethodBoundaryAspect
    {
        public TraceAttribute()
        {
            Params = "";
        }

        public TraceAttribute(bool writeToDebug)
        {
            Params = "";
            _writeToDebug = writeToDebug;
        }

        public string Params { get; set; }
        public string Name { get; set; }

        private bool _writeToDebug = false;

        public override void OnEntry(MethodExecutionArgs args)
        {

            if (_writeToDebug)
                Trace.WriteLine(string.Format("Entering {0}.{1}.",
                                        args.Method.DeclaringType.Name,
                                        args.Method.Name));

            ParameterInfo[] paramerters = args.Method.GetParameters();
            for(int i = 0; i < paramerters.Length; i++)
                Params += paramerters[i].Name + " = " + args.Arguments.GetArgument(i) + ",  ";

        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if (_writeToDebug)
            {
                Trace.WriteLine("Return Value: " + args.ReturnValue);

                Trace.WriteLine(string.Format("Leaving {0}.{1}.",
                                        args.Method.DeclaringType.Name,
                                        args.Method.Name));
            }
        }
    }
}
