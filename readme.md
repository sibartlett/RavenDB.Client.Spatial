# Spatial JsonConverters for RavenDB

___[NuGet Packages!](https://nuget.org/packages?q=Tags%3A%22ravendbspatial%22)___

Enabling you to use third-party spatial libraries with RavenDB!

GeoJSON and WKT JsonConverters for:

* [DotSpatial](http://dotspatial.codeplex.com/)
* [Geo](https://github.com/sibartlett/Geo)
* [NetTopologySuite](https://code.google.com/p/nettopologysuite/)

Note: As of 2.0, RavenDB only supports indexing WKT. GeoJSON support will hopefully come in [RavenDB 2.5](https://github.com/ayende/ravendb/pull/268).

## Example

Here's an example of using a WKT JsonConverter with DotSpatial/Geo/NetTopologySuite. 

Note: When calling SpatialGenerate in RavenDB 2.0, we have to cast to object and then to string. [RavenDB 2.5](https://github.com/ayende/ravendb/pull/268) will hopefully have better support for spatial JsonConverters.

```csharp
public class MyClass
{
	public string Id { get; set; }
	public IGeometry Shape { get; set; }
}

public class MyIndex : AbstractIndexCreationTask<MyClass>
{
	public MyIndex()
	{
		Map = shapes => from doc in docs
						select new
						{
							_ = SpatialGenerate("spatial", (string) (object) doc.Shape)
						};
	}
}

public class Program
{
	public void Main()
	{
		using (var store = new EmbeddableDocumentStore())
		{
			store.Conventions.CustomizeJsonSerializer = x => x.Converters.Add(new WktConverter());
			store.Initialize();

			using (var session = store.OpenSession())
			{
				session.Store(new MyClass { Shape = new Point(30.533, -34.543) });
				session.SaveChanges();
			}

			using (var session = store.OpenSession())
			{
				var results = session.Query<MyClass>()
								.Customize(x => x.WithinRadiusOf(100, 0, 0))
								.ToList();
			}
		}
	}
}
```

#### License

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.