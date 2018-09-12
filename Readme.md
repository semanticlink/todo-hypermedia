
# Development

* Rider (api)
* WebStorm (client)
* [.NET Core 2.1 SDK v.2.1.401](https://www.microsoft.com/net/download)
* [nodejs](https://nodejs.org/en/) (>= 10.0.0)
* [yarn](https://yarnpkg.com/latest.msi) (1.6.0 or above)
* [Git for windows](https://git-scm.com/download/win) (2.15) (Adjust your PATH environment to use git and optional Unix tools from Windows Command Prompt)
* [deployment instructions in wiki](https://bitbucket.org/toddbrackley/todo-rest-dotnetcore/wiki/Home)

## Prerequisites 

* JetBrains (Webstorm or Rider)
   - [Plantuml](https://plugins.jetbrains.com/plugin/7017?pr=idea) - `View > Tool Button` (and ensure `Graphviz` is setup with through Plantuml settings)
   - [Github markdown (gfm)](https://plugins.jetbrains.com/plugin/7701?pr=idea)
   - [IDEA mind map](https://plugins.jetbrains.com/plugin/8045-idea-mind-map)
   
* [Graphviz/dot](http://www.graphviz.org/)

## Optional

* Docker
* gfm

# Api development (ASP.NET Core)

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
    dotnet restore
```

## Client development (javascript)

Ensure that the Api is running and then `yarn dev` will open browser in localhost:8080

```
yarn install
yarn dev
```
        

# Links

* https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html


# Demo

* [Demo site](https://todo.semanticlink.io) (test-1@semanticlink.io:1234qwerZXCV)
