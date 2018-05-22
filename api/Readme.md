
# Development

## Prerequisites

* local version of [DynamoDb (jar)](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html)
* Java JDK
* Modify Header Value (HTTP Headers)
* GraphViz

## Setup DynamoDb

* From the folder of the extracted files for dynamoDb (run in shell or setup a run configuration)

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

Alternatively, spin up docker:

You need to have DynamoDB up and running on localhost:8000. You can easily do this by running the below Docker command:

`docker run -p 8000:8000 dwmkerr/dynamodb -sharedDb -inMemory`

## Setup MySql

`docker run --name identity-mysql -e MYSQL_USER=root -e MYSQL_PASSWORD=1 -e MYSQL_DATABASE=webapijwt -e MYSQL_ROOT_PASSWORD=secret -p 3306:3306 -d mysql/mysql-server:5.7`

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

# Manually creating a session

```bash
curl -X GET http://localhost:5000/tenant?q=rewire.example.nz \
 -H 'cache-control: no-cache' \
 -H 'accept: application/json'

curl -X POST  http://localhost:5000/tenant/88e8efb3-9f41-4908-87b4-93eb8bc5f7c7/user/ \
 -H 'cache-control: no-cache' \
 -H 'content-type: application/json' \
 -d '{"Email": "me@ozgur.dk", "Password": "SomeSecurePassword123!"}'

curl -X POST  http://localhost:5000/authenticate \
 -H 'cache-control: no-cache' \
 -H 'content-type: application/json' \
 -d '{"Email": "me@ozgur.dk", "Password": "SomeSecurePassword123!"}'

curl -X GET  http://localhost:5000/tenant/88e8efb3-9f41-4908-87b4-93eb8bc5f7c7/todo/ \
 -H 'authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJtZUBvemd1ci5kayIsImp0aSI6ImI0ODBmNDA0LTdhYzktNDAwMy04ZWRjLTA0MzVlYjg4YmYwNSIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL25hbWVpZGVudGlmaWVyIjoiOWJlNjViNWQtOTA3OS00MDA5LThlYjMtMDA3YmQzZmY4MmEzIiwiZXhwIjoxNTI5MTk4NDYyLCJpc3MiOiJodHRwOi8veW91cmRvbWFpbi5jb20iLCJhdWQiOiJodHRwOi8veW91cmRvbWFpbi5jb20ifQ.vesSfv1ki9EOQw9JqoBUmc5_NN_UAVnYBLO0-ltLwk8' \
 -H 'cache-control: no-cache' \
 -H 'accept: application/json'
 
 
```