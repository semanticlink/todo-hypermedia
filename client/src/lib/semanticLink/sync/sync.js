'use strict';
import _ from '../mixins/index';
import * as link from 'semantic-link';
import {log} from 'logger';
import * as cache from '../cache/cache';
import Differencer from './Differencer';
import axios from 'axios';
import {put} from 'semantic-link';
import {uriMappingResolver} from './UriMappingResolver';

/**
 * Internal data structure for working out which active to perform on documents.
 *
 * ** DO NOT DELETE** this is the jsdoc
 *
 * @class SyncInfo
 * @property {LinkedRepresentation} resource
 * @property {*} document
 * @property {string} action containing 'create','update', 'delete'
 * @private
 */

/**
 * @class SyncResolver
 * @property  resolve
 * @property  remove
 * @property  add
 * @property  update
 */

/**
 * Used for provisioning the resources (network of data) based on providing new document (resources). Based
 * on a difference set this class synchronises between the client version and the api
 */


/**
 * @return {SyncResolver}
 */
const defaultResolver = {
    resolve: u => u,
    remove: _.noop,
    add: _.noop,
    update: _.noop
};

/**
 * Default resource finder assumes that resources are in a collection via the 'items' attribute/array.
 * @return {function(CollectionRepresentation=, LinkedRepresentation=): LinkedRepresentation}
 */
const defaultfindResourceInCollectionStrategy = (collectionResource, resourceDocument) => _(collectionResource).findResourceInCollection(resourceDocument);

/**
 *
 * @param strategies
 * @param options
 * @param syncInfos
 * @return {Promise.<void>}
 */
export function tailRecursionThroughStrategies(strategies, options, syncInfos) {
    return _(strategies).sequentialWait((unusedMemo, strategy) => {

        if (options.childStrategyBatchSize === 0 || _(options.childStrategyBatchSize).isUndefined()) {
            // invoke a parallel strategy when want to go for it
            return _(syncInfos).mapWaitAll(syncInfo => strategy(syncInfo.resource, syncInfo.document, options));
        } else {
            // invoke a sequential strategy - and for now, single at a time
            return _(syncInfos).sequentialWait((unusedMemo2, syncInfo) => strategy(syncInfo.resource, syncInfo.document, options));
        }

    });
}

/**
 * Recurse through all the strategies working through change sets.
 * @param {*[]} strategies
 * @param {UtilOptions} options
 * @return {function(syncInfo:SyncInfo):Promise.<LinkedRepresentation>} containing the representation (@link LinkedRepresentation}
 */
export function syncInfos(strategies, options) {
    return syncInfo =>
        tailRecursionThroughStrategies(strategies, options, [syncInfo])
            .then(() => syncInfo.resource);
}

/**
 * Returns a array of uris from either uris or resources. Basically, this downgrades to uris from source representations.
 *
 * The default strategy is to look for 'self' link relation if it is a linked representation
 *
 * However, in most cases, you will need to write your own resolver, before synchronisation. In this example,
 * the resolver the notifications collection also looks into the `question-item` link relation:
 *
 *  options = _({}).extend(options, {
     *      uriListResolver: notificationCollection =>
     *          _(notificationCollection.items).map(item => this.link.getUri(item, /question-item/))
     *  });
 *
 *  getUriListOnNamedCollection(userResource, 'notifications', /notifications/, notificationUriList, options)
 *
 * TODO: the default could better use and extend representation underscore makeUriLists
 *
 * @param {CollectionRepresentation|LinkedRepresentation|string[]} uriListOrResource
 * @return {string[]}
 */
export function defaultUriListResolver(uriListOrResource) {

    if (uriListOrResource.items) {

        const uriList = [...uriListOrResource.items]
            .map(item => link.getUri(item, /self|canonical/))
            .filter(item => !!item);

        log.debug(`[Sync] Uri resolving resource items on 'self' [${uriList.join(',')}]`);
        return uriList;
    }
    else if (uriListOrResource.links) {
        const uri = link.getUri(uriListOrResource, /self|canonical/);
        log.debug(`[Sync] Uri resolving resource on 'self' ${uri}`);
        return [uri];
    } else {
        log.debug(`[Sync] Uri resolving default uri-list [${[uriListOrResource].join(',')}]`);
        // fallback to assuming is an array of uris
        return uriListOrResource;
    }
}

