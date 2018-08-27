
# Publishing

There are two targets on `yarn` in the `package.json`:

* `publish-api-demo`
* `publish-app-demo`

# Cache Invalidation

This is a release-over-the-top strategy for the demo. To invalidate caches:

1. Open Cloudfront
2. Distribution > Invalidations
3. Create Invalidation >  files/list/wildcard > Invalidate

## References

* [cache invalidation](https://stackoverflow.com/questions/1086240/how-can-i-update-files-on-amazons-cdn-cloudfront)