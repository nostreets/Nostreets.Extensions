using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Practices.Unity;
using System.Diagnostics;

namespace NostreetsInterceptor.Utilities
{
    public static class Extend
    {
        public static MethodInfo GetMethodInfo<T, T2>(this Expression<Func<T, T2>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static MethodInfo GetMethodInfo<T>(this T obj, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance) where T : new()
        {
            return typeof(T).GetMethod(methodName, searchSettings);
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return type.GetMethod(methodName, searchSettings);
        }

        public static MethodInfo GetMethodInfo(this string fullMethodName)
        {
            return (MethodInfo)fullMethodName.ScanAssembliesForObject();
        }

        public static List<Tuple<TAttribute, object>> GetObjectsWithAttribute<TAttribute>(this List<Tuple<TAttribute, object>> obj, ClassTypes types = ClassTypes.Any) where TAttribute : Attribute
        {
            return AttributeScanner<TAttribute>.ScanAssembliesForAttributes(types);
        }

        public static List<TAttribute> GetAttributes<TAttribute>(this List<TAttribute> obj) where TAttribute : Attribute
        {
            List<TAttribute> result = new List<TAttribute>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes()) { result.Add(item.Item1); }

            return result;
        }

        public static List<object> GetObjectsWithAttribute<TAttribute>(this List<object> obj) where TAttribute : Attribute
        {
            List<object> result = new List<object>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes()) { result.Add(item.Item2); }

            return result;
        }

        public static object Instantiate(this Type type)
        {

            return Activator.CreateInstance(type);
        }

        public static T Instantiate<T>(this T type)
        {
            return Activator.CreateInstance<T>();
        }

        public static T UnityInstantiate<T>(this T type, UnityContainer containter)
        {
            return containter.Resolve<T>();
        }

        public static object UnityInstantiate(this Type type, UnityContainer containter)
        {
            return containter.Resolve(type);
        }

        public static object ScanAssembliesForObject(this string nameToCheckFor, params string[] assembliesToSkip)
        {
            object result = AssemblyScanner.ScanAssembliesForObject(nameToCheckFor, assembliesToSkip);
            return result;
        }

    }
}
