
# Development

* Rider
* [.NET Core 2.1 SDK v.2.1.401](https://www.microsoft.com/net/download)
* [Git for windows](https://git-scm.com/download/win) (2.15) (Adjust your PATH environment to use git and optional Unix tools from Windows Command Prompt)
* [deployment instructions in wiki](https://github.com/semanticlink/todo-hypermedia/wiki/Home)

## Prerequisites 

* JetBrains (Rider)
   - [Plantuml](https://plugins.jetbrains.com/plugin/7017?pr=idea) - `View > Tool Button` (and ensure `Graphviz` is setup with through Plantuml settings)
   - [Github markdown (gfm)](https://plugins.jetbrains.com/plugin/7701?pr=idea)
   - [IDEA mind map](https://plugins.jetbrains.com/plugin/8045-idea-mind-map)
   
* [Graphviz/dot](http://www.graphviz.org/)

## Optional

* Docker
* gfm

# Api development

## Setup DynamoDb

In Docker:

You need to have DynamoDB up and running on localhost:8000. You can easily do this by running the below Docker command:

`docker run -p 8000:8000 dwmkerr/dynamodb -sharedDb -inMemory`

Alternatively, if want a browser GUI:

`docker run -p 8000:8000 -it --rm instructure/dynamo-local-admin` (open browser on http://localhost:8000)

# Local deployment

Build and run the project with the following environment variables. This ensures that we are not running in AWS and the 
local port is 5000.

* *ASPNETCORE_ENVIRONMENT*: Development
* *ASPNETCORE_URLS*: http://localhost:5000

## Restore dependencies
```
cd api
dotnet restore
```

Run the `Api` project through the IDE. For example, using Rider:

* Exec Path: `Api/bin/Debug/netcoreapp2.1/Api.dll`
* Working Directory: `./api/Api`
* Environment Variables: `ASPNETCORE_ENVIRONMENT=Development;ASPNETCORE_URLS=http://localhost:5000`        

# Links

* https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html

> Note: if browsing the API through the browser (ie HTML representation) then the client application must be running [see client]((https://github.com/semanticlink/client/readme.md))