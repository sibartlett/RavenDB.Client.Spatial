using System;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Raven.Imports.Newtonsoft.Json;
using Raven.Tests.Helpers;
using Xunit;

namespace Raven.Client.Spatial.Tests
{
    public class SpatialTestBase : RavenTestBase
    {
        public SpatialTestBase(Action<JsonSerializer> customizeJsonSerializer)
        {
            CustomizeJsonSerializer = customizeJsonSerializer;
        }

        public Action<JsonSerializer> CustomizeJsonSerializer { get; set; }

		protected void Assertion<TGeometryBase>(Func<TGeometryBase> geometry, Func<TGeometryBase, object> equalityFunc = null)
		{
			if (equalityFunc == null)
				equalityFunc = @base => @base;

            using (var store = NewDocumentStore())
            {
                store.Conventions.CustomizeJsonSerializer = CustomizeJsonSerializer;
                store.Initialize();

                using (var session = store.OpenSession())
                {
                    session.Store(new MyClass<TGeometryBase>
                    {
                        Geometry = geometry()
                    });
                    session.SaveChanges();
                }

                WaitForIndexing(store);

                using (var session = store.OpenSession())
                {
                    var doc = session.Query<MyClass<TGeometryBase>>().First();
					Assert.Equal(equalityFunc(geometry()), equalityFunc(doc.Geometry));
                }
            }
        }

        public class MyClass<T>
        {
            public string Id { get; set; }
            public T Geometry { get; set; }
        }
    }
}
