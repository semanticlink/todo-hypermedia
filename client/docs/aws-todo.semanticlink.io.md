Setting up aws s3 and cloudflare to serve up static javascript files for the client api.

# Prerequisites

* (Setup aws)[aws-configure.md]
* set up DNS/CNAME (cdn.semanticlink.io) pointing to the cloudfront distribution

# S3 Make bucket with domain

Create a bucket that *matches* the name of domain

```bash
$ aws s3 mb s3://todo.semanticlink.io

make_bucket: todo.semanticlink.io

```


# S3 Sync

## Upload

```bash
aws s3 sync ./dist/app/ s3://todo.semanticlink.io
```
Check the files are there (`ls`):

```bash
$ aws s3 ls s3://todo.semanticlink.io
2018-08-24 18:11:23     155990 app.js
2018-08-24 18:12:14        460 index.html
2018-08-24 18:11:23      24354 vendors~app.842187.js
```

## Set Permissions

At this point the files are not public.

1. Go to Service (s3) > Bucket > Permssions (tab) > Bucket Policy
2. Add JSON (below) > Save
  This bucket has public access. You have provided public access to this bucket. We highly recommend that you never grant any kind of public access to your S3 bucket.
3. Individually make each file "Make public"

```json
{
  "Version":"2012-10-17",
  "Statement":[{
	"Sid":"PublicReadGetObject",
        "Effect":"Allow",
	  "Principal": "*",
      "Action":["s3:GetObject"],
      "Resource":["arn:aws:s3:::todo.semanticlink.io/*"
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
$ aws s3 mb s3://cdn.semanticlink.io

$ aws s3 ls
2018-08-22 16:28:44 cdn.semanticlink.io
2018-02-15 14:37:37 todo-rest-aws

```

## AWS sync/upload

```
$aws s3 sync ./dist/ s3://semantic-link --exclude "*" --include "*api*.js"

upload: dist/api.js to s3://semantic-link/api.js
upload: dist/vendors~api.js to s3://semantic-link/vendors~api.js
upload: dist/vendors~api~app.js to s3://semantic-link/vendors~api~app.js
```