using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    /// <summary>
    /// Extension methods for <see cref="ILoggerFactory"/>.
    /// </summary>
    public static class ILoggerFactoryExtensions
    {
        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance using the full name of the given type.
        /// </summary>
        /// <param name="factory">The logger factory.</param>
        /// <param name="type">The logger type.</param>
        /// <returns>The <see cref="ILogger"/> instance</returns>
        public static ILogger CreateLogger(this ILoggerFactory factory, Type type)
        {
            return factory.CreateLogger(type, null);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance using the full name of the given type appending a given additional name.
        /// </summary>
        /// <param name="factory">The logger factory.</param>
        /// <param name="type">The logger type.</param>
        /// <param name="name">The logger additional name</param>
        /// <returns>The <see cref="ILogger"/> instance</returns>
        public static ILogger CreateLogger(this ILoggerFactory factory, Type type, string name)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            return factory.CreateLogger(type.NormalizedTypeName() + name);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance using the full name of the given type appending a given additional name.
        /// </summary>
        /// <typeparam name="TName">The type name</typeparam>
        /// <param name="factory">The logger factory.</param>
        /// <param name="name">The logger additional name</param>
        /// <returns>The <see cref="ILogger"/> instance</returns>
        public static ILogger CreateLogger<TName>(this ILoggerFactory factory, string name = null)
        {
            return factory.CreateLogger(typeof(TName), name);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance using the full name of the given type appending a given additional name.
        /// </summary>
        /// <typeparam name="TName">The type name</typeparam>
        /// <param name="factory">The logger factory.</param>
        /// <param name="name">The logger additional name</param>
        /// <returns>The <see cref="ILogger"/> instance</returns>
        public static ILogger CreateSubLogger(this ILoggerFactory factory, ILogger Logger, string name = null)
        {
            return factory.CreateLogger(string.Format("{0}.{1}", Logger.Name, name));
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance skipping frames up the stack to get the full name of the calling class
        /// and appending a given additional name
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="skipFrames">The number of frames up the stack to skip.</param>
        /// <param name="name">The logger additional name.</param>
        /// <returns>The <see cref="ILogger"/> instance</returns>
        public static ILogger GetCurrentClassLogger(this ILoggerFactory factory, int skipFrames = 1, string name = null)
        {
            var stackFrame = new StackFrame(skipFrames, false);
            var type = stackFrame.GetMethod().DeclaringType;

            return factory.CreateLogger(type, name);
        }
    }
}
