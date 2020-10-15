using System;
using System.Linq;
using System.Text;

namespace Logging
{
    /// <summary>
    /// Type extension methods for name normalization
    /// </summary>
    static class TypeExtensions
    {
        /// <summary>
        /// Gets a normalized name for the given type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The normalized type name</returns>
        public static string NormalizedTypeName(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericName = type.GetGenericTypeDefinition().FullName.Replace('+', '.');
                var genericNameIdx = genericName.IndexOf('`');
                var genericNameArguments = string.Join(",", type.GetGenericArguments().Select(NormalizedTypeName));

                return string.Format("{0}<{1}>", genericName.Substring(0, genericNameIdx), genericNameArguments);
            }

            return type.FullName.Replace('+', '.');
        }
    }
}
