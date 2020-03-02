# Frends.Community.MySql

FRENDS 4 Task for querying data from MySql database

- [Installing](#installing)
- [Task](#tasks)
    - [Query](#query)
- [Building](#building)
- [Contributing](#contributing)
- [Change Log](#change-log)

# Installing

You can install the task via FRENDS UI Task view, by searching for packages. You can also download the latest NuGet package from https://www.myget.org/feed/frends/package/nuget/Frends.MySql and import it manually via the Task view.
# Task

## Query

Execute queries against the MySql database.

### InputQuery Properties
| Property    | Type       | Description     | Example |
| ------------| -----------| --------------- | ------- |
| Connection string | string | MySql database connection string | `server=<<your host>>;uid=<<your username>>;pwd=<<your password>>;database=<<your database name>>;` |
| Query | string | The query to execute | `SELECT * FROM Table WHERE field = @paramName`|
| Parameters | array[Query Parameter] | Possible query parameters. See [Query Parameter](#query-parameter) |  |

#### Parameter Properties

| Property    | Type       | Description     | Example |
| ------------| -----------| --------------- | ------- |
| Name | string | Parameter name used in Query property | `username` |
| Value | string | Parameter value | `myUser` |
| Data type | enum<> | Parameter data type | `VARCHAR` |

### Options Properties

| Property    | Type       | Description     | Example |
| ------------| -----------| --------------- | ------- |
| Timeout seconds | int | Query timeout in seconds | `60` |
| Throw error on failure | bool | Specify if Exception should be thrown when an error occurs. If set to *false*, task outcome can be checked from #result.Success property. | `false` 
| MySqlTransactionIsolationLevel | enum<> | Possible Transaction isolation level values are: Default, ReadCommitted, None, Serializable, ReadUncommitted, RepeatableRead. None means that queries are not made inside a transaction. Other levels are explained in https://dev.mysql.com/doc/refman/8.0/en/innodb-transaction-isolation-levels.html | `None`

### Output
| Property    | Type       | Description     | Example |
| ------------| -----------| --------------- | ------- |
| Success | bool | Indicates wheather or no errors query is executed succesfully. Always true, if Throw error on failure is set to true. | `true` |
| Message | string | Error message. Always null, if Throw error on failure is set to true. | `true` |
| Result | JToken | Query result in JToken | `[ 
 {
  "Name": "Teela",
  "Age": 42,
  "Address": "Test road 123"
 },
 {
  "Name": "Adam",
  "Age": 42,
  "Address": null
 }
]` |

To access query result, use 
```
#result.Result
```

## ExecuteStoredProcedure

Executes StoredProcedure against the MySql database.


### InputProcedure Properties
| Property    | Type       | Description     | Example |
| ------------| -----------| --------------- | ------- |
| Connection string | string | MySql database connection string | `server=<<your host>>;uid=<<your username>>;pwd=<<your password>>;database=<<your database name>>;` |
| Execute | string | Name of the stored procedurte to be executed. | `SELECT * FROM Table WHERE field = @paramName`|
| Parameters | array[Query Parameter] | Possible query parameters. See [Query Parameter](#query-parameter) |  |

#### Parameter Properties for Execute Stored Procedure
Same as in Query: See [Query Parameter](#query-parameter) 

### Options Properties for Execute Stored Procedure
Same as in Query: See [Options Properties](#options-properties) 

### Output for Execute Stored Procedure
Same as in Query: See [Output](#output) 


# Building

Clone a copy of the repo

`git clone https://github.com/CommunityHiQ/Frends.Community.MySQL.git`

Rebuild the project

`dotnet build`

Run Tests

`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`

# Contributing
When contributing to this repository, please first discuss the change you wish to make via issue, email, or any other method with the owners of this repository before making a change.

1. Fork the repo on GitHub
2. Clone the project to your own machine
3. Commit changes to your own branch
4. Push your work back up to your fork
5. Submit a Pull request so that we can review your changes

NOTE: Be sure to merge the latest from "upstream" before making a pull request!

# Change Log

| Version | Changes |
| ----- | ----- |
| 1.0.7 | Initial version of MySql Query Task |
