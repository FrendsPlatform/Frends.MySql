# Frends.MySQL.ExecuteQuery
Frends MySQL task to execute query.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.MySQL/actions/workflows/ExecuteQuery_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.MySQL/actions)
![MyGet](https://img.shields.io/myget/frends-tasks/v/Frends.MySQL.ExecuteQuery)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.MySQL/Frends.MySQL.ExecuteQuery|main)

# Installing

You can install the Task via Frends UI Task View or you can find the NuGet package from the following NuGet feed https://www.myget.org/F/frends-tasks/api/v2.

## Building


Rebuild the project

`dotnet build`

Run tests

Setup MySQL to docker:
`docker run -p 3306:3306 -e MYSQL_ROOT_PASSWORD=my-secret-pw -d mysql`

`dotnet test`


Create a NuGet package

`dotnet pack --configuration Release`