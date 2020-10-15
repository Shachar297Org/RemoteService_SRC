#if DEBUG
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Rosslare.TestData
{
    class A { }

    // generic classes
    class B<T> { }

    class C<T1, T2> { }

    //nested classes
    class Outer<T>
    {
        public class D { }

        public class E<T1> { }

        public class F<T1, T2> { }
    }
}

namespace Logging.Tests
{
    using Rosslare.TestData;

    [TestClass]
    public class TypeExtensionsTests
    {
        [TestMethod]
        public void Can_Normalize_FullTypeNames()
        {
            var @namespace = typeof(A).Namespace;

            var allTypes = new Dictionary<Type, string>
            {
                //Simple classes
                { typeof(int), "System.Int32" },
                { typeof(List<int>), "System.Collections.Generic.List<System.Int32>" },
                { typeof(Dictionary<int, string>), "System.Collections.Generic.Dictionary<System.Int32,System.String>" },
                { typeof(Dictionary<int, List<string>>), "System.Collections.Generic.Dictionary<System.Int32,System.Collections.Generic.List<System.String>>" },
                { typeof(List<List<string>>), "System.Collections.Generic.List<System.Collections.Generic.List<System.String>>" },

                // Classes inside NonGeneric classes
                { typeof(A), @namespace + ".A" },
                { typeof(B<int>), @namespace + ".B<System.Int32>" },
                { typeof(C<int, string>), @namespace + ".C<System.Int32,System.String>" },
                { typeof(C<int, B<string>>), @namespace + ".C<System.Int32,Rosslare.TestData.B<System.String>>" },
                { typeof(B<B<string>>), @namespace + ".B<Rosslare.TestData.B<System.String>>" },

                // TODO: Will be supported later
                //// Classes inside Generic class
                //{ typeof(Outer<int>.D), @namespace + ".Outer<System.Int32>.D" },
                //{ typeof(Outer<int>.E<int>), @namespace + ".Outer<System.Int32>.E<System.Int32>" },
                //{ typeof(Outer<int>.F<int, string>), @namespace + ".Outer<System.Int32>.F<System.Int32,System.String>" },
                //{ typeof(Outer<int>.F<int, Outer<int>.E<string>>),@namespace + ".Outer<System.Int32>.F<System.Int32,Rosslare.TestData.Outer<System.Int32>.E<System.String>>" },
                //{ typeof(Outer<int>.E<Outer<int>.E<string>>), @namespace + ".Outer<System.Int32>.E<Rosslare.TestData.Outer<System.Int32>.E<System.String>>" }
            };

            foreach (var typeData in allTypes)
            {
                var type = typeData.Key;
                var expectedName = typeData.Value;
                var normlizedName = type.NormalizedTypeName();

                Assert.AreEqual(normlizedName, expectedName);
            }
        }
    }
}
#endif