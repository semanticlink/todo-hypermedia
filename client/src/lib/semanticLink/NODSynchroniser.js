'use strict';
import _ from './mixins/underscore';
import { link, SemanticLink } from './SemanticLink';
import log from './Logger';
import { nodMaker } from './NODMaker';
import Differencer from './Differencer';
import axios from 'axios';

// TODO: upgrade to es6 and use spread
const arrayToArguments = (functionWithArgs) => {
    return array => functionWithArgs.apply(this, array);
};

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
 * @class injectableNODSynchroniseService
 * @property  resolve
 * @property  remove
 * @property  add
 * @property  update
 */

/**
 * Used for provisioning the resources (network of data) based on providing new document (resources). Based
 * on a difference set this class synchronises between the client version and the api
 */
export default class NODSynchroniser {

    /**
     * @return {injectableNODSynchroniseService}
     */
    static get defaultResolver () {
        return {
            resolve: u => u,
            remove: _.noop,
            add: _.noop,
            update: _.noop
        };
    }

    /**
     * Default resource finder assumes that resources are in a collection via the 'items' attribute/array.
     * @return {function(CollectionRepresentation=, LinkedRepresentation=): LinkedRepresentation}
     */
    static get defaultfindResourceInCollectionStrategy () {
        return (collectionResource, resourceDocument) => _(collectionResource).findResourceInCollection(resourceDocument);
    }

    /**
     *
     * @param strategies
     * @param options
     * @param syncInfos
     * @return {Promise.<void>}
     */
    static tailRecursionThroughStrategies (strategies, options, syncInfos) {
        return _(strategies).sequentialWaitAll((unusedMemo, strategy) => {

            if (options.childStrategyBatchSize === 0 || _(options.childStrategyBatchSize).isUndefined()) {
                // invoke a parallel strategy when want to go for it
                return _(syncInfos).mapWaitAll(syncInfo => strategy(syncInfo.resource, syncInfo.document, options));
            } else {
                // invoke a sequential strategy - and for now, single at a time
                return _(syncInfos).sequentialWaitAll((unusedMemo2, syncInfo) => strategy(syncInfo.resource, syncInfo.document, options));
            }

        });
    }

    /**
     * Recurse through all the strategies working through change sets.
     * @param {*[]} strategies
     * @param {UtilOptions} options
     * @return {function(syncInfo:SyncInfo):Promise.<LinkedRepresentation>} containing the representation (@link LinkedRepresentation}
     */
    static syncInfos (strategies, options) {
        return syncInfo =>
            NODSynchroniser.tailRecursionThroughStrategies(strategies, options, [syncInfo])
                .then(() => syncInfo.resource);
    }

    /**
     * Returns a array of uris from either uris or resources. Basically, this downgrades to uris from source representations.
     *
     * The default strategy is to look for 'self' link relation if it is a linked representation
     *
     * However, in most cases, you will need to write your own resolver, beefore synchronisation. In this example,
     * the resolver the notifications collection also looks into the `question-item` link relation:
     *
     *  options = _({}).extend(options, {
     *      uriListResolver: notificationCollection =>
     *          _(notificationCollection.items).map(item => this.SemanticLink.getUri(item, /question-item/))
     *  });
     *
     *  getUriListOnNamedCollection(userResource, 'notifications', /notifications/, notificationUriList, options)
     *
     * TODO: the default could better use and extend representation underscore makeUriLists
     *
     * @param {LinkedRepresentation|string[]} uriListOrResource
     * @return {string[]}
     */
    static defaultUriListResolver (uriListOrResource) {
        if (uriListOrResource.links) {
            return [SemanticLink.getUri(uriListOrResource, /self|canonical/)];
        }
        // fallback to assuming is an array of uris
        return uriListOrResource;
    }

