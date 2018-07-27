'use strict';
import {nodMaker} from './NODMaker';
import Collection from './Collection';
import log from './Logger';

/**
 *
 * @param {LinkedRepresentation} resource
 * @param {NODMaker} nodMaker
 * @param {Collection} nodCollection
 * @param {Logger} $log
 * @return {{resourceFactory: (function(*): LinkedRepresentation), resourceResolver: (function(*))}}
 */
export default class PooledResource {

    /**
     * @param {LinkedRepresentation} resource
     */
    constructor(resource) {
        this.resource = resource;

        /**
         * Registered relations to be resolved
         *
         * @example
         *
         * {
         *    role: this.resolve('roles', /roles/, type),
         *    colour: this.resolve('colours', /colours/, type),
         *    resource: this.resolve('resources', /resources/, type),
         *  }
         *
         * @type {{}}
         */
        this.relsToResolve = {};
    }

    /**
     *
     * @param {string} collectionName
     * @param {string|RegExp|string[]|RegExp[]} collectionRel
     * @param {string} type
     * @returns {function(LinkedRepresentation, *):Promise.<LinkedRepresentation>|undefined}
     * @private
     */
    resolve(collectionName, collectionRel, type) {
        return (resource, options) => Collection
            .getResourceInNamedCollection(resource, collectionName, collectionRel, resource, options)
            .then(document => {
                if (document) {
                    return document;
                } else {
                    log.error(`TODO: make new pooled resource: ${type} '${resource.name || ''}'`);
                    return undefined;
                }
            });
    }

    /**
     *
     * @param {Link} linkRel
     * @returns {LinkedRepresentation}
     */
    resourceFactory(linkRel) {
        return nodMaker.makeSparseResourceFromUri(linkRel.href, {name: linkRel.title});
    }

    /**
     * Add rel to resolve. This loads up the rels to be matched in the resource resolver
     *
     * @example
     *
     *  registerRelToResolve('roles', /roles/, type)
     *
     * @param {string} collectionName
     * @param {string|RegExp|string[]|RegExp[]} collectionRel
     * @param type
     */
    registerRelToResolve(collectionName, collectionRel, type) {
        this.relsToResolve[type] = this.resolve(collectionName, collectionRel, type);
    }

    /**
     *
     * @param {string} type
     * @returns {*}
     */
    resourceResolver(type/*, context */) {

        if (this.relsToResolve[type]) {
            return this.relsToResolve[type];
        } else {
            log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(this.relsToResolve).join(',')}]`);
            return () => Promise.resolve(undefined);
        }
    }
}

export default function (contextResource) {

    let resolve = (collectionName, collectionRel, type) =>
        (resource, options) => Collection
            .getResourceInNamedCollection(contextResource, collectionName, collectionRel, resource, options)
            .then(document => {
                if (document) {
                    return document;
                } else {
                    log.error(`TODO: make new pooled resource: ${type} '${resource.name || ""}'`);
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
                $log.info(`Unable to resolve pooled resource '${type}', available: [${Object.keys(this.relsToResolve).join(',')}]`);
                return () => Promise.resolve(undefined);
            }
        }
    };

}

export let pooledResource = (resource) => new PooledResource(resource);