/**
 * see https://tools.ietf.org/html/rfc2483#section-5
 *
 * TODO: refactor out into underscore and put tests around
 * @param (string[]} uriList
 * @return {*}
 */
export function toUriListMimeTypeFormat(uriList) {
    uriList.join('\n');
}

/**
 * Update or create a resource in a collection based on a document depending on whether it previously exists or not.
 *
 * note: updates do not check for differences
 * @param collectionResource
 * @param {*} resourceDocument
 * @param {UtilOptions} options
 * @return {Promise.<SyncInfo>} contains a syncInfo
 * @private
 */
export function syncResourceInCollection(collectionResource, resourceDocument, options = {}) {

    const findResourceInCollectionStrategy = options.findResourceInCollectionStrategy || defaultfindResourceInCollectionStrategy;

    // locate the document in the collection items
    const collectionItemResource = findResourceInCollectionStrategy(collectionResource, resourceDocument);

    // check whether to update or create
    if (collectionItemResource && !options.forceCreate) {
        return cache
        // synchronise the item in the collection from the server
            .getCollectionResourceItem(collectionResource, collectionItemResource)
            .then(item => {
                // and update the resource back to the server and return it
                return cache.updateResource(item, resourceDocument, options);
            })
            .then(resource => ({
                resource: resource,
                document: resourceDocument,
                action: 'update'
            }));

    } else {
        return cache
        // add the document to the collection a
            .createCollectionResourceItem(collectionResource, resourceDocument, options)
            .then(item => {
                // ensure the resource is returned
                return cache.getResource(item);
            })
            .then(resource => ({
                resource: resource,
                document: resourceDocument,
                action: 'create'
            }));
    }
}

/**
 *
 * @param collectionResource
 * @param collectionDocument
 * @param options
 * @returns {Promise}
 * @private
 */
