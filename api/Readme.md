
# Development

## Prerequisites

* GraphViz
* PlantUML

## Optional

* Docker
* gfm

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

# Links

* https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.html

