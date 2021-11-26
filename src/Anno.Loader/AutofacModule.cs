﻿using System;
using System.Linq;
using Autofac;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Anno.Loader
{
    public class AutofacModule : Autofac.Module
    {
        //注意以下写法
        //builder.RegisterType<GuidTransientAnnoService>().As<IGuidTransientAnnoService>();
        //builder.RegisterType<GuidScopedAnnoService>().As<IGuidScopedAnnoService>().InstancePerLifetimeScope();
        //builder.RegisterType<GuidSingletonAnnoService>().As<IGuidSingletonAnnoService>().SingleInstance();

        protected override void Load(ContainerBuilder builder)
        {
            // The generic ILogger<TCategoryName> service was added to the ServiceCollection by ASP.NET Core.
            // It was then registered with Autofac using the Populate method in ConfigureServices.

            //builder.Register(c => new ValuesService(c.Resolve<ILogger<ValuesService>>()))
            //    .As<IValuesService>()
            //    .InstancePerLifetimeScope();
            // builder.RegisterType<BaseRepository>().As<IBaseRepository>();
            Const.AppSettings.IocDll.Distinct().ToList().ForEach(d =>
            {
                RegisterAssembly(builder, Const.Assemblys.Dic[d]);
            });
            foreach (var assembly in Const.Assemblys.DependedTypes)
            {
                RegisterAssembly(builder, assembly);
            }
        }
        private void RegisterAssembly(ContainerBuilder builder, Assembly assembly)
        {
            assembly.GetTypes().Where(x => x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract && !x.GetTypeInfo().IsInterface).ToList().ForEach(
                   t =>
                   {
                       if (t.GetCustomAttribute<NotInInjectAttribute>() != null)
                       {
                           return;
                       }
                       //if (CheckIfAnonymousType(t))
                       //{
                       //    return;
                       //}
                       var interfaces = t.GetInterfaces();
                       if (IsAssignableFrom(t, "Anno.EngineData.BaseModule")
                       || interfaces.ToList().Exists(i => i.Name == "IFilterMetadata")
                       || interfaces.Length <= 0)
                       {
                           if (t.IsGenericType)
                           {
                               builder.RegisterGeneric(t);
                           }
                           else
                           {
                               builder.RegisterType(t);
                           }
                       }
                       else if (!interfaces.ToList().Exists(i => i.Name == "IEntity"))
                       {
                           if (t.IsGenericType)
                           {
                               builder.RegisterGeneric(t).As(t.GetInterfaces());
                           }
                           else
                           {
                               builder.RegisterType(t).As(t.GetInterfaces());
                           }
                       }
                   });
        }
        internal static bool IsAssignableFrom(Type type, string baseTypeFullName)
        {
            bool success = false;
            if (type == null)
            {
                success = false;
            }
            else if (type.FullName == baseTypeFullName)
            {
                success = true;
            }
            else if (type.BaseType != null)
            {
                success = IsAssignableFrom(type.BaseType, baseTypeFullName);
            }
            return success;
        }
        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.Name.StartsWith("<>c__"))
            {
                return true;
            }
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
