
# Publishing

There are two targets on `yarn` in the `package.json`:

* `publish-api-demo`
* `publish-app-demo`

# Cache Invalidation (GUI)

This is a release-over-the-top strategy for the demo. To invalidate caches:

1. Open Cloudfront
2. Distribution > Invalidations
3. Create Invalidation >  files/list/wildcard > Invalidate

Note: only 1000 invalidation paths (ie searches so *.js is one path) per month are free.

# Cache invalidation (cli)

```
aws cloudfront create-invalidation --distribution-id E3B418MBKZV7VN --paths /app.*
```

## References

* [cache invalidation](https://stackoverflow.com/questions/1086240/how-can-i-update-files-on-amazons-cdn-cloudfront)
* [Invalidating Files](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/Invalidation.html)
* [Invalidation cli](https://docs.aws.amazon.com/cli/latest/reference/cloudfront/create-invalidation.html)