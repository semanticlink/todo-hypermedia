
# Development

## Prerequisites

* local version of [DynamoDb (jar)](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)
* Java jdk

## Setup DynamoDb

* From the folder of the extracted files for dynamoDb:

```shell
dynamodb_local_latest$ java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb -inMemory

Initializing DynamoDB Local with the following configuration:
Port:   8000
InMemory:       false
DbPath: null
SharedDb:       true
shouldDelayTransientStatuses:   false
CorsParams:     *
```


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

* https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html