using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace CoRProcessor
{
    public static class SetupCor
    {
        public static IServiceCollection AddCoR(this IServiceCollection services, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes()
                         .Where(t => IsAssignableToGenericType(t, typeof(IChainProcessor<>)) && t.IsClass))
            {
                if (type.GetGenericArguments().Length > 0) continue;
                services.AddScoped(type); 
            }
            
            return services;
        }

        public static ContainerBuilder AddCoR(this ContainerBuilder containerBuilder, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes()
                         .Where(t => IsAssignableToGenericType(t, typeof(IChainProcessor<>)) && t.IsClass))
            {
                if (type.GetGenericArguments().Length > 0) continue;
                containerBuilder.RegisterType(type).AsSelf().AsImplementedInterfaces().InstancePerLifetimeScope();
            }

            return containerBuilder;
        }
    
        private static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetTypeInfo().ImplementedInterfaces;

            if (interfaceTypes.Any(it => it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == genericType))
                return true;

            if (givenType.GetTypeInfo().IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.GetTypeInfo().BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
