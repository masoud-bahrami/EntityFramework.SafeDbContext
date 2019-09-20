using System.Collections.Concurrent;
using System.Threading;

namespace EntityFramework.SafeDbContext
{
    public static class CallContext<T>
    {
        private static readonly ConcurrentDictionary<string, AsyncLocal<T>>
            State = new ConcurrentDictionary<string, AsyncLocal<T>>();

        /// <summary>
        /// Stores a given object and associates it with the specified name.
        /// </summary>
        /// <param name="name">The name with which to associate the new item in the call context.</param>
        /// <param name="data">The object to store in the call context.</param>
        public static void SetData(string name, T data) =>
            State.GetOrAdd(name, _ => new AsyncLocal<T>()).Value = data;

        /// <summary>
        /// DeleteAsync an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <typeparam name="T">The type of the payload being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static bool DeleteDate(string name) =>
            State.TryRemove(name, out AsyncLocal<T> data);

        /// <summary>
        /// UpsertAsync an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <typeparam name="T">The type of the payload being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <param name="newData"></param>
        /// <param name="comparisionData"></param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static bool UpdateData(string name, T newData, T comparisionData) =>
            State.TryUpdate(name, new AsyncLocal<T>() { Value = newData }, new AsyncLocal<T>() { Value = comparisionData });

        /// <summary>
        /// Retrieves an object with the specified name from the <see cref="CallContext"/>.
        /// </summary>
        /// <typeparam name="T">The type of the payload being retrieved. Must match the type used when the <paramref name="name"/> was set via <see cref="SetData{T}(string, T)"/>.</typeparam>
        /// <param name="name">The name of the item in the call context.</param>
        /// <returns>The object in the call context associated with the specified name, or a default value for <typeparamref name="T"/> if none is found.</returns>
        public static T GetData(string name) =>
            State.TryGetValue(name, out AsyncLocal<T> data) ? data.Value : default(T);
    }
}