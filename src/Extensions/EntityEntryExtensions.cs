using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFramework.SafeDbContext
{
    public static class EntityEntryExtensions
    {
        public static bool OfType<TType>(this EntityEntry entityEntry)
        {
            var entity = entityEntry.Entity;
            return entity.GetType() == typeof(TType);
        }
        public static bool OfType(this EntityEntry entityEntry , Type type)
        {
            var entity = entityEntry.Entity;
            return entity.GetType() == type;
        }

        public static bool SetCurrentValue(this EntityEntry entityEntry,
            string propertyName,
            object propertyValue)
        {
            entityEntry.Property(propertyName).CurrentValue = propertyValue;

            return false;
        }
        public static void SetCurrentValue<TType>(this EntityEntry entityEntry,
            string propertyName,
            object propertyValue)
        {
            if (!OfType<TType>(entityEntry))
            {
                throw new Exception($"entityEntry type not not compatible");
            }

            entityEntry.Property(propertyName).CurrentValue = propertyValue;
        }

        public static void SetCurrentValueIfInTypes(this EntityEntry entityEntry,
            List<Type> types,
            string propertyName,
            object propertyValue)
        {
            if (types.Any(x=>x==entityEntry.Entity.GetType()))
                SetCurrentValue(entityEntry, propertyName, propertyValue);
            
        }
        public static void SetCurrentValues(this EntityEntry entityEntry,
            Dictionary<string, object> nameValues)
        {
            foreach (var keyValuePair in nameValues)
            {
                SetCurrentValue(entityEntry, keyValuePair.Key, keyValuePair.Value);
            }
        }
        public static void SetCurrentValues<TType>(this EntityEntry entityEntry,
            Dictionary<string, object> nameValues)
        {
            foreach (var keyValuePair in nameValues)
            {
                SetCurrentValue<TType>(entityEntry, keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}