export function synchroniseCollection(collectionResource, collectionDocument, options = {}) {

    /**
     * @type {SyncResolver}
     */
    const resolver = options.resolver || defaultResolver;

    /**
     * Delete a resource from the local state cache
     */
    const deleteResourceAndUpdateResolver = (deleteResource) => {
        return cache
            .deleteCollectionItem(collectionResource, deleteResource, options)
            .then(result => {
                resolver.remove(link.getUri(deleteResource, /self|canonical/));
                return result;
            });
    };

    /**
     * Update a resource and remember the URI mapping so that if a reference to the
     * network of data resource is required we can resolve a document reference to
     * real resource in our network.
     */
    const updateResourceAndUpdateResolver = (updateResource, updateDataDocument) => {
        return cache.getResource(updateResource)
            .then(updateResource => cache.tryUpdateResource(updateResource, updateDataDocument))
            .then(updateResource => {
                resolver.update(
                    link.getUri(updateDataDocument, /self|canonical/),
                    link.getUri(updateResource, /self|canonical/)
                );
                return updateResource;
            });

    };

    /**
     *
     * @param {*} createDataDocument
     * @return {?LinkedRepresentation} the new resource
     */
    const createResourceAndUpdateResolver = (createDataDocument) => {
        return cache
            .createCollectionResourceItem(collectionResource, createDataDocument, options)
            .then(result => {
                resolver.add(
                    link.getUri(createDataDocument, /self|canonical/),
                    link.getUri(result, /self|canonical/));
                return result;
            });
    };

    /**
     * A contribute-only collection needs have an item removed. We send a DELETE request
     * back to the server on the collection URI with a payload containing the URI of the
     * removed item
     */
    const removeContributeOnlyResourceAndUpdateResolver = (deleteResource) => {

        options = _({}).extend(options, {
            deleteFactory: item => {
                let itemUri = link.getUri(item, /canonical|self/);
                let collectionUri = link.getUri(collectionResource, /canonical|self/);
                log.debug(`[Sync] Removing item '${itemUri}' from collection ${collectionUri}`);
                return link.delete(
                    collectionResource,
                    /canonical|self/,
                    'application/json',
                    [itemUri]);
            }
        });
        return cache
            .deleteCollectionItem(collectionResource, deleteResource, options)
            .then(result => {
                resolver.remove(link.getUri(deleteResource, /self|canonical/));
                return result;
            });

    };

    /**
     * Don't make request back to update, just remember the URI mapping so that if a reference to the
     * network of data resource is required we can resolve a document reference to the real resource in
     * our network.
     */
    const updateContributeOnlyResourceAndUpdateResolver = (collectionResourceItem, updateDataDocument) => {
        // at this point, it is the same implementation as the read-only form
        return updateReadonlyResourceAndUpdateResolver(collectionResourceItem, updateDataDocument);
    };

    /**
     * A contribute-only collection is missing a URI. This is likely to cause problems because
     * the URI will not be resolvable, because no matching resource can be found. It will then attempt to
     * add the item to the collection
     */
    const addContributeOnlyResourceAndUpdateResolver = (createDataDocument) => {

        return cache
            .createCollectionResourceItem(collectionResource, createDataDocument, options)
            .then(result => {
                resolver.add(
                    link.getUri(createDataDocument, /self|canonical/),
                    link.getUri(result, /self|canonical/));
                return result;
            });
    };

    /**
     * A read-only collection needs have an item deleted. We don't delete it
     * but can add it to our mapping resolver anyway.
     *
     * We don't expect to come in here but we will if the document supplied
     * has less items that the current network of data (likely from time to time).
     */
    const deleteReadonlyResourceAndUpdateResolver = (collectionResourceItem) => {
        resolver.remove(link.getUri(collectionResourceItem, /self|canonical/));
        return Promise.resolve(undefined);
    };

    /**
     * Don't make request back to update, just remember the URI mapping so that if a reference to the
     * network of data resource is required we can resolve a document reference to the real resource in
     * our network.
     */
    const updateReadonlyResourceAndUpdateResolver = (collectionResourceItem, updateDataDocument) => {
        resolver.update(
            link.getUri(updateDataDocument, /self|canonical/),
            link.getUri(collectionResourceItem, /self|canonical/)
        );
        return Promise.resolve(undefined);
    };

    /**
     * A read-only collection is missing a URI. This is likely to cause problems because
     * the URI will not be resolvable, because no matching resource can be found.
     */
    const createReadonlyResourceAndUpdateResolver = () => {
        return Promise.resolve(undefined);
    };

    if (options.contributeonly) {
        log.debug(`[Sync] Collection '${link.getUri(collectionResource, /self|canonical/)}' is contribute-only`);
        const contributeOnly = {
            ...options,
            ...{
                createStrategy: addContributeOnlyResourceAndUpdateResolver,
                updateStrategy: updateContributeOnlyResourceAndUpdateResolver,
                deleteStrategy: removeContributeOnlyResourceAndUpdateResolver
            }
        };

        return Differencer.diffCollection(collectionResource, collectionDocument, contributeOnly);
    }
    // If the caller has signalled that the collection is read-only, or the collection
    // if missing a 'create-form' representation then we assume that the NOD can
    // not be changed.
    else if (options.readonly || !link.matches(collectionResource, /create-form/)) {
        log.debug(`[Sync] Collection '${link.getUri(collectionResource, /self|canonical/)}' is read-only`);
        const readOnlyOptions = {
            ...options,
            ...{
                createStrategy: createReadonlyResourceAndUpdateResolver,
                updateStrategy: updateReadonlyResourceAndUpdateResolver,
                deleteStrategy: deleteReadonlyResourceAndUpdateResolver
            }
        };

        return Differencer.diffCollection(collectionResource, collectionDocument, readOnlyOptions);
    } else {
        log.debug(`[Sync] Collection  '${link.getUri(collectionResource, /self|canonical/)}' is updatable`);
        const opts = {
            ...options,
            ...{
                createStrategy: createResourceAndUpdateResolver,
                updateStrategy: updateResourceAndUpdateResolver,
                deleteStrategy: deleteResourceAndUpdateResolver
            }
        };
        return Differencer.diffCollection(collectionResource, collectionDocument, opts);
    }
}

/**
 * Retrieves a resource and synchronises (its attributes)
 * @param {LinkedRepresentation} resource
 * @param {*} resourceDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the resource {@link LinkedRepresentation}
 */
