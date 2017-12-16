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

        public static Type CreateType(string className, string[] propertyNames, Type[] types)
        {
            ClassBuilder builder = new ClassBuilder(className);
            return builder.CreateType(propertyNames, types);
        }

        public static object CreateObject(string className, string[] propertyNames, Type[] types)
        {
            ClassBuilder builder = new ClassBuilder(className);
            return builder.CreateObject(propertyNames, types);
        }

        private Type CreateType(string[] propertyNames, Type[] types)
        {
            if (propertyNames.Length != types.Length)
                throw new Exception("The number of property names should match their corresopnding types number");

            else
                return CreateObject(propertyNames, types).GetType();
        }

        private object CreateObject(string[] propertyNames, Type[] types)
        {
            if (propertyNames.Length != types.Length)
                throw new Exception("The number of property names should match their corresopnding types number");

            TypeBuilder dynamicClass = this.CreateClass();

            this.CreateConstructor(dynamicClass);

            for (int ind = 0; ind < propertyNames.Count(); ind++)
                CreateProperty(dynamicClass, propertyNames[ind], types[ind]);

            Type type = dynamicClass.CreateType();

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

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
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
        }
    }
}
