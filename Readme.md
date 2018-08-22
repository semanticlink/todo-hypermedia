
# Development

* Rider (api)
* WebStorm (client)
* [.NET Core 2.1 SDK v.2.1.401](https://www.microsoft.com/net/download)

Api instructions [here]() 
Client instructions [here]()

# AWS deployment

## Prerequisites

* `dotnet lamdba` must be installed (`dotnet new -i Amazon.Lambda.Templates::*`) 
* In AWS, created a user/role with credentials and permissions
* In AWS, create a stack
* In AWS, certificates in ACM (Amazon Certificate Manager) and linked to API Gateway

## Residuals

* http --> https
* access on the root (currently errors `Missing Authentication Token`)

## Restore dependencies
```
    dotnet restore
```


##Deploy application

Deploy the application only from the `Api` folder (note this is folder of the Api.csproj ie `todo-rest/api/Api`)

```
cd Api
dotnet lambda deploy-serverless --framework netcoreapp2.1 --s3-bucket todo-rest-aws --stack-name DotNetCoreServerless
```

# Links

* https://github.com/aws/aws-lambda-dotnet

# Appendices

## Error: `dotnet-lambda not found`

You are in wrong in the folder. Ensure you follow instructions above to be in `todo-rest/api/Api`.

```
$ dotnet lambda help
No executable found matching command "dotnet-lambda"
```

## Install `dotnet lambda`

```
$ dotnet new -i Amazon.Lambda.Templates::*
  Restoring packages for ~/.templateengine/dotnetcli/v2.1.301/scratch/restore.csproj...
  Installing Amazon.Lambda.Templates 3.2.0.
  Generating MSBuild file ~/.templateengine/dotnetcli/v2.1.301/scratch/obj/restore.csproj.nuget.g.props.
  Generating MSBuild file ~/.templateengine/dotnetcli/v2.1.301/scratch/obj/restore.csproj.nuget.g.targets.
  Restore completed in 5.57 sec for ~/.templateengine/dotnetcli/v2.1.301/scratch/restore.csproj.
  
  Usage: new [options]
  
  Options:
    -h, --help          Displays help for this command.
    -l, --list          Lists templates containing the specified name. If no name is specified, lists all templates.
    -n, --name          The name for the output being created. If no name is specified, the name of the current directory is used.
    -o, --output        Location to place the generated output.
    -i, --install       Installs a source or a template pack.
    -u, --uninstall     Uninstalls a source or a template pack.
    --nuget-source      Specifies a NuGet source to use during install.
    --type              Filters templates based on available types. Predefined values are "project", "item" or "other".
    --force             Forces content to be generated even if it would change existing files.
    -lang, --language   Filters templates based on language and specifies the language of the template to create.
  
  
  Templates                                                 Short Name                              Language          Tags                                 
  ---------------------------------------------------------------------------------------------------------------------------------------------------------
  Order Flowers Chatbot Tutorial                            lambda.OrderFlowersChatbot              [C#]              AWS/Lambda/Function                  
  Lambda Detect Image Labels                                lambda.DetectImageLabels                [C#], F#          AWS/Lambda/Function                  
  Lambda Empty Function                                     lambda.EmptyFunction                    [C#], F#          AWS/Lambda/Function                  
  Lex Book Trip Sample                                      lambda.LexBookTripSample                [C#]              AWS/Lambda/Function                  
  Lambda Simple DynamoDB Function                           lambda.DynamoDB                         [C#], F#          AWS/Lambda/Function                  
  Lambda Simple Kinesis Firehose Function                   lambda.KinesisFirehose                  [C#]              AWS/Lambda/Function                  
  Lambda Simple Kinesis Function                            lambda.Kinesis                          [C#], F#          AWS/Lambda/Function                  
  Lambda Simple S3 Function                                 lambda.S3                               [C#], F#          AWS/Lambda/Function                  
  Lambda Simple SQS Function                                lambda.SQS                              [C#]              AWS/Lambda/Function                  
  Lambda ASP.NET Core Web API                               serverless.AspNetCoreWebAPI             [C#], F#          AWS/Lambda/Serverless                
  Lambda ASP.NET Core Web Application with Razor Pages      serverless.AspNetCoreWebApp             [C#]              AWS/Lambda/Serverless                
  Serverless Detect Image Labels                            serverless.DetectImageLabels            [C#], F#          AWS/Lambda/Serverless                
  Lambda DynamoDB Blog API                                  serverless.DynamoDBBlogAPI              [C#]              AWS/Lambda/Serverless                
  Lambda Empty Serverless                                   serverless.EmptyServerless              [C#], F#          AWS/Lambda/Serverless                
  Lambda Giraffe Web App                                    serverless.Giraffe                      F#                AWS/Lambda/Serverless                
  Serverless Simple S3 Function                             serverless.S3                           [C#], F#          AWS/Lambda/Serverless                
  Step Functions Hello World                                serverless.StepFunctionsHelloWorld      [C#], F#          AWS/Lambda/Serverless                
  Console Application                                       console                                 [C#], F#, VB      Common/Console                       
  Class library                                             classlib                                [C#], F#, VB      Common/Library                       
  
  ...
  
```

## .NET Core inspect

### Windows

``` dotnet  --info
   .NET Core SDK (reflecting any global.json):
    Version:   2.1.301
    Commit:    59524873d6
   
   Runtime Environment:
    OS Name:     Windows
    OS Version:  10.0.17134
    OS Platform: Windows
    RID:         win10-x64
    Base Path:   C:\Program Files\dotnet\sdk\2.1.301\
   
   Host (useful for support):
     Version: 2.1.3
     Commit:  124038c13e
   
   .NET Core SDKs installed:
     2.1.200 [C:\Program Files\dotnet\sdk]
     2.1.301 [C:\Program Files\dotnet\sdk]
   
   .NET Core runtimes installed:
     Microsoft.AspNetCore.All 2.1.1 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.All]
     Microsoft.AspNetCore.App 2.1.1 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
     Microsoft.NETCore.App 2.0.7 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
     Microsoft.NETCore.App 2.1.1 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
     Microsoft.NETCore.App 2.1.3 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
   
   To install additional .NET Core runtimes or SDKs:
     https://aka.ms/dotnet-download
```

### MacOS

```
$ dotnet --info
.NET Core SDK (reflecting any global.json):
 Version:   2.1.401
 Commit:    91b1c13032

Runtime Environment:
 OS Name:     Mac OS X
 OS Version:  10.13
 OS Platform: Darwin
 RID:         osx.10.13-x64
 Base Path:   /usr/local/share/dotnet/sdk/2.1.401/

Host (useful for support):
  Version: 2.1.3
  Commit:  124038c13e

.NET Core SDKs installed:
  2.1.200 [/usr/local/share/dotnet/sdk]
  2.1.301 [/usr/local/share/dotnet/sdk]
  2.1.401 [/usr/local/share/dotnet/sdk]

.NET Core runtimes installed:
  Microsoft.AspNetCore.All 2.1.1 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.All 2.1.3 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.All]
  Microsoft.AspNetCore.App 2.1.1 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
  Microsoft.AspNetCore.App 2.1.3 [/usr/local/share/dotnet/shared/Microsoft.AspNetCore.App]
  Microsoft.NETCore.App 2.0.7 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.1 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]
  Microsoft.NETCore.App 2.1.3 [/usr/local/share/dotnet/shared/Microsoft.NETCore.App]

To install additional .NET Core runtimes or SDKs:
  https://aka.ms/dotnet-download
```
