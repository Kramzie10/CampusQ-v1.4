This folder contains simple repository classes that use `System.Data.SqlClient` to persist data to SQL Server.

- `DbConfig` contains `ConnectionString` and `EnsureDatabaseAndTables()` which creates the database and tables if missing.
- `UserRepository` provides `GetAll`, `Add`, and `Remove` for `Users`.
- `QueueRepository` provides `GetAll`, `Add`, and `Remove` for `Queue`.

Usage:
- Adjust `DbConfig.ConnectionString` to point to your SQL Server instance.

Note: For production use, add parameterization, connection pooling configuration and better error handling.
