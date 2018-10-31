import {PooledCollection} from 'semantic-link-cache';
import {log} from 'logger';
import {makeSparseResourceFromUri} from 'semantic-link-cache/cache/sparseResource';


/**
 * A pooled resource is required by a resource to be resolved but lives outside the current scope of the resource for
 * resolution. This function provides that resolution service that is plugged in via the {@link UtilOptions} when syncing
 * resources.
 *
 * @example
 *
 *  The 'todos' collection in the userCollection requires a 'tags' collection that lives outside todos
 **
 *      return cache
 *          .getResource(user)
 *          .then(user =>
 *               sync.getResourceInNamedCollection(
 *                  user,
 *                  'todos',
 *                  /todos/,
 *                  userDocument,
 *                  [],
 *                  {
 *                      ...options,
 *                      ...pooledTagResourcesResolver(tenant)
 *                  }));
 *
 * @param {LinkedRepresentation} contextResource
 * @return {{resourceFactory: (function(*): LinkedRepresentation), resourceResolver: (function(string):Array<function(*, *)>)}} see {@link UtilOptions.resourceFactory} and {@link UtilOptions.resourceResolver}
 */
export function pooledTagResourceResolver(contextResource) {

    let resolve = (collectionName, collectionRel, type) =>
        (resource, options) => PooledCollection
            .getPooledCollection(contextResource, collectionName, collectionRel, resource, options)
            .then(document => {
                if (document) {
                    return document;
                } else {
                    log.error(`TODO: make new pooled resource: ${type} '${resource.name || ''}'`);
                    return undefined;
                }
            });

    return {
        resourceFactory: linkRel => makeSparseResourceFromUri(linkRel.href, {name: linkRel.title}),
        resourceResolver: (type/*, context */) => {

            const rel = {
                tag: resolve('tags', /tags/, type),
            };

            if (rel[type]) {
                return rel[type];
            } else {
                log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(rel).join(',')}]`);
                return () => Promise.resolve(undefined);
            }
        }
    };

}
