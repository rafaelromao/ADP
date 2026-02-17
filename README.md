# ADP - Abstract Data Provider

> **Disclaimer**: This code was created around 2007. At that time, **Entity Framework did not exist** - it was first released in August 2008 as part of .NET Framework 3.5 SP1. This project was developed to solve the same problem that Entity Framework later addressed: providing an object-relational mapping (ORM) layer for .NET applications. This code is published for portfolio purposes only and is **not intended to be used in new projects and is not maintained**.

---

## Overview

ADP (Abstract Data Provider) is a custom ORM (Object-Relational Mapping) framework for .NET that enables rapid development of data-oriented applications. It provides a clean abstraction layer between the application business logic and the underlying database, allowing developers to work with domain-specific objects instead of raw SQL statements.

**Technology**: C# (.NET 2.0), ADO.NET, TCP/IP Sockets  
**Created**: ~2007  
**Status**: This library is posted as a portfolio project only.

## Architecture

### Multi-Layer Architecture

```
┌─────────────────────────────────────────┐
│     Client Application (WinForms)        │
├─────────────────────────────────────────┤
│     ADPObjects (Domain Model)            │
│  - ADPObject (Persistent Objects)        │
│  - ADPCollection                         │
│  - ADPSession                            │
├─────────────────────────────────────────┤
│     ADPClientLibrary (Client Proxy)      │
│  - ADPClient (IADPProvider impl)         │
│  - ADPProxy                              │
│  - ADPCommandClient                      │
├─────────────────────────────────────────┤
│     Network (TCP/IP Socket)              │
├─────────────────────────────────────────┤
│     ADPServerLibrary (Server)           │
│  - ADPServer                             │
│  - ADPCommandServer                      │
│  - ADPProvider                           │
│  - ADPConnectionPool                     │
├─────────────────────────────────────────┤
│     ADPConnectionDrivers (Data Access)   │
│  - ADPBaseConnection                     │
│  - ADPConnectionForIBProvider           │
│  - ADPConnectionForXMLDataTable         │
│  - ADPConnectionForDelimitedFile        │
└─────────────────────────────────────────┘
```

### Key Components

#### ADPCommon (Shared Types)
| Component | Description |
|-----------|-------------|
| `IADPProvider` | Interface defining database operations (Login, ExecuteSelect, ExecuteCommand, GetKey, etc.) |
| `ADPMessage` | Message format for client-server communication |
| `ADPParam` | Parameter wrapper for SQL statements |
| `ADPSerializer` | XML serialization for DataTable transport |
| `ADPStoredStatementProvider` | Loads SQL statements from external files |
| `ADPTracer` | Logging and debugging utility |

#### ADPClientLibrary (Client-Side)
| Component | Description |
|-----------|-------------|
| `ADPClient` | Implements IADPProvider; sends commands to ADPServer |
| `ADPProxy` | High-level facade for database operations |
| `ADPCommandClient` | TCP client for server communication |

#### ADPServerLibrary (Server-Side)
| Component | Description |
|-----------|-------------|
| `ADPServer` | Main server process; interprets and routes commands |
| `ADPCommandServer` | TCP listener handling concurrent client connections |
| `ADPProvider` | Implements IADPProvider; orchestrates database operations |
| `ADPConnectionPool` | Manages database connection pooling |

#### ADPConnectionDrivers (Database Abstraction)
| Component | Description |
|-----------|-------------|
| `ADPBaseConnection` | Abstract base class for connections |
| `ADPConnectionForIBProvider` | InterBase/Firebird support |
| `ADPConnectionForXMLDataTable` | XML file as database |
| `ADPConnectionForDelimitedFile` | CSV/Delimited files as database |

#### ADPObjects (Domain Model)
| Component | Description |
|-----------|-------------|
| `ADPObject` | Base class for persistent entities |
| `ADPCollection<T>` | Generic collection with binding support |
| `ADPSession` | Represents a database session; manages transactions and cache |
| `ADPPersister` | Handles object persistence (CRUD operations) |
| `ADPFilterCriteria` | Query builder for dynamic queries |

## How It Works

### 1. Connection and Session Management