export function getResource(resource, resourceDocument, strategies, options = {}) {
    log.debug(`[Sync] resource ${link.getUri(resource, /self/)}`);

    return cache.getResource(resource, options)
        .then(resource => {
            return cache
                .updateResource(resource, resourceDocument, options)
                .then(() => _(strategies)
                    .sequentialWait((memo, strategy) => {
                        if (strategy) {

                            if (!_(strategy).isFunction()) {
                                log.warn('[Sync] Calling function has not handed in correct strategy and will not provision');
                                return Promise.resolve({});
                            }
                            return strategy(resource, resourceDocument, options);
                        }
                        return Promise.resolve({});
                    })
                    // TODO: not understood
                    .catch(err => log.warn(`Empty object ${link.getUri(err, /self/)}`))
                );
        });
}

/**
 * Retrieves a resource from a named collection on a resource.
 *
 * @param {LinkedRepresentation} parentResource
 * @param {*} resourceDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the resource {@link LinkedRepresentation}
 */
export function getResourceInCollection(parentResource, resourceDocument, strategies, options = {}) {

    log.debug(`[Sync] collection ${link.getUri(parentResource, /self/)} with '${link.getUri(resourceDocument, /self/) || resourceDocument.name}'`);

    return cache
        .getCollectionResource(parentResource, _({}).defaults(options, {mappedTitle: 'name'}))
        .then(collectionResource => this.syncResourceInCollection(collectionResource, resourceDocument, options))
        .then(syncInfos(strategies, options));
}

/**
 * Retrieves the named collection and then synchronises with the provided collection document.
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName
 * @param {string|RegExp|string[]|RegExp[]} collectionRel
 * @param {*} collectionDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the collection {@link CollectionRepresentation}
 */
export function getCollectionInNamedCollection(parentResource, collectionName, collectionRel, collectionDocument, strategies, options = {}) {

    log.debug(`[Sync] collection '${collectionName}' on ${link.getUri(parentResource, /self/)}`);

    options = {...options, ...{mappedTitle: 'name'}};

    return cache
        .getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
        .then(collectionResource => {

            if (!collectionResource) {
                log.info(`[Sync] No '${collectionName}' on resource ${link.getUri(parentResource, /self/)}`);
                return Promise.resolve(parentResource);
            }
            // in the context of the collection, synchronise the collection part of the document
            return this.synchroniseCollection(collectionResource, collectionDocument, options)
            // returns [infos, createResults, updateItems, deleteItems], we'll just have 'infos'
                .then(([syncInfos]) => {
                    return cache
                    // populate the potentially sparse collection - we need to ensure that
                    // any existing ones (old) are not stale and that any just created (sparse)
                    // are hydrated
                        .tryGetCollectionResourceItems(collectionResource, options)
                        .then(() => {
                            // each item in the collection perform the strategy
                            return tailRecursionThroughStrategies(strategies, options, syncInfos);
                        });
                })
                .then(() => collectionResource);
        });
}

/**
 * Retrieves the named collection and then synchronises with the provided named collection document in the provided parent document.
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName
 * @param {string|RegExp|string[]|RegExp[]} collectionRel
 * @param {*} parentDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the collection {@link CollectionRepresentation}
 */
export function getNamedCollection(parentResource, collectionName, collectionRel, parentDocument, strategies, options) {
    return this.getCollectionInNamedCollection(parentResource, collectionName, collectionRel, parentDocument[collectionName], strategies, options);
}

/**
 * Retrieves a resource from a named collection on a resource and then synchronises.
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName
 * @param {string|RegExp|string[]|RegExp[]} collectionRel
 * @param {*} resourceDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the resource {@link LinkedRepresentation}
 */
export function getResourceInNamedCollection(parentResource, collectionName, collectionRel, resourceDocument, strategies, options = {}) {

    log.debug(`[Sync] resource '${collectionName}' on ${link.getUri(parentResource, /self/)}`);

    return cache
    // ensure that the collection is added to the parent resource
        .getNamedCollectionResource(parentResource, collectionName, collectionRel, Object.assign({}, options, {mappedTitle: 'name'}))
        .then(collectionResource => this.syncResourceInCollection(collectionResource, resourceDocument, options))
        .then(syncInfos(strategies, options));
}

