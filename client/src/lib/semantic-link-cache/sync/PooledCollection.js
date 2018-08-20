import {findResourceInCollection, findResourceInCollectionByUri} from '../mixins/collection';
import {log} from 'logger';
import * as cache from '../cache/cache';
import * as link from 'semantic-link';

/**
 * Used for provisioning a pooled collection (network of data) based on providing a document (resources). Based on
 * the differences it will resolve a resource and may create one in the process.
 *
 * A pooled collection lives outside of the current context that needs to be resolved. Examples similar to this
 * in other contexts are called meta-data or static collections. The main point is that resources from the pooled collection
 * are used by reference in the current collection.
 *
 * When pooled collection is read-only then no resources may be added from other contexts.
 *
 * @example
 *
 * import * as cache from './cache';
 * import PooledCollection from './sync/PooledCollection';
 * import {log} from 'logger';
 *
 * export default function (contextResource) {
 *
 *     let resolve = (collectionName, collectionRel, type) =>
 *         (resource, options) => PooledCollection
 *             .getResourceInNamedCollection(contextResource, collectionName, collectionRel, resource, options)
 *             .then(document => {
 *                 if (document) {
 *                     return document;
 *                 } else {
 *                     log.error(`TODO: make new pooled resource: ${type} '${resource.name || ''}'`);
 *                     return undefined;
 *                 }
 *             });
 *
 *     return {
 *         resourceFactory: linkRel => cache.makeSparseResourceFromUri(linkRel.href, {name: linkRel.title}),
 *         resourceResolver: (type) => {
 *
 *             const rels = {
 *                 role: resolve('roles', /roles/, type),
 *             };
 *
 *             if (rels[type]) {
 *                 return rels[type];
 *             } else {
 *                 log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(rels).join(',')}]`);
 *                 return () => Promise.resolve(undefined);
 *             }
 *         }
 *     };
 *
 * }
 *
 */
export default class PooledCollection {

    static get defaultResolver() {
        return {
            resolve: u => u,
            remove: () => {
            },
            add: () => {
            },
            update: () => {
            }
        };
    }

    /**
     * Add to the resolver the mapping between the new document uri and the existing nod uri
     * @param document
     * @param resource
     * @param options
     * @return {*}
     * @private
     */
    static resolve(document, resource, options) {
        if (link.matches(document, /self|canonical/)) {
            (options.resolver || PooledCollection.defaultResolver).add(
                link.getUri(document, /self|canonical/),
                link.getUri(resource, /self|canonical/));
        }
        return resource;
    }

    /**
     * Make a resource item inside a collection, add it to the resolver and
     * @param collectionResource
     * @param resourceDocument
     * @param options
     * @return {Promise} containing the created resource
     * @private
     */
    static makeAndResolveResource(collectionResource, resourceDocument, options) {
        return cache
            .createCollectionItem(collectionResource, resourceDocument, options)
            .then(createdResource => {
                log.info(`Pooled: created ${link.getUri(createdResource, /self|canonical/)}`);
                return PooledCollection.resolve(resourceDocument, createdResource, options);
            });
    }

    /**
     * Retrieves a resource from a named collection on a parent resource.
     *
     * @param {LinkedRepresentation} parentResource
     * @param {string} collectionName
     * @param {string|RegExp|string[]|RegExp[]} collectionRel
     * @param {*} resourceDocument
     * @param {UtilOptions} options
     * @return {Promise} containing the uri resource {@link LinkedRepresentation}
     */
    static getResourceInNamedCollection(parentResource, collectionName, collectionRel, resourceDocument, options) {

        //options = Object.assign({}, {mappedTitle: 'name'}, options);

        return cache
            .getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
            .then(collectionResource => {

                let parentUri = link.getUri(parentResource, /self|canonical/);

                // strategy one & two: it is simply found map it based on self and/or mappedTitle
                let existingResource = findResourceInCollection(collectionResource, resourceDocument, options.mappedTitle);

                if (existingResource) {

                    log.info(`Pooled: ${collectionName} on ${parentUri} - found: ${link.getUri(existingResource, /self|canonical/)}`);
                    return PooledCollection.resolve(resourceDocument, existingResource, options);

                } else if (link.getUri(resourceDocument, /self|canonical/)) {

                    //strategy three: check to see if self is an actual resource anyway and map it if it is, otherwise make
                    let documentURI = link.getUri(resourceDocument, /self|canonical/);
                    let resolvedUri = (options.resolver || PooledCollection.defaultResolver).resolve(documentURI);

                    if (resolvedUri !== documentURI) {
                        let tryGetResource = findResourceInCollectionByUri(collectionResource, resolvedUri);
                        if (tryGetResource) {
                            return tryGetResource;
                        } else {
                            log.error(`Resource '${resolvedUri}' is not found on ${parentUri} - very unexpected`);
                        }
                    } else {
                        return PooledCollection.makeAndResolveResource(collectionResource, resourceDocument, options);
                    }

                } else {
                    // strategy four: make if we can because we at least might have the attributes
                    log.info(`Pooled: ${collectionName} on ${parentUri} - not found}`);
                    return PooledCollection.makeAndResolveResource(collectionResource, resourceDocument, options);
                }

            });
    }

}

