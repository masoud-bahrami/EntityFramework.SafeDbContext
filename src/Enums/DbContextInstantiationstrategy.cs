namespace EntityFramework.SafeDbContext
{
    /// <summary>
    /// 
    /// </summary>
    public enum DbContextInstantiationStrategy
    {
        /// <summary>
        /// Force create new AmbientDbContext where already instantiated one or not
        /// </summary>
        CreateNewInstanceAnyway,
        /// <summary>
        /// 
        /// </summary>
        CreateNewInstanceAnywayAndScopedNewInstantiation,
        /// <summary>
        /// If already instantiated one, add new AmbientDbContext under it scope
        /// </summary>
        AppendIfOneAlreadyInstantiated
    }
}