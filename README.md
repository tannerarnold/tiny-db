# TinyDb

Dead simple file based table storage written in C#.

## Purpose

Every C# or .NET database provider has to use LINQ, or somehow uses SQL to query data. For getting up off the ground with a simple prototype, I figured there was a niche that was solely missing: A single file database which doesn't use SQL.

* **Dead Simple**: None of the bloat of SQL, or the need to use LINQ. For basic CRUD operations (get all entities from a database, get one by id, insert, update, and delete), it only takes two lines of code.
* **Very Performant**: Because the operations happen in memory, this database provides very rapid operations. It even allows thread-safe concurrency.
* **Robust Serialization**: Everything in the file is stored in JSON as bytes, which makes anything that can be serialized as JSON can be stored in this database.

## Building From Source

This requires the dotnet command to build, as well as the .NET 8 runtime installed on your machine.

Clone the repository and build using a single set of commands:
```bash
git clone git@github.com:tannerarnold/tiny-db.git
cd tinydb
dotnet build
```

## Basic Usage

```cs
using TinyDb;

await using Database<Entity> database = new("db.db");
Entity newEntity = await database.InsertAsync(new Entity("Jane Doe"));
Entity[] = await database.AllAsync();
```

## Documentation

### AllAsync

```cs
Entity[] entities = await database.AllAsync();
```

AllAsync will retrieve all the entities from the database and return an array of entities.

### ByIdAsync

```cs
Entity? entity = await database.ByIdAsync(1);
```

ByIdAsync will retrieve an entity by its Id value, which is an integer.

### InsertAsync

```cs
Entity entity = await database.InsertAsync(new Entity("Jane Doe"));
```

InsertAsync will insert an entity into the database, assigning it a new Id and returning the entity with the assigned Id.

### UpdateAsync

```cs
Entity entity = await database.UpdateAsync(entity);
```

UpdateAsync will update the entity in the database, and return back the updated entity.

### DeleteAsync

```cs
await database.DeleteAsync(entity);
```

DeleteAsync will delete the entity from the database.

### Saving / Persisting Data

Whenever a call to Insert, Update, or Delete happens, or when the database is disposed of, the database persists the changes to the file, serializing the entity to JSON.
