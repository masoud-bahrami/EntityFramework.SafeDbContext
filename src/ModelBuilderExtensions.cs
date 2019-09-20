using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace EntityFramework.SafeDbContext
{
    public static class ModelBuilderExtensions
    {
        public static void RegisterDbSetsConfig(this ModelBuilder modelBuilder, Assembly assembly)
        {
            var applyConfigMethod = typeof(ModelBuilder).GetMethods()
                .Where(e => e.Name == "ApplyConfiguration"
                            && e.GetParameters().Single().ParameterType.Name == typeof(IEntityTypeConfiguration<>).Name)
                .Single();

            foreach (var item in GetAllImplementingIEntityTypeConfiguration(assembly))
            {
                var entityType = item.ImplementedInterfaces.Single().GenericTypeArguments.Single();
                var applyConfigGenericMethod = applyConfigMethod.MakeGenericMethod(entityType);
                applyConfigGenericMethod.Invoke(modelBuilder, new object[] { Activator.CreateInstance(item) });
            }
        }

        private static List<TypeInfo> GetAllImplementingIEntityTypeConfiguration(Assembly assembly)
        {
            return assembly.DefinedTypes
                .Where(t => t.ImplementedInterfaces
                                .Any(i =>
                                    i.IsGenericType
                                    && i.Name == typeof(IEntityTypeConfiguration<>).Name)
                            && t.IsClass
                            && !t.IsAbstract
                            && !t.IsNested)
                .ToList();
        }
        public static MethodInfo EntityMethod(this ModelBuilder modelBuilder)
        {
            var method = modelBuilder.GetType().GetMethod("Entity", new Type[] { typeof(Type) });
            return method;
        }
        public static async Task RegisterDbSetsAsync(this ModelBuilder modelBuilder, Assembly _assembly)
        {
            if (_assembly == null)
                return;

            await Task.Run(() =>
            {
                var assembly = Assembly.Load(_assembly.FullName);
                var method = modelBuilder.EntityMethod();
                if (method == null) return;

                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.IsClass
                        && InheritFrom<BaseDbEntity>(type)
                        && !type.GetCustomAttributes(typeof(NotMappedAttribute), false).Any())
                    {
                        method.Invoke(modelBuilder, new[] { type });
                    }
                }
            });
        }

        private static bool InheritFrom<T>(Type type)
        {
            if (type == null)
                return false;
            if (type == typeof(T))
                return true;
            return InheritFrom<T>(type.BaseType);
        }
    }
}