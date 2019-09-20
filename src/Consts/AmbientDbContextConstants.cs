using System;

namespace EntityFramework.SafeDbContext
{
    internal class AmbientDbContextConstants
    {
        private static string _ambientDbContextScopeKey;
        internal static string AmbientDbContextScopeKey
        {
            get
            {
                if (string.IsNullOrEmpty(_ambientDbContextScopeKey))
                    _ambientDbContextScopeKey = Guid.NewGuid().ToString();
                return _ambientDbContextScopeKey;
            }
        }
    }
}