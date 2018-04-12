using NostreetsExtensions.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Configuration;

namespace NostreetsExtensions
{
    public static class Static
    {
        #region Static Methods

        /// <summary>
        /// Gets the local ip addresses.
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetLocalIPAddresses()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);

            IPAddress[] addr = ipEntry.AddressList;

            return addr;
        }

        /// <summary>
        /// Gets the objects with attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="types">The types.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<Tuple<TAttribute, object, Assembly>> GetObjectsWithAttribute<TAttribute>(ClassTypes section) where TAttribute : Attribute
        {
            List<Tuple<TAttribute, object, Assembly>> result = new List<Tuple<TAttribute, object, Assembly>>();
            List<Project> projects = GetSolutionProjects();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (Project proj in projects)
                    foreach (var item in scanner.ScanForAttributes(Assembly.Load(proj.ProjectName), section))
                        result.Add(new Tuple<TAttribute, object, Assembly>(item.Item1, item.Item2, item.Item4));

            return result;
        }

        /// <summary>
        /// Gets the objects by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="section">The section.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<object> GetObjectsByAttribute<TAttribute>(ClassTypes section) where TAttribute : Attribute
        {
            List<object> result = new List<object>();
            List<Project> projects = GetSolutionProjects();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (Project proj in projects)
                    foreach (var item in scanner.ScanForAttributes(Assembly.Load(proj.ProjectName), section))
                        result.Add(item.Item2);

            return result;
        }

        /// <summary>
        /// Gets the methods by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>() where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();
            List<Project> projects = GetSolutionProjects();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (Project proj in projects)
                    foreach (var item in scanner.ScanForAttributes(Assembly.Load(proj.ProjectName), ClassTypes.Methods))
                        result.Add((MethodInfo)item.Item2);

            return result;
        }

        /// <summary>
        /// Gets the types by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<Type> GetTypesByAttribute<TAttribute>() where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();
            List<Project> projects = GetSolutionProjects();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (Project proj in projects)
                    foreach (var item in scanner.ScanForAttributes(Assembly.Load(proj.ProjectName), ClassTypes.Type))
                        result.Add((Type)item.Item2);

            return result;
        }

        /// <summary>
        /// Gets the properties by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertiesByAttribute<TAttribute>() where TAttribute : Attribute
        {
            List<PropertyInfo> result = null;
            List<Project> projects = GetSolutionProjects();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (Project proj in projects)
                    foreach (var item in scanner.ScanForAttributes(Assembly.Load(proj.ProjectName), ClassTypes.Properties))
                        result.Add((PropertyInfo)item.Item2);

            return result.ToList();
        }

        public static void UpdateWebConfig(string key, string value)
        {
            // Get the configuration.
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            bool doesKeyExist = false;

            foreach (KeyValueConfigurationElement item in config.AppSettings.Settings)
                if (item.Key == key)
                    doesKeyExist = true;


            if (!doesKeyExist)
                config.AppSettings.Settings.Add(key, value);
            else
                config.AppSettings.Settings[key].Value = value;

            // Save to the file,
            config.Save(ConfigurationSaveMode.Minimal);
        }

        public static string SolutionPath()
        {
            string solutionDirPath = Assembly.GetCallingAssembly().CodeBase.StepOutOfDirectory(3);

            return solutionDirPath.ScanForFilePath(null, "sln");
        }

        public static Solution GetSolution()
        {
            return new Solution(SolutionPath());
        }

        public static List<Project> GetSolutionProjects()
        {
            return GetSolution().Projects;
        }

        #endregion
    }
}
