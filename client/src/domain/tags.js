import * as PooledCollection from 'semantic-network';
import {log} from 'logger';
import {makeSparseResourceFromUri} from 'semantic-network/cache/sparseResource';
import {getPooledCollection} from 'semantic-network/sync/PooledCollection';


/**
 * A pooled resource is required by a resource to be resolved but lives outside the current scope of the resource for
 * resolution. This function provides that resolution service that is plugged in via the {@link CacheOptions} when syncing
 * resources.
 *
 * @example
 *
 *  The 'todos' collection in the userCollection requires a 'tags' collection that lives outside todos
 **
 *      return get(user)
 *          .then(user =>
 *               sync({
 *                  resource: user,
 *                  rel: /todos/,
 *                  document: userDocument,
 *                  strategies: [],
 *                  options: {
 *                      ...options,
 *                      ...pooledTagResourcesResolver(tenant)
 *                  }));
 *
 * @param {LinkedRepresentation} contextResource
 * @return {CacheOptions}
 * @see ResourceFactory
 * @see ResourceResolver
 */
export function pooledTagResourceResolver(contextResource) {

    let resolve = (collectionName, collectionRel, type) =>
        (resource, options) => {
            return getPooledCollection(contextResource, collectionName, collectionRel, resource, options)
                .then(document => {
                    if (!document) {
                        log.error(`TODO: make new pooled resource: ${type} '${resource.name || ''}'`);
                    }
                    return document;
                });
        };

    return {
        //resourceFactory: linkRel => makeSparseResourceFromUri(linkRel.href, {name: linkRel.title}),
        resourceResolver: (fieldNameOrLinkRel/*, context */) => {

            const rel = {
                tag: resolve('tags', /tags/, fieldNameOrLinkRel),
            };

            if (rel[fieldNameOrLinkRel]) {
                return rel[fieldNameOrLinkRel];
            } else {
                log.info(`Unable to resolve pooled resource '${fieldNameOrLinkRel}', available: [${Object.keys(rel).join(',')}]`);
                return () => Promise.resolve(undefined);
            }
        }
    };

}
