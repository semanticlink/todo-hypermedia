
# Development



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
