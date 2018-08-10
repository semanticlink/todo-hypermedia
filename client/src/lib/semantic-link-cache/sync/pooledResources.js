
import * as cache from '../cache/cache';
import PooledCollection from './PooledCollection';
import {log} from 'logger';

/**
 * Example pooled resorce implementation
 * @param contextResource
 * @return {{resourceFactory: (function(*): LinkedRepresentation), resourceResolver: resourceResolver}}
 */
export default function (contextResource) {

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
        resourceFactory: linkRel => cache.makeSparseResourceFromUri(linkRel.href, {name: linkRel.title}),
        resourceResolver: (type/*, context */) => {

            const rels = {
                role: resolve('roles', /roles/, type),
            };

            if (rels[type]) {
                return rels[type];
            } else {
                log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(rels).join(',')}]`);
                return () => Promise.resolve(undefined);
            }
        }
    };

}
