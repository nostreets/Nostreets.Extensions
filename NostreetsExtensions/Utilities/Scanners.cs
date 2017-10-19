using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Utilities
{
    public enum ClassTypes
    {
        Any = 1,
        Assembly = 2,
        Methods = 4,
        Constructors = 8,
        Properties = 16,
        OtherFields = 32,
        Type = 64,
        Parameters = 128
    }

    public static class AssemblyScanner
    {
        static AssemblyScanner()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("System")) { skipAssemblies.Add(assembly.FullName); }
            }

        }

        private static List<string> skipAssemblies = new List<string>();

        public static object ScanAssembliesForObject(string nameToCheckFor, params string[] assembliesToSkip)
        {
            object result = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SeachForObject(assembly, nameToCheckFor, out result, assembliesToSkip);
                if (result != null) { break; }
            }

            return result;
        }

        private static void SeachForObject(Assembly assembly, string nameToCheckFor, out object result, params string[] assembliesToSkip)
        {
            result = null;
            string[] namesToCheckFor = null;
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            if (assembliesToSkip != null) { foreach (var assemble in assembliesToSkip) { if (skipAssemblies.Find(a => a.Contains(assemble)) == null) { skipAssemblies.AddRange(assembliesToSkip); } } }
            if (nameToCheckFor.Contains('.')) { namesToCheckFor = nameToCheckFor.Split('.'); }
            else { namesToCheckFor = new[] { nameToCheckFor }; }
            bool shouldSkip = false;
            foreach (string skippedAssembly in skipAssemblies) { if (assembly.FullName.Contains(skippedAssembly)) { shouldSkip = true; } }

            if (!skipAssemblies.Contains(assembly.FullName) || shouldSkip)
            {
                skipAssemblies.Add(assembly.FullName);

                foreach (Type type in assembly.GetTypes())
                {
                    if (namesToCheckFor.Any(a => a == type.Name))
                    {
                        foreach (MemberInfo member in type.GetMembers(memberInfoBinding))
                        {
                            if (member.MemberType == MemberTypes.Method && namesToCheckFor.Any(a => a == member.Name))
                            {
                                foreach (ParameterInfo parameter in ((MethodInfo)member).GetParameters())
                                {
                                    if (result != null) { break; }
                                    if (namesToCheckFor.Any(a => a == parameter.Name))
                                    {
                                        result = parameter; 
                                    }
                                }

                                if (result != null) { break; }
                                if (namesToCheckFor.Any(a => a == member.Name)) { result = member; }

                            }
                        }

                        if (result != null) { break; }
                        if (namesToCheckFor.Any(a => a == type.Name))
                        {
                            result = type;
                        }
                    }

                }
            }
        }
    }


    public static class AttributeScanner<TAttribute> where TAttribute : Attribute
    {
        static AttributeScanner()
        {
            targetMap = new List<Tuple<TAttribute, object>>();


            skipAssemblies = new List<string>(typeof(TAttribute).Assembly.GetReferencedAssemblies().Select(c => c.FullName));
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("System")) { skipAssemblies.Add(assembly.FullName); }
            }

            ScanAllAssemblies();
        }

        private static List<Tuple<TAttribute, object>> targetMap;

        private static List<string> skipAssemblies;

        private static void Add(TAttribute attribute, object item)
        {
            targetMap.Add(new Tuple<TAttribute, object>(attribute, item));
        }

        private static void ScanAllAssemblies()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SearchForAttributes(assembly);
            }
        }

        private static void SearchForAttributes(Assembly assembly, ClassTypes part = ClassTypes.Any)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            if (!skipAssemblies.Contains(assembly.FullName))
            {
                skipAssemblies.Add(assembly.FullName);

                if (part == ClassTypes.Any || part == ClassTypes.Assembly)
                {
                    foreach (TAttribute attr in assembly.GetCustomAttributes(typeof(TAttribute), false))
                    { Add(attr, assembly); }
                }

                foreach (Type type in assembly.GetTypes())
                {
                    if (part == ClassTypes.Any || part == ClassTypes.Type)
                    {
                        foreach (TAttribute attr in type.GetCustomAttributes(typeof(TAttribute), false))
                        { Add(attr, type); }
                    }

                    foreach (MemberInfo member in type.GetMembers(memberInfoBinding))
                    {
                        if (member.MemberType == MemberTypes.Property && (part == ClassTypes.Properties | part == ClassTypes.Any))
                        {
                            foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                            { Add(attr, member); }
                        }

                        if (member.MemberType == MemberTypes.Method && (part == ClassTypes.Methods | part == ClassTypes.Any))
                        {
                            foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                            { Add(attr, member); }


                        }

                        if (member.MemberType == MemberTypes.Method && (part == ClassTypes.Parameters | part == ClassTypes.Any))
                        {
                            foreach (ParameterInfo parameter in ((MethodInfo)member).GetParameters())
                            {
                                foreach (TAttribute attr in parameter.GetCustomAttributes(typeof(TAttribute), false))
                                { Add(attr, parameter); }
                            }
                        }

                    }
                }
            }

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                if (!skipAssemblies.Contains(assemblyName.FullName))
                {
                    try
                    {
                        SearchForAttributes(Assembly.Load(assemblyName));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
            }
        }

        public static List<Tuple<TAttribute, object>> ScanAssembliesForAttributes(ClassTypes section = ClassTypes.Any)
        {
            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

            foreach (StackFrame stackFrame in stackFrames)
            {
                SearchForAttributes(stackFrame.GetMethod().GetType().Assembly, section);
            }

            return targetMap;
        }


    }
}

