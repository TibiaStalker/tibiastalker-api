﻿using System.Reflection;
using Autofac;

namespace TibiaEnemyOtherCharactersFinder.Infrastructure.Configuration
{
    public class AutofacModule: Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly()).AsImplementedInterfaces().PreserveExistingDefaults();
            builder.RegisterAssemblyTypes(Assemblies.ApplicationAssembly).AsImplementedInterfaces().PreserveExistingDefaults();
        }
    }
}