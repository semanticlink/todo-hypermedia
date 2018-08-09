'use strict';
import {nodMaker} from '../lib/semanticLink/NODMaker';
import PooledCollection from '../lib/semanticLink/sync/PooledCollection';
import {log} from 'logger';


/**
 * A pooled resource is required by a resource to be resolved but lives outside the current scope of the resource for
 * resolution. This function provides that resolution service that is plugged in via the {@link UtilOptions} when syncing
 * resources.
 *
 * @example
 *
 *  The 'todos' collection in the userCollection requires a 'tags' collection that lives outside todos
 *
 *      // extend the options with the resolver
 *      options = {...options, ...pooledTagResourcesResolver(tenant)};
 *
 *      return this.nodMaker
 *          .getResource(user)
 *          .then(user =>
 *               sync.getResourceInNamedCollection(user, 'todos', /todos/, userDocument, [], options));
 *
 * @param {LinkedRepresentation} contextResource
 * @return {{resourceFactory: (function(*): LinkedRepresentation), resourceResolver: (function(string):Array<function(*, *)>)}} see {@link UtilOptions.resourceFactory} and {@link UtilOptions.resourceResolver}
 */
export function pooledTagResourceResolver(contextResource) {

    let resolve = (collectionName, collectionRel, type) =>
        (resource, options) => PooledCollection
            .getResourceInNamedCollection(contextResource, collectionName, collectionRel, resource, options)
            .then(document => {
                if (document) {
                    return document;
                } else {
                    log.error(`TODO: make new pooled resource: ${type} '${resource.name || ''}'`);
                    return undefined;
                }
            });

    return {
        resourceFactory: linkRel => nodMaker.makeSparseResourceFromUri(linkRel.href, {name: linkRel.title}),
        resourceResolver: (type/*, context */) => {

            const rels = {
                tag: resolve('tags', /tags/, type),
            };

            if (rels[type]) {
                return rels[type];
            } else {
                log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(this.relsToResolve).join(',')}]`);
                return () => Promise.resolve(undefined);
            }
        }
    };

}
