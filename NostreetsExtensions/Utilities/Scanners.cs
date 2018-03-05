using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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

    public class DirectoryScanner : Disposable
    {
        public DirectoryScanner()
        {

        }

        private string BaseDirectory { get { return AppDomain.CurrentDomain.BaseDirectory; } }
        private List<string> CheckedDirectories { get { return _checkedDirectories; } }
        public Dictionary<string, Assembly> BackedUpAssemblies { get { return _backedUpAssemblies; } }

        private List<string> _checkedDirectories = new List<string>();
        private Dictionary<string, Assembly> _backedUpAssemblies = new Dictionary<string, Assembly>();

        public Assembly GetBackedUpAssembly(Assembly assembly)
        {
            return BackedUpAssemblies.FirstOrDefault(a => a.Value == assembly).Value;
        }

        public Assembly GetBackedUpAssembly(string assemblyName)
        {
            return BackedUpAssemblies.FirstOrDefault(a => a.Key == assemblyName).Value;
        }

        public ResolveEventHandler LoadBackUpDirectoryOnEvent()
        {
            return new ResolveEventHandler(
                (a, b) =>
                    {
                        string assembly = b.Name.Split(',')[0];
                        LoadBackupAssemblies(assembly);
                        return AppDomain.CurrentDomain.GetAssembly(assembly);
                    });
        }

        private void LoadBackupAssemblies(params string[] assemblies)
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

        private List<string> ScanFolderForKeywords(string pathExt = null, string fileExt = null, bool searchRecursively = false, int numOfBckstps = 0, params string[] keys)
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

    public class AssemblyScanner : Disposable
    {
        public AssemblyScanner()
        {
            skipAssemblies = new List<string>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName.Contains("System") || assembly.FullName.Contains("Microsoft")) { skipAssemblies.Add(assembly.FullName); }
            }

            skipAssemblies.Add("Unity.Mvc5");

        }

        private List<string> skipAssemblies = null;

        private void SearchForObject(Assembly assembly, string nameToCheckFor, out object result, string[] assembliesToLookFor, string[] assembliesToSkip, ClassTypes classType = ClassTypes.Any)
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
                else if (assembliesToLookFor != null && assembliesToLookFor.Length > 0 && assembliesToLookFor[0] != null && !assembliesToLookFor.Any(a => a.Contains(assembly.GetName().Name))) { shouldSkip = true; }
            }

            if (!shouldSkip)
            {
                if ((classType == ClassTypes.Assembly || classType == ClassTypes.Any) && namesToCheckFor.Any(a => assembly.FullName.Contains(a)))
                    result = assembly;

                foreach (Type type in assembly.GetTypes())
                {
                    if (namesToCheckFor.Any(a => a == type.Name))
                    {
                        if (classType == ClassTypes.Methods || classType == ClassTypes.Any)
                            foreach (MemberInfo member in type.GetMembers(memberInfoBinding))
                            {
                                if (classType == ClassTypes.Parameters || classType == ClassTypes.Any)
                                    foreach (ParameterInfo parameter in ((MethodInfo)member).GetParameters())
                                    {
                                        if (result != null)
                                            break;

                                        if (namesToCheckFor.Any(a => a == parameter.Name))
                                            result = parameter;
                                    }

                                if (result != null)
                                    break;

                                if (namesToCheckFor.Any(a => a == member.Name))
                                    result = member;

                            }

                        if (classType == ClassTypes.Properties || classType == ClassTypes.Any)
                            foreach (PropertyInfo prop in type.GetProperties())
                            {
                                if (result != null)
                                    break;

                                if (namesToCheckFor.Any(a => a == prop.Name))
                                    result = prop;
                            }

                        if (classType == ClassTypes.Constructors || classType == ClassTypes.Any)
                            foreach (ConstructorInfo construct in type.GetConstructors())
                            {
                                if (result != null)
                                    break;

                                if (namesToCheckFor.Any(a => a == construct.Name))
                                    result = construct;
                            }

                        if (result != null) { break; }

                        if (namesToCheckFor.Any(a => a == type.Name) && classType == ClassTypes.Type || classType == ClassTypes.Any)
                        {
                            result = type;
                        }
                    }

                }
            }
        }

        public object ScanAssembliesForObject(string nameToCheckFor, string[] assembliesToLookFor = null, string[] assembliesToSkip = null, ClassTypes classType = ClassTypes.Any)
        {
            object result = null;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SearchForObject(assembly, nameToCheckFor, out result, assembliesToLookFor, assembliesToSkip, classType);
                if (result != null) { break; }
            }

            return result;
        }

    }

    public class AttributeScanner<TAttribute> : Disposable where TAttribute : Attribute
    {
        private List<Tuple<TAttribute, object, Type, Assembly>> _targetMap;
        private List<string> _skipAssemblies;
        private Func<Assembly, bool> _assembliesToSkip;

        public AttributeScanner()
        {
            _targetMap = new List<Tuple<TAttribute, object, Type, Assembly>>();
            _skipAssemblies = new List<string>(typeof(TAttribute).Assembly.GetReferencedAssemblies().Select(c => c.FullName));

        }

        public IEnumerable<Tuple<TAttribute, object, Type, Assembly>> ScanAssembliesForAttributes(ClassTypes section = ClassTypes.Any, Type type = null, Func<Assembly, bool> assembliesToSkip = null)
        {
            var props = _targetMap.Where(a => type != null && a.Item3 == type);
            _assembliesToSkip = assembliesToSkip;

            if (props.Count() == 0)
                if (type == null)
                    ScanAllAssemblies(section);

                else
                    ScanType(type, section);

            return (props.Count() == 0) ? _targetMap : _targetMap.Where(a => type != null && a.Item3 == type);
        }

        private void Add(TAttribute attribute, object item, Type type, Assembly assembly)
        {
            _targetMap.Add(new Tuple<TAttribute, object, Type, Assembly>(attribute, item, type, assembly));
        }

        private void AddSkippedAssemblies()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (_assembliesToSkip != null && _assembliesToSkip(assembly)) { _skipAssemblies.Add(assembly.FullName); }
            }

        }

        private void ScanType(Type typeToScan, ClassTypes classPart)
        {
            const BindingFlags memberInfoBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;


            if (classPart == ClassTypes.Any || classPart == ClassTypes.Type)
                foreach (TAttribute attr in typeToScan.GetCustomAttributes(typeof(TAttribute), false))
                    Add(attr, typeToScan, typeToScan, typeToScan.Assembly);


            foreach (MemberInfo member in typeToScan.GetMembers(memberInfoBinding))
            {
                if (member.MemberType == MemberTypes.Property && (classPart == ClassTypes.Properties | classPart == ClassTypes.Any))
                    foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, member, typeToScan, typeToScan.Assembly);


                if (member.MemberType == MemberTypes.Method && (classPart == ClassTypes.Methods | classPart == ClassTypes.Any))
                    foreach (TAttribute attr in member.GetCustomAttributes(typeof(TAttribute), false))
                        Add(attr, member, typeToScan, typeToScan.Assembly);


                if (member.MemberType == MemberTypes.Method && (classPart == ClassTypes.Parameters | classPart == ClassTypes.Any))
                    foreach (ParameterInfo parameter in ((MethodInfo)member).GetParameters())
                        foreach (TAttribute attr in parameter.GetCustomAttributes(typeof(TAttribute), false))
                            Add(attr, parameter, typeToScan, typeToScan.Assembly);

            }
        }

        private void ScanAllAssemblies(ClassTypes classPart = ClassTypes.Any)
        {
            AddSkippedAssemblies();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                SearchForAttributes(assembly, classPart);
            }
        }

        private void SearchForAttributes(Assembly assembly, ClassTypes classPart = ClassTypes.Any, Type typeToCheck = null)
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
                        { Add(attr, assembly, typeof(Assembly), assembly); }
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

