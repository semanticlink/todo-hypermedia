Setting up aws s3 and cloudflare to serve up static javascript files for the client api.

# Prerequisites

* (Setup aws)[aws-configure.md]
* set up DNS/CNAME (cdn.semanticlink.io) pointing to the cloudfront distribution

0. Copy files to s3 Bucket
1. Setup AWS Certificate Manager (you must use "US East (N. Virginia)" region
2. Create a DNS entry for validation
2. Create Cloudfront distribution
3. Create DNS entry for custom domain


# S3 Make bucket with domain

Create a bucket that *matches* the name of domain

```bash
$ aws s3 mb s3://cdn.semanticlink.io

make_bucket: cdn.semanticlink.io

```


# S3 Sync

## Upload

```bash
aws s3 sync ./dist/ s3://cdn.semanticlink.io --exclude "*" --include "*api*.js"
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
      "Resource":["arn:aws:s3:::cdn.semanticlink.io/*"
      ]
    }
  ]
}
```

# Certificate Manager

Log into the AWS console and visit the AWS Certificate Manager.

IMPORTANT: Make sure you switch to the “US East (N. Virginia)” region using the region selector at the top right, CloudFront only works with certificates that are created in that “global” region.


# CloudFront

1. Open CloudFront
2. Create a distribution
   Alternate Domain Name (CNAMEs): cdn.semanticlink.io
   Custom SSL (wait until ready)

# Add DNS CNAME entry

1. cdn.semanticlink.io CNAME xxxxxx.cloudfront.net (this is the domain name in General tab in the cloudfront distribution

# Links

* [Hosting static sites with Amazon S3 and Cloudflare](https://medium.com/@thomasroest/hosting-static-sites-with-amazon-s3-and-cloudflare-c403b6fbad59)
* [Static Site Hosting with S3 and CloudFlare](https://wsvincent.com/static-site-hosting-with-s3-and-cloudflare/)
* [Configure a Custom Domain for CloudFront with HTTPS](https://deliciousbrains.com/wp-offload-s3/doc/custom-domain-https-cloudfront/)

# Appendixes

##

```
$ aws s3 mb s3://cdn.semanticlink.io

$ aws s3 ls
2018-08-22 16:28:44 cdn.semanticlink.io
2018-02-15 14:37:37 todo-rest-aws

```

### AWS sync/upload

```
$aws s3 sync ./dist/ s3://semantic-link --exclude "*" --include "*api*.js"

upload: dist/api.js to s3://semantic-link/api.js
upload: dist/vendors~api.js to s3://semantic-link/vendors~api.js
upload: dist/vendors~api~app.js to s3://semantic-link/vendors~api~app.js
```