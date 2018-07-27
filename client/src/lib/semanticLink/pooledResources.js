'use strict';
import {nodMaker} from './NODMaker';
import Collection from './Collection';
import log from './Logger';

/**
 * Example pooled resorce implementation
 * @param contextResource
 * @return {{resourceFactory: (function(*): LinkedRepresentation), resourceResolver: resourceResolver}}
 */
export default function (contextResource) {

    let resolve = (collectionName, collectionRel, type) =>
        (resource, options) => Collection
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
