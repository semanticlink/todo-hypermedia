
# Development

## Prerequisites

* local version of [DynamoDb (jar)](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)
* Java JDK
* Modify Header Value (HTTP Headers)
* GraphViz

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

## Setup Modify Header Value (HTTP Headers)

These instructions for Mozilla Firefox and is the recommended approach. This plugin works best 
because you can narrow header modification down specifically to sites that you are browsing the
network of data. This avoid situations where you forget that you have the headers modified and a site
stops working (eg GitHub).

* In Firefox, install the extension, Modify Header Value (HTTP Headers)
* Add a new line items:
  * *URL*: `http://locahost:5000/` 
  * *Header Name*: `Accept`
  * *Header Value*: `application/json;q=0.95`
 
Then update the line item to ensure the [X] Add is checked. 

# Local deployment

Build and run the project with the following environment variables. This ensures that we are not running in AWS and the 
local port is 5000.

* *ASPNETCORE_ENVIRONMENT*: Development
* *ASPNETCORE_URLS*: http://localhost:5000

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