/**
 * Retrieves a singleton resource on a parent resource and updates based on a singleton
 * of the same name in the given document
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} singletonName
 * @param {string|RegExp|string[]|RegExp[]} singletonRel
 * @param {*} parentDocument
 * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions} options
 * @return {Promise} containing the collection {@link CollectionRepresentation}
 */
export function getSingleton(parentResource, singletonName, singletonRel, parentDocument, strategies, options = {}) {

    options = _({}).defaults(options, {mappedTitle: 'name'});

    log.debug(`[Sync] singleton '${singletonName}' on ${link.getUri(parentResource, /self/)}`);

    return cache
        .getResource(parentResource)
        .then(resource => cache
            .tryGetSingletonResource(resource, singletonName, singletonRel, undefined, options)
            .then(singletonResource => {
                if (!singletonResource) {
                    log.debug(`[Sync] No update: singleton '${singletonName}' not found on ${link.getUri(parentResource, /self/)}`);
                    return Promise.resolve(resource);
                } else {
                    return cache
                        .updateResource(singletonResource, parentDocument[singletonName], options)
                        .then(() => _(strategies)
                            .sequentialWait((memo, strategy) => {
                                if (strategy) {

                                    if (!_(strategy).isFunction()) {
                                        log.warn(`[Sync] Calling function has not handed in correct strategy and will not provision '${singletonName}'`, options);
                                        return Promise.resolve({});
                                    }
                                    return strategy(singletonResource, parentDocument[singletonName], options);
                                }
                                return Promise.resolve({});
                            }));
                }
            }));
}

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string[]} documentUriList
 * @param {UtilOptions} options
 * @return {Promise.Array.<SynchroniseInfo[], Array.<LinkedRepresentation[], LinkedRepresentation[]>, Array.<LinkedRepresentation[], LinkedRepresentation[]>, LinkedRepresentation[]>} syncInfos
 * @private
 */
export function synchroniseUriList(collection, documentUriList, options = {}) {

    const uriListResolver = options.uriListResolver || defaultUriListResolver;

    // KLUDGE: for now inject uriMappingResolver for everyone. Gives a resolver without configuring up
    const resolver = options.resolver || uriMappingResolver || defaultResolver;

    /**
     *
     * uri-list resolvers
     *
     */

    const createUriListAndUpdateResolver = uriList => {
        return link.post(collection, /self/, 'text/uri-list', toUriListMimeTypeFormat(uriList))
            .then(response => {
                if (response.status === 201) {
                    return axios.get(response.headers().location);
                } else {
                    return Promise.reject(new Error(`Unable to create resources '${uriList.join(',')}'`));
                }
            })
            .catch(response => {
                if (response.status === 415) {
                    log.info('Trying to create again');
                    return link.post(collection, /self/, 'application/json', uriList)
                        .then(response => {
                            if (response.status === 201) {
                                log.debug('[Sync] Created');
                                return axios.get(response.headers().location);
                            } else {
                                return Promise.reject(new Error(`Unable to create resources '${uriList.join(',')}'`));
                            }
                        })
                        .catch(() => Promise.reject(new Error(`Unable to create resources '${uriList.join(',')}'`)));
                } else {
                    return Promise.reject(new Error(`Unable to create resources '${uriList.join(',')}'`));
                }
            })
            .then(response => {

                // iterate through the result list and match on the index for url resolution
                // we are assuming that order is matched in the result
                if (response.data.items) {
                    response.data.items.forEach((item, i) => {
                        resolver.add(
                            uriList[i],
                            item.id
                        );
                    });
                } else {
                    log.error(`[Sync] Ill formed representation on GET '${response.config.url}'`);
                }
                return uriList;
            });
    };

    const deleteUriListAndUpdateResolver = uriList => {
        return link.delete(collection, /self/, 'text/uri-list', toUriListMimeTypeFormat(uriList))
            .then(response => {

                if (response.status === 200 || response.status === 204) {
                    _(uriList).each(uri => {
                        resolver.remove(uri);
                    });
                }
                else {
                    log.error(`[Sync] Unable to DELETE resources  '${uriList.join(',')}'`);
                }
                return uriList;
            })
            .catch(response => {
                if (response.status === 415) {
                    log.info('Trying to delete again');
                    return link.delete(collection, /self/, 'application/json', uriList)
                        .then(response => {

                            if (response.status === 200 || response.status === 204) {
                                log.debug('[Sync] Deleted');

                                _(uriList).each(uri => {
                                    resolver.remove(uri);
                                });
                            }
                            else {
                                log.error(`[Sync] Unable to DELETE resources  '${uriList.join(',')}'`);
                            }
                            return uriList;
                        });
                }
            });

    };

    const resourceUriList = uriListResolver(collection);

    if (options.contributeonly) {

        log.debug(`[Sync] uri-list is contribute-only: [${resourceUriList.join(',')}]`);
        throw new Error('Not implemented');
    }
    // If the caller has signalled that the collection is read-only, or the collection
    // if missing a 'create-form' representation then we assume that the NOD can
    // not be changed.
    else if (options.readonly) {

        log.debug('[Sync] uri-list is read-only: ');

        throw new Error('Not implemented');

    } else {
        log.debug(`[Sync] uri-list is updateable: [${[resourceUriList].join(',')}]`);
        const opts = {
            ...options,
            ...{
                createStrategy: createUriListAndUpdateResolver,
                updateStrategy: () => [],
                deleteStrategy: deleteUriListAndUpdateResolver
            }
        };
        return Differencer.diffUriList(resourceUriList, documentUriList, opts);
    }

}

