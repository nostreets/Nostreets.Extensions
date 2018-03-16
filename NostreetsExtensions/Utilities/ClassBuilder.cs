using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.Utilities
{
    public class ClassBuilder
    {
        AssemblyName _asemblyName = null;

        ClassBuilder(string className)
        {
            _asemblyName = new AssemblyName(className);
        }

        public static Type CreateType(string className, string[] propertyNames, Type[] types, Tuple<string, Type, object[]>[] attributes = null)
        {
            ClassBuilder builder = new ClassBuilder(className);
            return builder.CreateType(propertyNames, types, attributes);
        }

        public static object CreateObject(string className, string[] propertyNames, Type[] types, Tuple<string, Type, object[]>[] attributes = null)
        {
            ClassBuilder builder = new ClassBuilder(className);
            return builder.CreateObject(propertyNames, types, attributes);
        }

        private Type CreateType(string[] propertyNames, Type[] types, Tuple<string, Type, object[]>[] attributes = null)
        {

            if (propertyNames.Length != types.Length)
                throw new Exception("The number of property names should match their corresopnding types number");

            else
            {
                if (propertyNames.Length != types.Length)
                    throw new Exception("The number of property names should match their corresopnding types number");

                TypeBuilder dynamicClass = CreateClass();
                CreateConstructor(dynamicClass);


                for (int ind = 0; ind < propertyNames.Count(); ind++)
                    CreateProperty(dynamicClass, propertyNames[ind], types[ind]
                                  , (attributes != null && attributes.Any(a => a.Item1 == propertyNames[ind]))
                                     ? attributes.Where(a => a.Item1 == propertyNames[ind]).ToArray()
                                     : null); 


                return dynamicClass.CreateType();
            }
        }

        private object CreateObject(string[] propertyNames, Type[] types, Tuple<string, Type, object[]>[] attributes = null)
        {
            Type type = null;
            if (propertyNames.Length != types.Length)
                throw new Exception("The number of property names should match their corresopnding types number");

            else
                type = CreateType(propertyNames, types, attributes);

            return Activator.CreateInstance(type);
        }

        private TypeBuilder CreateClass()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(_asemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(_asemblyName.FullName
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , null);
            return typeBuilder;
        }

        private void CreateConstructor(TypeBuilder typeBuilder)
        {
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType, Tuple<string, Type, object[]>[] attributes)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();


            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });
            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);


            if (attributes != null)
                foreach (var attribute in attributes)
                {
                    Type[] attrParams = attribute.Item3.Select(a => a.GetType()).ToArray();
                    ConstructorInfo attrConstructor = attribute.Item2.GetConstructor(attrParams) 
                                        ?? attribute.Item2.GetConstructors().Where((a, b) => a.GetParameters().Length == attrParams.Length && attrParams[b] == a.GetParameters()[b].ParameterType).FirstOrDefault()
                                        ?? attribute.Item2.GetConstructors()[0];

                    CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(attrConstructor, attribute.Item3);
                    propertyBuilder.SetCustomAttribute(attrBuilder);
                }
        }
    }
}