    /**
     * see https://tools.ietf.org/html/rfc2483#section-5
     *
     * TODO: refactor out into underscore and put tests around
     * @param (string[]} uriList
     * @return {*}
     */
    static toUriListMimeTypeFormat (uriList) {
        uriList.map(i => i + '\r\n').join('');
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
    syncResourceInCollection (collectionResource, resourceDocument, options = {}) {

        const findResourceInCollectionStrategy = options.findResourceInCollectionStrategy || NODSynchroniser.defaultfindResourceInCollectionStrategy;

        // locate the document in the collection items
        const collectionItemResource = findResourceInCollectionStrategy(collectionResource, resourceDocument);

        // check whether to update or create
        if (collectionItemResource && !options.forceCreate) {
            return nodMaker
            // synchronise the item in the collection from the server
                .getCollectionResourceItem(collectionResource, collectionItemResource)
                .then(item => {
                    // and update the resource back to the server and return it
                    return nodMaker.updateResource(item, resourceDocument, options);
                })
                .then(resource => ({
                    resource: resource,
                    document: resourceDocument,
                    action: 'update'
                }));

        } else {
            return nodMaker
            // add the document to the collection a
                .createCollectionResourceItem(collectionResource, resourceDocument, options)
                .then(item => {
                    // ensure the resource is returned
                    return nodMaker.getResource(item);
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
    synchroniseCollection (collectionResource, collectionDocument, options = {}) {

        /**
         * @type {injectableNODSynchroniseService}
         */
        const resolver = options.resolver || NODSynchroniser.defaultResolver;

        /**
         * Delete a resource from the local state cache
         */
        const deleteResourceAndUpdateResolver = (deleteResource) => {
            return nodMaker
                .deleteCollectionItem(collectionResource, deleteResource, options)
                .then(result => {
                    resolver.remove(SemanticLink.getUri(deleteResource, /self|canonical/));
                    return result;
                });
        };

        /**
         * Update a resource and remember the URI mapping so that if a reference to the
         * network of data resource is required we can resolve a document reference to
         * real resource in our network.
         */
        const updateResourceAndUpdateResolver = (updateResource, updateDataDocument) => {
            return nodMaker.getResource(updateResource)
                .then(updateResource => nodMaker.tryUpdateResource(updateResource, updateDataDocument))
                .then(updateResource => {
                    resolver.update(
                        SemanticLink.getUri(updateDataDocument, /self|canonical/),
                        SemanticLink.getUri(updateResource, /self|canonical/)
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
            return nodMaker
                .createCollectionResourceItem(collectionResource, createDataDocument, options)
                .then(result => {
                    resolver.add(
                        SemanticLink.getUri(createDataDocument, /self|canonical/),
                        SemanticLink.getUri(result, /self|canonical/));
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
                    let itemUri = SemanticLink.getUri(item, /canonical|self/);
                    let collectionUri = SemanticLink.getUri(collectionResource, /canonical|self/);
                    log.info(`Removing item '${itemUri}' from collection ${collectionUri}`);
                    return link.delete(
                        collectionResource,
                        /canonical|self/,
                        'application/json',
                        [itemUri]);
                }
            });
            return nodMaker
                .deleteCollectionItem(collectionResource, deleteResource, options)
                .then(result => {
                    resolver.remove(SemanticLink.getUri(deleteResource, /self|canonical/));
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

            return nodMaker
                .createCollectionResourceItem(collectionResource, createDataDocument, options)
                .then(result => {
                    resolver.add(
                        SemanticLink.getUri(createDataDocument, /self|canonical/),
                        SemanticLink.getUri(result, /self|canonical/));
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
            resolver.remove(SemanticLink.getUri(collectionResourceItem, /self|canonical/));
            return Promise.resolve(undefined);
        };

        /**
         * Don't make request back to update, just remember the URI mapping so that if a reference to the
         * network of data resource is required we can resolve a document reference to the real resource in
         * our network.
         */
        const updateReadonlyResourceAndUpdateResolver = (collectionResourceItem, updateDataDocument) => {
            resolver.update(
                SemanticLink.getUri(updateDataDocument, /self|canonical/),
                SemanticLink.getUri(collectionResourceItem, /self|canonical/)
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
            log.info(`Collection '${SemanticLink.getUri(collectionResource, /self|canonical/)}' is contribute-only`);
            const contributeOnly = _({}).extend(
                options, {
                    createStrategy: addContributeOnlyResourceAndUpdateResolver,
                    updateStrategy: updateContributeOnlyResourceAndUpdateResolver,
                    deleteStrategy: removeContributeOnlyResourceAndUpdateResolver
                }
            );

            return Differencer.diffCollection(collectionResource, collectionDocument, contributeOnly);
        }
        // If the caller has signalled that the collection is read-only, or the collection
        // if missing a 'create-form' representation then we assume that the NOD can
        // not be changed.
        else if (options.readonly || !SemanticLink.matches(collectionResource, /create-form/)) {
            log.info(`Collection '${SemanticLink.getUri(collectionResource, /self|canonical/)}' is read-only`);
            const readOnlyOptions = _({}).extend(
                options, {
                    createStrategy: createReadonlyResourceAndUpdateResolver,
                    updateStrategy: updateReadonlyResourceAndUpdateResolver,
                    deleteStrategy: deleteReadonlyResourceAndUpdateResolver
                }
            );

            return Differencer.diffCollection(collectionResource, collectionDocument, readOnlyOptions);
        } else {
            log.debug(`Collection  '${SemanticLink.getUri(collectionResource, /self|canonical/)}' is updatable`);
            const opts = _({}).extend(
                options, {
                    createStrategy: createResourceAndUpdateResolver,
                    updateStrategy: updateResourceAndUpdateResolver,
                    deleteStrategy: deleteResourceAndUpdateResolver
                }
            );
            return Differencer.diffCollection(collectionResource, collectionDocument, opts);
        }
    }

    /**
     * Retrieves a resource and synchronises (its atrributes)
     * @param {LinkedRepresentation} resource
     * @param {*} resourceDocument
     * @param {{function(LinkedRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
     * @param {UtilOptions} options
     * @return {Promise} containing the resource {@link LinkedRepresentation}
     */
    getResource (resource, resourceDocument, strategies, options = {}) {
        log.info(`[Sync] resource ${SemanticLink.getUri(resource, /self/)}`);

        return nodMaker.getResource(resource, options)
            .then(resource => {
                return nodMaker
                    .updateResource(resource, resourceDocument, options)
                    .then(() => _(strategies)
                        .sequentialWaitAll((memo, strategy) => {
                            if (strategy) {

                                if (!_(strategy).isFunction()) {
                                    log.warn('Calling function has not handed in correct strategy and will not provision');
                                    return Promise.resolve({});
                                }
                                return strategy(resource, resourceDocument, options);
                            }
                            return Promise.resolve({});
                        }));
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
    getResourceInCollection (parentResource, resourceDocument, strategies, options = {}) {

        log.info(`[Sync] collection ${SemanticLink.getUri(parentResource, /self/)}`);

        return nodMaker
            .getCollectionResource(parentResource, _({}).defaults(options, {mappedTitle: 'name'}))
            .then(collectionResource => this.syncResourceInCollection(collectionResource, resourceDocument, options))
            .then(NODSynchroniser.syncInfos(strategies, options));
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
    getCollectionInNamedCollection (parentResource, collectionName, collectionRel, collectionDocument, strategies, options = {}) {

        log.info(`[Sync] collection '${collectionName}' on ${SemanticLink.getUri(parentResource, /self/)}`);

        options = _({}).defaults(options, {mappedTitle: 'name'});

        return nodMaker
            .getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
            .then(collectionResource => {

                if (!collectionResource) {
                    log.info(`No '${collectionName}' on resource ${SemanticLink.getUri(parentResource, /self/)}`);
                    return Promise.resolve(parentResource);
                }
                // in the context of the collection, synchronise the collection part of the document
                return this.synchroniseCollection(collectionResource, collectionDocument, options)
                    .then(arrayToArguments(syncInfos => {
                        return nodMaker
                        // populate the potentially sparse collection - we need to ensure that
                        // any existing ones (old) are not stale and that any just created (sparse)
                        // are hydrated
                            .tryGetCollectionResourceItems(collectionResource, options)
                            .then(() => {
                                // each item in the collection perform the strategy
                                return NODSynchroniser.tailRecursionThroughStrategies(strategies, options, syncInfos);
                            });
                    }))
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
    getNamedCollection (parentResource, collectionName, collectionRel, parentDocument, strategies, options) {
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
    getResourceInNamedCollection (parentResource, collectionName, collectionRel, resourceDocument, strategies, options = {}) {

        log.info(`[Sync] resource '${collectionName}' on ${SemanticLink.getUri(parentResource, /self/)}`);

        return nodMaker
        // ensure that the collection is added to the parent resource
            .getNamedCollectionResource(parentResource, collectionName, collectionRel, Object.assign({}, options, {mappedTitle: 'name'}))
            .then(collectionResource => this.syncResourceInCollection(collectionResource, resourceDocument, options))
            .then(NODSynchroniser.syncInfos(strategies, options));
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
    getSingleton (parentResource, singletonName, singletonRel, parentDocument, strategies, options = {}) {

        options = _({}).defaults(options, {mappedTitle: 'name'});

        log.info(`[Sync] singleton '${singletonName}' on ${SemanticLink.getUri(parentResource, /self/)}`);

        return nodMaker
            .getResource(parentResource)
            .then(resource => nodMaker
                .tryGetSingletonResource(resource, singletonName, singletonRel, undefined, options)
                .then(singletonResource => {
                    if (!singletonResource) {
                        log.info(`No update: singleton '${singletonName}' not found on ${SemanticLink.getUri(parentResource, /self/)}`);
                        return Promise.resolve(resource);
                    } else {
                        return nodMaker
                            .updateResource(singletonResource, parentDocument[singletonName], options)
                            .then(() => _(strategies)
                                .sequentialWaitAll((memo, strategy) => {
                                    if (strategy) {

                                        if (!_(strategy).isFunction()) {
                                            log.warn(`Calling function has not handed in correct strategy and will not provision '${singletonName}'`, options);
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
     * @return [] syncInfos
     * @private
     */
    synchroniseUriList (collection, documentUriList, options = {}) {

        const uriListResolver = options.uriListResolver || NODSynchroniser.defaultUriListResolver;
        const resolver = options.resolver || NODSynchroniser.defaultResolver;

        /**
         *
         * uri-list resolvers
         *
         */

        const createUriListAndUpdateResolver = uriList => {
            return link.post(collection, /self/, 'text/uri-list', NODSynchroniser.toUriListMimeTypeFormat(uriList))
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
                                    log.info('Created');
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
                        log.error(`Ill formed representation on GET '${response.config.url}'`);
                    }
                    return uriList;
                });
        };

        const updateUriListAndUpdateResolver = () => {
            // not implemented/not needed
            return [];
        };

        const deleteUriListAndUpdateResolver = uriList => {
            return link.delete(collection, /self/, 'text/uri-list', NODSynchroniser.toUriListMimeTypeFormat(uriList))
                .then(response => {

                    if (response.status === 200 || response.status === 204) {
                        _(uriList).each(uri => {
                            resolver.remove(uri);
                        });
                    }
                    else {
                        log.error(`Unable to DELETE resources  '${uriList.join(',')}'`);
                    }
                    return uriList;
                })
                .catch(response => {
                    if (response.status === 415) {
                        log.info('Trying to delete again');
                        return link.delete(collection, /self/, 'application/json', uriList)
                            .then(response => {

                                if (response.status === 200 || response.status === 204) {
                                    log.info('Deleted');

                                    _(uriList).each(uri => {
                                        resolver.remove(uri);
                                    });
                                }
                                else {
                                    log.error(`Unable to DELETE resources  '${uriList.join(',')}'`);
                                }
                                return uriList;
                            });
                    }
                });

        };

        options = _({}).extend(
            options,
            {
                createStrategy: createUriListAndUpdateResolver,
                updateStrategy: updateUriListAndUpdateResolver,
                deleteStrategy: deleteUriListAndUpdateResolver
            }
        );

        const resourceUriList = uriListResolver(collection);

        return Differencer.diffUriList(resourceUriList, documentUriList, options);

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
    getUriListOnNamedCollection (parentResource, uriListName, uriListRel, uriList, options = {}) {

        log.info(`[Sync] uriList '${uriListName}' on ${SemanticLink.getUri(parentResource, /self/)}`);

        return nodMaker
            .getResource(parentResource)
            .then(resource => nodMaker
                .tryGetNamedCollectionResource(resource, uriListName, uriListRel, undefined, options)
                .then(collection => {
                    if (!collection) {
                        log.info(`No update: uri-list '${uriListName}' not found on ${SemanticLink.getUri(parentResource, /self/)}`);
                        return Promise.resolve(undefined);
                    } else {

                        return this.synchroniseUriList(collection, uriList, options)
                            .then(arrayToArguments(() => {
                                options = _({}).extend(options, {forceLoad: true});
                                return nodMaker.getCollectionResourceAndItems(collection, options);
                            }));
                    }
                }));
    }

}

export let nodSynchroniser = new NODSynchroniser();