/**
 * Retrieves a uri-list from a named collection on a resource, uses the uriListResolver to convert from
 * items in the collection (if necessary) and then synchronises with the provided uri-list (array of strings).
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} uriListName
 * @param {string|RegExp|string[]|RegExp[]} uriListRel
 * @param {*[]} uriList
 * @param {UtilOptions} options
 * @return {Promise} containing the collection resource and items {@link LinkedRepresentation} but not resolved uri-list items
 */
export function getUriListOnNamedCollection(parentResource, uriListName, uriListRel, uriList, options = {}) {

    log.debug(`[Sync] uriList '${uriListName}' on ${link.getUri(parentResource, /self/)}`);

    return cache
        .getResource(parentResource)
        .then(resource => cache
            .tryGetNamedCollectionResource(resource, uriListName, uriListRel, options)
            .then(collection => {
                if (!collection) {
                    log.info(`[Sync] No update: uri-list '${uriListName}' not found on ${link.getUri(parentResource, /self/)}`);
                    return Promise.resolve(undefined);
                } else {

                    return this.synchroniseUriList(collection, uriList, options)
                    // note discarding synch infos (not sure why)
                        .then(() => {
                            options = {...options, forceLoad: true};
                            return cache.getCollectionResourceAndItems(collection, options);
                        });
                }
            }));
}

/**
 * Retrieves a uri-list from a named collection on a resource, uses the uriListResolver to resolve
 * items in the collection (if necessary) and then PUTs the provided uri-list back on the collection.
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} uriListName
 * @param {string|RegExp|string[]|RegExp[]} uriListRel
 * @param {*[]} uriList
 * @param {UtilOptions} options
 * @return {Promise} containing the collection resource and items {@link LinkedRepresentation} but not resolved uri-list items
 */
export function patchUriListOnNamedCollection(parentResource, uriListName, uriListRel, uriList, options = {}) {

    log.debug(`[Sync] uriList '${uriListName}' on ${link.getUri(parentResource, /self/)} [${[uriList].join(',')}]`);
    return cache
        .getResource(parentResource)
        .then(resource => cache
            .tryGetNamedCollectionResource(resource, uriListName, uriListRel, options)
            .then(collection => {
                if (!collection) {
                    log.info(`[Sync] No update: uri-list '${uriListName}' not found on ${link.getUri(parentResource, /self/)}`);
                    return Promise.resolve(undefined);
                } else {
                    // make all the necessary changes to create and remove individual uri
                    return this.synchroniseUriList(collection, uriList, options)
                        .then(() => {
                            // now make it so and the server should pay nicely and accept all the changes
                            log.debug('[Sync] update collection with uri-list');
                            return cache
                                .getSingletonResource(collection, 'editForm', /edit-form/, options)
                                .then(form => put(form, /submit/, 'text/uri-list', uriList.join('\n')));
                        })
                        .then(() => cache.getCollectionResourceAndItems(collection, {...options, ...{forceLoad: true}}));
                }
            }));
}

