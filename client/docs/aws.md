Setting up aws s3 and cloudflare to serve up static javascript files for the client api.

# Prerequisites

* install [awscli](https://aws.amazon.com/cli/)
* create security credentials
* configured with cli
* set up DNS/CNAME (api-client.goneopen.com) pointing to the cloudfront distribution

# S3 Make bucket with domain

Create a bucket that *matches* the name of domain

```bash
$ aws s3 mb s3://api-client.goneopen.com

make_bucket: api-client.goneopen.com

```


# S3 Sync

Upload

```bash
aws s3 sync ./dist/ s3://api-client.goneopen.com --exclude "*" --include "*api*.js"
```
Check the files are there (`ls`):

```bash
$ aws s3 ls s3://semantic-link
2018-08-22 19:40:48     139842 api.js
2018-08-22 19:40:48      54198 vendors~api.js
2018-08-22 19:40:48     822964 vendors~api~app.js
```

## Set Permissions

At this point the files are not public.

1. Go to Bucket > Permssions (tab) > Bucket Policy
2. Add JSON (below) > Save
3. This bucket has public access. You have provided public access to this bucket. We highly recommend that you never grant any kind of public access to your S3 bucket.

```json
{
  "Version":"2012-10-17",
  "Statement":[{
	"Sid":"PublicReadGetObject",
        "Effect":"Allow",
	  "Principal": "*",
      "Action":["s3:GetObject"],
      "Resource":["arn:aws:s3:::api-client.goneopen.com/*"
      ]
    }
  ]
}
```

# CloudFront


# Links

* [Hosting static sites with Amazon S3 and Cloudflare](https://medium.com/@thomasroest/hosting-static-sites-with-amazon-s3-and-cloudflare-c403b6fbad59)
* [Static Site Hosting with S3 and CloudFlare](https://wsvincent.com/static-site-hosting-with-s3-and-cloudflare/)

# Appendixes

##

```
$ aws s3 mb s3://semantic-link

$ aws s3 ls
2018-08-22 16:28:44 semantic-link
2018-02-15 14:37:37 todo-rest-aws

```

## AWS settings

```
$ aws configure


```

```.aws/config
[default]
output = text
region = ap-southeast-2
```

```.aws/credentials
[default]
aws_access_key_id = AKI......
aws_secret_access_key = bU0F....
````
## AWS sync/upload

```
$aws s3 sync ./dist/ s3://semantic-link --exclude "*" --include "*api*.js"

upload: dist/api.js to s3://semantic-link/api.js
upload: dist/vendors~api.js to s3://semantic-link/vendors~api.js
upload: dist/vendors~api~app.js to s3://semantic-link/vendors~api~app.js
```