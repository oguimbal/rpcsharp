using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Rpcsharp.Proxying
{
    namespace Private
    {
        public interface IReferenceSetter
        {
            void SetReference(string reference);
        }
    }

    /// <summary>
    /// A type which automatically implements dumb interfaces, with getters/setters bound to a field, and methods throwing an exception.
    /// </summary>
    static class InterfaceImplementer
    {
        const MethodAttributes GetSetAttr = MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName | MethodAttributes.Virtual;
        
        static readonly ModuleBuilder ModuleBuilder;
        static readonly Dictionary<Type, Type> Implementations = new Dictionary<Type, Type>();
        static readonly PropertyInfo ReferenceProp = ExpressionUtilities.GetProperty<IRpcRoot>(r => r.Reference);
        static readonly MethodInfo StringEquals = ExpressionUtilities.GetCalledMethod(() => string.Equals("", ""));
        static readonly MethodInfo ObjectEquals = ExpressionUtilities.GetCalledMethod<object>(x=>x.Equals(null));


        static InterfaceImplementer()
        {
            var name = new AssemblyName {Name = "RpcSharpDynamicTypes"};
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assembly.DefineDynamicModule(name.Name, name.Name + ".dll", false);
        }

        static class Creators<T>
        {
            public static Func<T> Create;
        }

        public static T Create<T>()
            where T:IRpcRoot
        {
            if (Creators<T>.Create != null)
                return Creators<T>.Create();

            var implemented = Implement(typeof (T));
            Creators<T>.Create = Expression.Lambda<Func<T>>(Expression.New(implemented.GetConstructor(new Type[0])))
                                            .Compile();
            return Creators<T>.Create();
        }
        

        static Type Implement(Type interfaceType)
        {
            lock (Implementations)
            {
                Type generated;
                if (Implementations.TryGetValue(interfaceType, out generated))
                    return generated;

                // create the type
                var typeName = interfaceType.FullName + "<>Dynamic";
                var type = ModuleBuilder.DefineType(
                    typeName
                    , attr: TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit | TypeAttributes.AutoLayout
                    , parent: typeof(object)
                    , interfaces: new Type[] { interfaceType }
                    );
                type.AddInterfaceImplementation(interfaceType);
                type.AddInterfaceImplementation(typeof(Private.IReferenceSetter));

                // implement IEquatable<self>
                var equalsMethod = ImplementEquatable(type, interfaceType);

                // override object.Equals
                {
                    var eq = CreateMethod(type, ObjectEquals);
                    var il = eq.GetILGenerator();
                    Label lab;

                    // check null
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Brfalse, lab = il.DefineLabel());
                    il.Emit(OpCodes.Ldc_I4_0); // return false if object is null
                    il.Emit(OpCodes.Ret);

                    il.MarkLabel(lab);

                    // check same reference
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Brfalse, lab =  il.DefineLabel());
                    il.Emit(OpCodes.Ldc_I4_1); // return true if reference equals
                    il.Emit(OpCodes.Ret);

                    il.MarkLabel(lab);

                    // make 'as' this type
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Isinst, interfaceType);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Brfalse, lab = il.DefineLabel());
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Ldc_I4_0); // return false if the 'as' is null
                    il.Emit(OpCodes.Ret);

                    il.MarkLabel(lab);

                    // call comparer
                    il.Emit(OpCodes.Callvirt, equalsMethod);
                    il.Emit(OpCodes.Ret);
                }
                
                // define an empty constructor
                {
                    var ci = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
                    var il = ci.GetILGenerator();
                    il.Emit(OpCodes.Ret);
                }

                // Implement each property
                foreach (PropertyInfo pinfo in interfaceType
                                                    .GetProperties()
                                                    .Union(interfaceType
                                                            .GetInterfaces()
                                                            .Where(x=>x.Assembly!=typeof(IRpcRoot).Assembly
                                                        ).SelectMany(x => x.GetProperties())))
                {
                    //  Creates a backing field
                    var field = type.DefineField("_" + pinfo.Name, pinfo.PropertyType, FieldAttributes.PrivateScope);
                        
                    var pb = type.DefineProperty(pinfo.Name, PropertyAttributes.None, pinfo.PropertyType, Type.EmptyTypes);

                    // Replicate property attributes
                    ReplicateAttributes(pinfo, pb.SetCustomAttribute);

                    // Creates a getter (if required)
                    if (pinfo.GetGetMethod() != null)
                    {
                        var getMethod = type.DefineMethod(
                            "get_" + pinfo.Name
                            , attributes: GetSetAttr | MethodAttributes.Public
                            , returnType: pinfo.PropertyType
                            , parameterTypes: Type.EmptyTypes);

                        var il = getMethod.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, field);
                        il.Emit(OpCodes.Ret);
                        pb.SetGetMethod(getMethod);
                        type.DefineMethodOverride(getMethod, pinfo.GetGetMethod());
                    }

                    // Creates a setter (public if required)
                    {
                        var hasSet = pinfo.GetSetMethod() != null;
                        MethodBuilder setMethod = type.DefineMethod(
                            "set_" + pinfo.Name
                            , attributes: hasSet ? (GetSetAttr | MethodAttributes.Public) : GetSetAttr
                            , returnType: typeof(void)
                            , parameterTypes: new[] {pinfo.PropertyType});

                        // Sets the backing field
                        var il = setMethod.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldarg_1);
                        il.Emit(OpCodes.Stfld, field);
                        il.Emit(OpCodes.Ret);
                        pb.SetSetMethod(setMethod);

                        // todo: Save a diff on setter, in order to propagate it to server. See issue #8

                        if (hasSet)
                            type.DefineMethodOverride(setMethod, pinfo.GetSetMethod());
                    }
                }


                // Implement remote methods
                foreach (var remoteMethod in interfaceType
                    .GetMethods()
                    .Union(interfaceType
                            .GetInterfaces()
                            .Where(x=>x.Assembly!=typeof(IRpcRoot).Assembly && (!x.IsGenericType || x.GetGenericTypeDefinition() != typeof(IEquatable<>)))
                            .SelectMany(x => x.GetMethods())
                        ).Where(m => !m.IsSpecialName)
                    )
                {
                    var remoteMethodBuilder = CreateMethod(type, remoteMethod);

                    // Creates a dummy implementation that just throws an exception.
                    var il = remoteMethodBuilder.GetILGenerator();
                    il.ThrowException(typeof(CannotCallRemoteMethodException));

                    // Replicate method attributes
                    ReplicateAttributes(remoteMethod, remoteMethodBuilder.SetCustomAttribute);
                }

                // implement IRpcRoot.Reference
                {
                    //  Creates a backing field
                    var field = type.DefineField("<>ref", ReferenceProp.PropertyType, FieldAttributes.PrivateScope);
                        
                    var pb = type.DefineProperty(ReferenceProp.Name, PropertyAttributes.None, ReferenceProp.PropertyType, Type.EmptyTypes);
                    var getRefBuilder = CreateMethod(type, ReferenceProp.GetGetMethod());
                    var il = getRefBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, field);
                    il.Emit(OpCodes.Ret);

                    pb.SetGetMethod(getRefBuilder);


                    var setRefBuilder = CreateMethod(type, typeof(Private.IReferenceSetter).GetMethods().Single());
                    il = setRefBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldarg_1);
                    il.Emit(OpCodes.Stfld, field);
                    il.Emit(OpCodes.Ret);
                    
                    
                }

                generated = type.CreateType();
                Implementations.Add(interfaceType, generated);
                return generated;
            }
        }

        static MethodBuilder CreateMethod(TypeBuilder type, MethodInfo implementedMethod)
        {
            var remoteMethodBuilder = type.DefineMethod(
                implementedMethod.Name
                , attributes: ( implementedMethod.IsPublic ? MethodAttributes.Public : MethodAttributes.FamANDAssem) | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual
                , returnType: implementedMethod.ReturnType
                , parameterTypes: implementedMethod
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray());
            type.DefineMethodOverride(remoteMethodBuilder, implementedMethod);
            return remoteMethodBuilder;
        }

        static MethodInfo ImplementEquatable(TypeBuilder type, Type equatableType)
        {
            var equatable = typeof (IEquatable<>).MakeGenericType(equatableType);
            type.AddInterfaceImplementation(equatable);

            MethodBuilder equalsMethod = type.DefineMethod(
                "Equals"
                , attributes: MethodAttributes.Public | MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual
                , returnType: typeof(bool)
                , parameterTypes: new[] { equatableType });

            type.DefineMethodOverride(equalsMethod, equatable.GetMethod("Equals", new[] { equatableType }));
            var il = equalsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, ReferenceProp.GetGetMethod());
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, ReferenceProp.GetGetMethod());
            il.Emit(OpCodes.Call, StringEquals);
            il.Emit(OpCodes.Ret);

            return equalsMethod;
        }

        static void ReplicateAttributes(MemberInfo pinfo, Action<CustomAttributeBuilder> setattr)
        {
            foreach (var a in pinfo.GetCustomAttributesData())
            {
                setattr(new CustomAttributeBuilder(
                    a.Constructor
                    , constructorArgs:  a.ConstructorArguments.Select(x => x.Value).ToArray()
                    , namedProperties:  a.NamedArguments.Where(x => !x.IsField).Select(x => (PropertyInfo)x.MemberInfo).ToArray()
                    , propertyValues:   a.NamedArguments.Where(x => !x.IsField).Select(x => x.TypedValue.Value).ToArray()
                    , namedFields:      a.NamedArguments.Where(x => x.IsField).Select(x => (FieldInfo)x.MemberInfo).ToArray()
                    , fieldValues:      a.NamedArguments.Where(x => x.IsField).Select(x => x.TypedValue.Value).ToArray()
                    ));
            }
        }
    }
}
