using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

    public static class DirectoryScanner
    {

        static DirectoryScanner()
        {

        }

        private static string BaseDirectory { get { return AppDomain.CurrentDomain.BaseDirectory; } }
        private static List<string> CheckedDirectories { get { return _checkedDirectories; } }
        public static Dictionary<string, Assembly> BackedUpAssemblies { get { return _backedUpAssemblies; } }

        private static List<string> _checkedDirectories = new List<string>();
        private static Dictionary<string, Assembly> _backedUpAssemblies = new Dictionary<string, Assembly>();

        public static Assembly GetBackedUpAssembly(Assembly assembly)
        {
            return BackedUpAssemblies.FirstOrDefault(a => a.Value == assembly).Value;
        }

        public static Assembly GetBackedUpAssembly(string assemblyName)
        {
            return BackedUpAssemblies.FirstOrDefault(a => a.Key == assemblyName).Value;
        }

        public static ResolveEventHandler LoadBackUpDirectoryOnEvent()
        {
            return new ResolveEventHandler(
                (a, b) =>
                    {
                        string assembly = b.Name.Split(',')[0];
                        LoadBackupAssemblies(assembly);
                        return AppDomain.CurrentDomain.GetAssembly(assembly);
                    });
        }

        private static void LoadBackupAssemblies(params string[] assemblies)
        {
            Dictionary<string, Assembly> result = new Dictionary<string, Assembly>();
            List<string> list = ScanFolderForKeywords("BACKUP", ".dll", true, 2, assemblies);

            foreach (string backup in assemblies)
            {
                if (list.Any(file => file.Contains(backup + ".dll")))
                {
                    string assemblyPath = list.FirstOrDefault(file => file.Contains(backup + ".dll"));
                    Assembly assembly = Assembly.LoadFile(assemblyPath);
                    result.Add(assembly.GetName().Name, assembly);
                }
            }

            _backedUpAssemblies = result;
        }

        private static List<string> ScanFolderForKeywords(string pathExt = null, string fileExt = null, bool searchRecursively = false, int numOfBckstps = 0, params string[] keys)
        {
            List<string> result = new List<string>();
            string targetedDirectory = BaseDirectory,
                   startingDirectory = null;


            if (numOfBckstps > 0) { targetedDirectory = BaseDirectory.StepOutOfDirectory(numOfBckstps); }
            targetedDirectory = targetedDirectory.StepIntoDirectory(pathExt, true);
            startingDirectory = targetedDirectory;

            do
            {
                string[] filesInFolder = Directory.GetFiles(targetedDirectory);
                string[] subDirectories = Directory.GetDirectories(targetedDirectory);

                if (keys.Length > 0)
                {
                    foreach (string key in keys)
                    {
                        if (fileExt != null)
                        {
                            if (filesInFolder.Any(file => file.Contains(key + fileExt)))
                            {
                                result.AddRange(filesInFolder.Where(file => file.Contains(key + fileExt) || (file.Contains(key) && file.Contains(fileExt))));
                            }
                        }
                        else if (filesInFolder.Any(file => file.Contains(key)))
                        {
                            result.AddRange(filesInFolder.Where(file => file.Contains(key)));
                        }
                    }
                }

                if (searchRecursively)
                {
                    if (subDirectories.Length > 0)
                    {
                        string intialDirectory = targetedDirectory;

                        foreach (string dir in subDirectories)
                        {
                            if (!CheckedDirectories.Contains(dir))
                            {
                                targetedDirectory = targetedDirectory.StepIntoDirectory(dir);
                                CheckedDirectories.Add(targetedDirectory);
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }

                        if (intialDirectory == targetedDirectory)
                        {
                            if (targetedDirectory == startingDirectory)
                            {
                                searchRecursively = false;
                            }
                            else
                            {
                                targetedDirectory = targetedDirectory.StepOutOfDirectory(1);
                            }
                        }
                    }
                    else
                    {
                        targetedDirectory = targetedDirectory.StepOutOfDirectory(1);
                    }
                }
                else
                {
                    searchRecursively = false;
                }

            }
            while (searchRecursively);

            return result;
        }

    }

    public static class AssemblyScanner
    {
        static AssemblyScanner()
        {
            skipAssemblies = new List<string>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft")) { skipAssemblies.Add(assembly.FullName); }
            }

            skipAssemblies.Add("Unity.Mvc5");

        }

        private static List<string> skipAssemblies = null;

        private static void SearchForObject(Assembly assembly, string nameToCheckFor, out object result, string[] assembliesToLookFor, string[] assembliesToSkip)
        {

            result = null;
            string[] namesToCheckFor = null;
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
            bool shouldSkip = false;

            if (assembliesToLookFor == null) { assembliesToLookFor = new string[0]; }
            if (assembliesToSkip == null) { assembliesToSkip = new string[0]; }
            if (assembliesToSkip != null) { foreach (var assemble in assembliesToSkip) { if (skipAssemblies.Find(a => a.Contains(assemble)) == null) { skipAssemblies.AddRange(assembliesToSkip); } } }
            if (nameToCheckFor.Contains('.')) { namesToCheckFor = nameToCheckFor.Split('.'); }
            else { namesToCheckFor = new[] { nameToCheckFor }; }

            foreach (string skippedAssembly in skipAssemblies)
            {
                if (assembly.FullName.Contains(skippedAssembly)) { shouldSkip = true; }
                else if (assembliesToLookFor != null && assembliesToLookFor.Length > 0 && !assembliesToLookFor.Any(a => a.Contains(assembly.GetName().Name))) { shouldSkip = true; }
            }

            if (!shouldSkip)
            {
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

        public static object ScanAssembliesForObject(string nameToCheckFor, string[] assembliesToLookFor = null, string[] assembliesToSkip = null)
        {
            object result = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SearchForObject(assembly, nameToCheckFor, out result, assembliesToLookFor, assembliesToSkip);
                if (result != null) { break; }
            }

            return result;
        }

    }


    public static class AttributeScanner<TAttribute> where TAttribute : Attribute
    {
        private static List<Tuple<TAttribute, object>> _targetMap;
        private static List<string> _skipAssemblies;
        private static Func<Assembly, bool> _assembliesToSkip;

        static AttributeScanner()
        {
            _targetMap = new List<Tuple<TAttribute, object>>();
            _skipAssemblies = new List<string>(typeof(TAttribute).Assembly.GetReferencedAssemblies().Select(c => c.FullName));

        }

        public static List<Tuple<TAttribute, object>> ScanAssembliesForAttributes(ClassTypes section = ClassTypes.Any, Type type = null, Func<Assembly, bool> assembliesToSkip = null)
        {
            _assembliesToSkip = assembliesToSkip;

            if (_targetMap.Count < 0)
                if (type == null)
                    ScanAllAssemblies(section);

                else
                    ScanType(type, section);

            return (_targetMap.Count > 0) ? _targetMap : null;
        }

        private static void Add(TAttribute attribute, object item)
        {
            _targetMap.Add(new Tuple<TAttribute, object>(attribute, item));
        }

        private static void AddSkippedAssemblies()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_assembliesToSkip != null && _assembliesToSkip(assembly)) { _skipAssemblies.Add(assembly.FullName); }
            }

        }

        private static void ScanType(Type typeToScan, ClassTypes classPart)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;


            if (classPart == ClassTypes.Any || classPart == ClassTypes.Type)
                foreach (TAttribute attr in typeToScan.GetCustomAttributes(typeof(TAttribute), false))
                    Add(attr, typeToScan);


            foreach (MemberInfo member in typeToScan.GetMembers(memberInfoBinding))
            {
                if (member.MemberType == MemberTypes.Property && (classPart == ClassTypes.Properties | classPart == ClassTypes.Any))
                    foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, member);


                if (member.MemberType == MemberTypes.Method && (classPart == ClassTypes.Methods | classPart == ClassTypes.Any))
                    foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, member);


                if (member.MemberType == MemberTypes.Method && (classPart == ClassTypes.Parameters | classPart == ClassTypes.Any))
                    foreach (ParameterInfo parameter in ((MethodInfo)member).GetParameters())
                        foreach (TAttribute attr in parameter.GetCustomAttributes(typeof(TAttribute), false))
                            Add(attr, parameter);

            }
        }

        private static void ScanAllAssemblies(ClassTypes classPart = ClassTypes.Any)
        {
            AddSkippedAssemblies();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SearchForAttributes(assembly, classPart);
            }
        }

        private static void SearchForAttributes(Assembly assembly, ClassTypes classPart = ClassTypes.Any, Type typeToCheck = null)
        {
            bool shouldSkip = false;

            try
            {
                foreach (string skippedAssembly in _skipAssemblies) { if (assembly.FullName.Contains(skippedAssembly)) { shouldSkip = true; } }

                if (typeToCheck != null)
                {
                    ScanType(typeToCheck, classPart);
                }
                else if (!shouldSkip)
                {
                    if (classPart == ClassTypes.Any || classPart == ClassTypes.Assembly)
                    {
                        foreach (TAttribute attr in assembly.GetCustomAttributes(typeof(TAttribute), false))
                        { Add(attr, assembly); }
                    }

                    foreach (Type type in assembly.GetTypes())
                    {
                        ScanType(type, classPart);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

    }


}