```csharp
// Client-side: Create a session
ADPSession session = new ADPSession();
session.Login(connectionInfo);

// Server-side: ADPServer maintains connection pool
// and manages multiple concurrent sessions
```

### 2. Object Persistence

Developers define persistent classes by inheriting from `ADPObject`:

```csharp
public class Customer : ADPObject {
    private string name;
    public string Name {
        get { return (string)GetPropertyValue("Name"); }
        set { SetPropertyValue("Name", value); }
    }
}
```

### 3. Loading Objects

```csharp
// Load by primary key
Customer customer = new Customer(session);
customer.Load(customerKey);

// Load collection with filter
ADPCollection<Customer> customers = 
    Customer.LoadRange<Customer>(session, filterCriteria);
```

### 4. Saving Changes

```csharp
session.StartTransaction();
try {
    customer.Name = "New Name";
    customer.Persist();
    session.Commit();
} catch {
    session.Rollback();
}
```

### 5. Stored Statements

SQL statements are stored in external files and loaded by language:

```csharp
// Get statement by ID
string sql = provider.GetSQLStatement("CustomerInsert", databaseSessionID);
```

## Design Patterns

### 1. Provider Pattern
`IADPProvider` defines a contract for database operations. Multiple implementations (IB, XML, DelimitedFile) can be swapped without changing client code.

### 2. Connection Pooling
`ADPConnectionPool` manages a pool of reusable database connections, improving performance in multi-threaded scenarios.

### 3. Proxy Pattern
`ADPProxy` acts as a surrogate for `ADPClient`, providing additional logic (logging, caching) transparently.

### 4. Object-Relational Mapping
`ADPObject` and `ADPPersister` handle the translation between database rows and domain objects.

### 5. Session Pattern
`ADPSession` encapsulates a database session, transaction state, and object cache.

### 6. Lazy Loading
Properties can be marked for lazy loading - only loaded when accessed:

```csharp
// Key generation modes
public enum ADPKeyGeneration {
    Disable,
    Early,   // Generated on first access
    Lazy     // Generated on Persist()
}
```

### 7. Thread-Safe Message Processing
`ADPCommandServer` uses `ThreadPool.QueueUserWorkItem` for concurrent command processing with proper locking.

## Network Protocol

### Message Format
```
[MESSAGE_START]<XML_DATA>[CHECKSUM_START]<checksum>[MESSAGE_END]
```

### Packet Splitting
Large responses are split into multiple TCP packets with start/end markers for reassembly.

### Checksum Validation
Each message includes a checksum to ensure data integrity.

## Key Features

- **Multi-database support**: InterBase, XML files, Delimited files
- **Transaction support**: Full ACID transactions with commit/rollback
- **Connection pooling**: Optimized resource management
- **Stored statements**: SQL separated from code
- **Lazy loading**: Performance optimization for large object graphs
- **Binding support**: INotifyPropertyChanged for WinForms data binding
- **Custom key generation**: Configurable primary key generation strategies

## Comparison with Modern ORMs

| Feature | ADP (2007) | Entity Framework (Modern) |
|---------|-------------|---------------------------|
| LINQ Support | No | Yes |
| Code First | No | Yes |
| Migrations | No | Yes |
| Lazy Loading | Manual | Built-in |
| Change Tracking | Manual | Automatic |
| Migration Path | None | Use EF Core |

## Dependencies

- `System.Data` - ADO.NET base classes
- `System.Xml` - XML serialization
- `System.Net.Sockets` - TCP/IP networking

## License

Creative Commons Attribution 4.0 International (CC BY 4.0) - See LICENSE file for details.

## Value Proposition

This project demonstrates:

- **ORM Design**: Building an object-relational mapper from scratch
- **Client-Server Architecture**: TCP/IP-based distributed system design
- **Connection Pooling**: Resource management in high-concurrency scenarios
- **Design Patterns**: Application of classic patterns in real-world code
- **ADO.NET Mastery**: Low-level database access patterns
- **Threading**: Concurrent request handling with thread pools
- **Network Programming**: Custom protocol design and implementation

## Historical Context

When this code was written (~2007):
- Entity Framework didn't exist yet (released 2008)
- NHibernate was available but complex
- Most .NET applications used raw ADO.NET or DataSets
- This project attempted to simplify data access similar to what EF later provided
