
# Development



# AWS deployment

## Prerequisites

* dotnet lamdba (`dotnet new -i Amazon.Lambda.Templates::*`) 
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

Deploy the application only from the `Api` folder.

```
cd Api
dotnet lambda deploy-serverless --s3-bucket todo-rest-aws --stack-name DotNetCoreServerless
```

# Links

* 