# Spatial JsonConverters for RavenDB

___[NuGet Packages!](https://nuget.org/packages?q=Tags%3A%22ravendbspatial%22)___

Enabling you to use third-party spatial libraries with RavenDB 3.0+!

GeoJSON and WKT JsonConverters for:

* [DotSpatial](http://dotspatial.codeplex.com/)
* [Geo](https://github.com/sibartlett/Geo)
* [NetTopologySuite](https://code.google.com/p/nettopologysuite/)

## Example

Here's an example of using a WKT JsonConverter with DotSpatial/Geo/NetTopologySuite. 

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
							doc.Shape
						};

		Spatial(x => x.Shape, x => x.Geography.Default())
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
								.Spatial(x => x.WithinRadiusOf(100, new Point(0, 0)))
								.ToList();
			}
		}
	}
}
```

#### License

RavenDB.Client.Spatial is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.