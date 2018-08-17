import _ from '../mixins/index';
import {stateFlagEnum} from './stateFlagEnum';
import SparseResource from './SparseResource';
import {resourceMerger} from '../sync/ResourceMerger';
import * as link from 'semantic-link';
import {log} from 'logger';
import State from './State';
import {filter} from 'semantic-link/lib/filter';

/**
 *
 * This is just a utility that has a series of helpers that allows the client to layout the network of data
 * in a way that it wants. It allows data to have self-consistency.
 *
 * At some level this is a helper with object-as-a-network-of-data representation mapper (ORM!)
 *
 */

/**
 * @class CreateFormMergeStrategy
 * @param {LinkedRepresentation} resource
 * @param {FormRepresentation} createForm
 * @param {CreateCollectionResourceItemOptions} options
 * @return {Promise}
 */

export function defaultCreateFormStrategy(resource, createForm, options) {
    return resourceMerger.createMerge(resource, createForm, options);
}

/**
 * A replacer function to strip the state from a model
 * see https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify
 *
 * This function currently removes the stateFlag and angular $$hashKey attributes
 * @param key
 * @param value
 */
export function ToJsonReplacer(key, value) {
    return key !== '$$hashKey' && key !== 'createForm' && key !== 'editForm' ? value : undefined;
}

/**
 * @class CreateCollectionResourceItemOptions
 * @extends {UtilOptions}
 * @property {CreateFormMergeStrategy} createFormCallback
 * @property createForm
 * @property {*} resolver
 */

export function toWireRepresentation(resource) {
    return State.delete(resource);
}

/**
 * Returns the resource state. This shifts the context of the network of data the specified resource.
 * @param {LinkedRepresentation} resource
 * @return {State}
 */
export function getResourceState(resource) {
    return State.get(resource);
}

/**
 *
 * @param {LinkedRepresentation} resource
 * @param {*} defaultValue
 * @return {State}
 */
export function tryGetResourceState(resource, defaultValue) {
    return State.tryGet(resource, defaultValue);
}

export function createResourceOnCollection(collection, collectionAttribute, rel, document, options = {}) {
    return getNamedCollectionResource(collection, collectionAttribute, rel, options)
        .then(childCollection => createCollectionResourceItem(childCollection, document, options));
}

/**
 * A factory that creates a {@link State} for a resource in at a given state {@link stateFlagEnum} as
 * a named attribute of an object
 * @param state
 * @return {function():{Symbol, (state): State}}
 */
export function defaultState(state) {
    return () => State.make(state);
}

/**
 *
 * @param {stateFlagEnum} state
 * @return {SparseResourceOptions}
 */
export function makeSparseResourceOptions(state) {
    return {
        stateFactory: defaultState(state)
    };
}

/**
 * Make a new, sparsely populated {@link LinkedRepresentation} with {@link State}.
 * @param {*=} defaultValues
 * @param {stateFlagEnum=} state
 * @return {LinkedRepresentation}
 */
export function makeLinkedRepresentation(defaultValues, state) {
    return SparseResource.makeLinkedRepresentation(makeSparseResourceOptions(state || stateFlagEnum.unknown), defaultValues);
}

/**
 * Make a virtual representation for process
 * @param {*=} defaultValues
 * @return {LinkedRepresentation}
 */
export function makeVirtualRepresentation(defaultValues) {
    return SparseResource.makeLinkedRepresentation(makeSparseResourceOptions(stateFlagEnum.virtual), defaultValues);
}

// /**
//  *
//  * @param {LinkedRepresentation} resource
//  * @param {CollectionRepresentation} collection
//  * @return {*|LinkedRepresentation}
//  */
// function addItemToCollectionResource(resource, collection) {
//     return getResourceState(collection)
//         .addItemToCollectionResource(collection, ()=>resource);
// }

/**
 * Make a new, sparsely populated {@link CollectionRepresentation} with {@link State}.
 *
 * This means that there is at least an empty `items` attribute.
 *
 * @param {*=} defaultValues
 * @param {stateFlagEnum=} state=stateFlagEnum.unknown
 * @return {CollectionRepresentation}
 */
export function makeCollection(defaultValues, state) {
    return SparseResource.makeCollection(makeSparseResourceOptions(state || stateFlagEnum.unknown), defaultValues);
}

/**
 * Make a new, sparsely populated {@link LinkedRepresentation} with {@link State} and
 * link relation 'self' populated with from given uri
 *
 * @param uri
 * @param defaultValues
 * @param {stateFlagEnum=} state
 * @return {LinkedRepresentation}
 */
export function makeSparseResourceFromUri(uri, defaultValues, state) {
    if (!uri) {
        state = stateFlagEnum.virtual;
    }
    return SparseResource.makeFromUri(uri, makeSparseResourceOptions(state || stateFlagEnum.locationOnly), defaultValues);
}

/**
 * Make a new, sparsely populated {@link CollectionRepresentation} with {@link State} and
 * link relation 'self' populated with from given uri
 *
 * @param {string} uri
 * @param {*=} defaultValues
 * @param {stateFlagEnum=} state
 * @return {CollectionRepresentation}
 */
export function makeSparseCollectionResourceFromUri(uri, defaultValues, state) {
    if (!uri) {
        state = stateFlagEnum.virtual;
    }
    return SparseResource.makeCollectionFromUri(uri, makeSparseResourceOptions(state || stateFlagEnum.locationOnly), defaultValues);
}

/**
 * Add a resource into the tree where the value is unknown (including the URI)
 *
 * @param {LinkedRepresentation} resource the parent resource that will act as the container
 *   for the named child resource.
 * @param {string} resourceName the name of the child resource in the container
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation} resource existing as a child
 */
export function makeUnknownResourceAddedToResource(resource, resourceName, defaultValues) {
    return getResourceState(resource)
        .addResourceByName(resource, resourceName, () => makeLinkedRepresentation(defaultValues));
}

/**
 * Add a collection resource into the tree where the value is unknown (including the URI)
 *
 * @param {LinkedRepresentation} resource a parent resource container
 * @param {string} collectionResourceName
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {CollectionRepresentation}
 */
export function makeUnknownCollectionAddedToResource(resource, collectionResourceName, defaultValues) {
    return getResourceState(resource)
        .addCollectionResourceByName(resource, collectionResourceName, () => makeCollection(defaultValues));
}

/**
 * Add a resource into a collection in the tree where the value is unknown (including the URI)
 *
 * @param {CollectionRepresentation} collection
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation}
 */
export function makeUnknownResourceAddedToCollection(collection, defaultValues) {
    return State.addItemToCollectionResource(collection, () => makeCollection(defaultValues));
}

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceUri
 * @param {*} defaultValues
 * @return {*|LinkedRepresentation}
 */
export function makeResourceFromUriAddedToCollection(collection, resourceUri, defaultValues) {
    return State.addItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri, defaultValues));
}

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string[]} uriList
 * @return {CollectionRepresentation}
 */
export function makeCollectionItemsFromUriListAddedToCollection(collection, uriList) {
    const resourceState = getResourceState(collection);
    uriList.forEach(resourceUri =>
        resourceState.addItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri)));
    return collection;
}

/**
 * Takes a feed representation and converts to a sparse collection representation
 *
 * @param {CollectionRepresentation} collection
 * @param {FeedRepresentation} feedRepresentation
 * @return {CollectionRepresentation}
 */
export function makeCollectionItemsFromFeedAddedToCollection(collection, feedRepresentation) {
    feedRepresentation.items.forEach(item => addCollectionResourceItemByUri(collection, item.id, {name: item.title}));
    return collection;
}

/**
 * Adds a, or uses an existing, named collection on resource and then adds items into the collection based on a uri list
 *
 * @param {LinkedRepresentation} resource
 * @param {string} collectionResourceName
 * @param {string} collectionUri
 * @param {string[]} itemsUriList
 * @param {stateFlagEnum} state
 * @return {LinkedRepresentation}
 */
export function makeNamedCollectionFromUriAndResourceFromUriList(resource, collectionResourceName, collectionUri, itemsUriList, state) {
    let collection = getResourceState(resource)
        .addCollectionResourceByName(
            resource,
            collectionResourceName,
            () => {
                if (!collectionUri) {
                    state = stateFlagEnum.virtual;
                }
                return SparseResource.makeCollectionFromUri(collectionUri, makeSparseResourceOptions(state || stateFlagEnum.locationOnly));
            });

    // only add items not currently loaded
    _(itemsUriList).each(uri => addCollectionResourceItemByUri(collection, uri));
    return collection;
}

/**
 * Add a singleton list (array) of sparse LinkedRepresentations based on an attribute that has a uriList.
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string} itemsUriListName
 * @param {stateFlagEnum=} state
 * @return {Promise} contains an array of sparsely populated resources
 */
export function makeSingletonSparseListFromAttributeUriList(resource, singletonName, itemsUriListName, state) {

    return getResource(resource)
        .then(resource => {

            if (!resource[singletonName]) {
                makeUnknownCollectionAddedToResource(resource, singletonName);
            }

            _(resource[itemsUriListName]).map(uri => {
                if (!resource[singletonName].items.find(item => link.getUri(item, /canonical|self/) === uri)) {
                    resource[singletonName].items.splice(resource[singletonName].length, 0, makeSparseResourceFromUri(uri, state));
                }
            });

            return resource[singletonName];
        });

}

/**
 * Add a singleton sparse LinkedRepresentations based on an uri.
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string} uri
 * @return {Promise} contains an array of populated resources
 */
export function makeSingletonSparseFromUri(resource, singletonName, uri) {
    return getResource(resource)
        .then(resource => {

            if (!resource[singletonName]) {
                resource[singletonName] = makeSparseResourceFromUri(uri);
            }

            return resource[singletonName];
        });
}

/**
 * Add a singleton list (array) of hydrated LinkedRepresentations based on an attribute that has a uriList.
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string} itemsUriListName
 * @param {stateFlagEnum=} state
 * @return {Promise} contains an array of populated resources
 */
export function makeSingletonListFromAttributeUriList(resource, singletonName, itemsUriListName, state) {
    return makeSingletonSparseListFromAttributeUriList(resource, singletonName, itemsUriListName, state)
        .then(collection => {
            return _(collection).mapWaitAll(item => {
                return getResource(item);
            });
        });
}

/**
 * Given the parent{@link LinkedRepresentation} get the best place in that data structure
 * for a named {@link CollectionRepresentation} given the uri of the collection resource item.
 * Basically, this adds a named collection on a parent and then hydrates that particular item in the
 * collection.
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionResourceName
 * @param collectionRel
 * @param {*} itemUri the uri of the item in the collection to find
 * @param {UtilOptions} options
 * @return {*|Promise} containing the item in the collection
 */
export function getResourceFromUriAddedToNamedCollection(parentResource, collectionResourceName, collectionRel, itemUri, options = {}) {

    let collection = parentResource[collectionResourceName];

    if (!collection) {
        collection = makeSparseCollectionResourceFromUri(link.getUri(parentResource, collectionRel));
        parentResource[collectionResourceName] = collection;
    }

    let item = _(collection.items).find(item => itemUri === link.getUri(item, /self|canonical/));
    if (_(item).isEmpty()) {
        item = addCollectionResourceItemByUri(collection, itemUri);
    }
    return getCollectionResource(item, options);
}

/**
 * Add a resource into a collection in the tree where the value is known (including the URI)
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceUri
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation}
 * @obsolete
 */
export function addCollectionResourceItemByUri(collection, resourceUri, defaultValues) {
    return State.addItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri, defaultValues));
}

/**
 * Get a resource. The resource **must** be {@link LinkedRepresentation} with
 * at least the link relation `self` or 'canonical' in place.
 *
 * @example:
 *
 *   {
 *       links: [
 *           {
 *               rel: 'self',
 *               href: 'https://example.com/resource'
 *           }
 *       ]
 *   }
 *
 * @param {LinkedRepresentation} resource
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}
 */
export function getResource(resource, options = {}) {
    return getResourceState(resource)
        .getResource(resource, options);
}

/**
 * Get a resource. The resource **must** be {@link LinkedRepresentation} with
 * at least the link relation `self` or 'canonical' in place otherwise it will return the defaultValue
 * @param {LinkedRepresentation} resource
 * @param defaultValue
 * @param {UtilOptions} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}
 * @return {*}
 */
export function tryGetResource(resource, defaultValue = undefined, options = {}) {
    const tryResource = State.tryGet(resource, defaultValue);

    if (tryResource === defaultValue) {
        log.debug(`Using default value on ${link.getUri(resource, /self/)}`);
        return Promise.resolve(defaultValue);
    } else {
        return tryResource
            .getResource(resource, options);

    }
}

/**
 *
 * @param {CollectionRepresentation} resource
 * @param {UtilOptions} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}
 */
export function getCollectionResource(resource, options) {
    return getResourceState(resource)
        .getCollectionResource(resource, options);
}

/**
 * Given a collection of local resources, get the resource identified by the uri
 * or add it if it doesn't exist and ensure that it is hydrated (its state is synchronised
 * with the server).
 *
 * This may result in a partially synchronised collection.
 *
 * @param {CollectionRepresentation} collection
 * @param {string} itemUri the id of the resource
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}
 */
export function getCollectionResourceItemByUri(collection, itemUri, options) {
    return getResourceState(collection)
        .makeItemOnCollectionResource(collection, itemUri, options)
        .then(resource => getResource(resource, options));
}

/**
 * Given a collection of local resources, get the resource identified by the uri
 * and ensure that it is hydrated (its state is synchronised with the server).
 *
 * This may result in a partially synchronised collection.
 *
 * @param {CollectionRepresentation} collection
 * @param {LinkedRepresentation} resource
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}
 */
export function getCollectionResourceItem(collection, resource, options) {
    return getCollectionResourceItemByUri(collection, link.getUri(resource, /canonical|self/), options);
}

/**
 * Given a singleton item (i.e. a child resource attribute) get its value.
 *
 * This make the assumption that we have the parent of the server resource and
 * the relationship to the child (i.e. the subject of what we are trying to get).
 *
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}, which could be a {@link FeedRepresentation}
 */
export function getSingletonResource(resource, singletonName, rel, options) {

    return getResource(resource, options)
        .then(resource => {
            if (resource[singletonName]) {
                return getResource(resource[singletonName], options);
            } else {
                return getResourceState(resource)
                // add a sparsely populated resource as a named attribute and return it
                    .makeSingletonResource(resource, singletonName, rel, options)
                    .then(resource => getResource(resource, options));
            }
        });
}

/**
 * Given a singleton item (i.e. a child resource attribute) get its value.
 *
 * This make the assumption that we have the parent of the server resource and
 * the relationship to the child (i.e. the subject of what we are trying to get).
 *
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string|RegExp} rel the link relation name
 * @param {LinkedRepresentation} defaultValue
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}, which could be a {@link FeedRepresentation}
 */
export function tryGetSingletonResource(resource, singletonName, rel, defaultValue, options = {}) {
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    if (!link.getUri(resource, rel, undefined)) {
        log.debug(`Missing uri for rel '${rel}' - resolving with default value`);
        return Promise.resolve(defaultValue);
    }

    if (resource[singletonName]) {
        return tryGetResource(resource[singletonName], defaultValue, options);
    } else {
        return getResourceState(resource)
        // add a sparsely populated resource as a named attribute and return it
            .makeSingletonResource(resource, singletonName, rel, options)
            .then(resource => {
                //
                return getResource(resource, options);
            })
            .catch(() => {
                return log.error(`Unexpected error making singleton '${singletonName}'`);
            });
    }
}

/**
 * Get a resource and ensure the collection is created, sparsely populate the items
 * in the current set (but not refresh the item set itself).
 *
 * The result:
 *
 *   status     resource in network of data
 *   --------------------------------------
 *   hydrated - resource
 *   hydrated - resource.collection
 *   feedOnly - resource.collection.items
 *
 * @param {LinkedRepresentation} resource
 * @param {string} collectionAttribute the name of a {@link FeedRepresentation}
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions} options
 * @return {Promise} promise contains a {@link CollectionRepresentation}
 *
 */
export function getNamedCollectionResource(resource, collectionAttribute, rel, options) {
    if (resource[collectionAttribute]) {
        return getCollectionResource(resource[collectionAttribute], options);
    } else {
        return getResource(resource)
            .then((resource) => getResourceState(resource)
                .makeCollectionResource(resource, collectionAttribute, rel, options))
            .then((collection) => {
                if (collection) {
                    return getCollectionResource(collection, options);
                } else {
                    return collection;
                }
            });
    }
}

/**
 * Get a resource with a collection, sparsely populate the items in the current set
 * (but not refresh the item set itself).
 *
 * @param {LinkedRepresentation} resource
 * @param {string} collectionAttribute the name of a {@link FeedRepresentation}
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link CollectionRepresentation}
 */
export function tryGetNamedCollectionResource(resource, collectionAttribute, rel, options = {}) {
    options = {...options, getUri: link.getUri};
    return getNamedCollectionResource(resource, collectionAttribute, rel, options);
}

/**
 * Takes a set of resources and add a named collection on each - the resulting set are sparsely populated.
 * @param {LinkedRepresentation[]} singletons
 * @param {string} collectionName
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} promise contains original {@link LinkedRepresentation[]}
 */
export function getNamedCollectionResourceOnSingletons(singletons, collectionName, rel, options) {
    return _(singletons)
        .mapWaitAll(singleton =>
            getResource(singleton)
                .then(resource => tryGetNamedCollectionResource(resource, collectionName, rel, options)))
        .catch(err => {
            log.info('Singleton error:', err);
            return Promise.resolve(singletons);
        })
        .then(() => singletons);
}

/**
 * Takes a set of resources and add a named collection on each - the resulting set are sparsely populated.
 * @param {LinkedRepresentation[]} singletons
 * @param {string} collectionName
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} promise contains original {@link LinkedRepresentation[]}
 */
export function tryGetNamedCollectionResourceOnSingletons(singletons, collectionName, rel, options = {}) {
    options = {...options, getUri: link.getUri};

    return _(singletons)
        .mapWaitAll(singleton =>
            getResource(singleton)
                .then(resource => tryGetNamedCollectionResource(resource, collectionName, rel, options)))
        .catch(err => {
            log.info('Singleton error:', err);
            return Promise.resolve(singletons);
        })
        .then(() => singletons);
}

/**
 * Ensures that a collection resource items all to {@link stateFlagEnum.hydrated} state - this is a
 * pre-emptive load of all child resources. If a child does not exist it is removed from the collection.
 *
 * @param {CollectionRepresentation} collection
 * @param {UtilOptions} options (with a cancellable)
 * @return {Promise} with the collection resource
 */
export function tryGetCollectionResourceItems(collection, options = {}) {

    return _(collection)
        .mapWaitAll((item) => {
            return getResource(item, options)
                .catch(() =>
                    Promise.resolve(getResourceState(collection).removeItemFromCollectionResource(collection, item)));
        })
        .then(() => {
            return collection;
        });
}

/**
 * Ensures that a collection resource is synchronised with its items all to {@link stateFlagEnum.hydrated} state - this is a
 * pre-emptive load of all child resources.
 *
 * @param {LinkedRepresentation} resource
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function getCollectionResourceAndItems(resource, options) {

    return getCollectionResource(resource, options)
        .then(collection => tryGetCollectionResourceItems(collection, options));
}

/**
 * Ensures that a collection resource from a link relation is synchronised as a name
 * attribute with its items all to {@link stateFlagEnum.hydrated} state - this is a
 * pre-emptive load of all child resources.
 *
 * @param {LinkedRepresentation} resource
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function getNamedCollectionResourceAndItems(resource, collectionName, rel, options) {

    return getNamedCollectionResource(resource, collectionName, rel, options)
        .then(collection => tryGetCollectionResourceItems(collection, options));
}

/**
 * Ensures that a collection resource from a link relation based on its uri and is synchronised as a name
 * attribute with an items made to {@link stateFlagEnum.feedOnly} state.
 *
 * This is used where there are multiple link relations of the same name.
 *
 * Note: this is for more complex api designs using multiple link relations on a resource to representat collections.
 *
 * @param {LinkedRepresentation} resource the parent to the collection
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} rel the link relation name
 * @param {string} title the title of the resource in the collection to be hydrated
 * @param {UtilOptions=} options
 * @return {Promise} with the item by uri from the collection resource of type {@link FeedRepresentation}
 */
export function getNamedCollectionByTitle(resource, collectionName, rel, title, options) {

    /**
     * Override the semantic link getUri implementation that returns the first found href. However
     * because these options cascade through, ensure that the getUri is only used once and then discarded
     */
    return getNamedCollectionResource(
        resource,
        collectionName,
        rel,
        {
            ...options,
            getUri: _.once((links, relationshipType, mediaType) => {
                const [first,] = filter(links, relationshipType, mediaType).filter(link => link.title === title);
                if (first) {
                    log.debug(`[Cache] getUri override on rel '${rel}' found title '${title}' using '${first.href}'`);
                    return first.href;
                } else {
                    log.debug(`No match on link rel '${rel}' for title '${title}'`);
                }
            })
        })
        .then(collection => tryGetCollectionResourceItems(collection, options));

}

/**
 * Ensures that a collection resource from a link relation is synchronised as a name
 * attribute with an items made to {@link stateFlagEnum.feedOnly} state. It will then load the single
 * resource identified by its uri to {@link stateFlagEnum.hydrated} state.
 *
 * @param {LinkedRepresentation} resource the parent to the collection
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} rel the link relation name
 * @param {string} uri the uri of the resource in the collection to be hydrated
 * @param {UtilOptions=} options
 * @return {Promise} with the item by uri from the collection resource of type {@link FeedRepresentation}
 */
export function getItemInNamedCollectionByUri(resource, collectionName, rel, uri, options) {

    return getNamedCollectionResource(resource, collectionName, rel, options)
        .then((collection) => {

            if (!collection) {
                throw new Error(`A collection should have been created ${link.getUri(resource, /self|canonical/)} with ${collectionName}`);
            }

            let itemResource = _(collection).findResourceInCollectionByRelOrAttribute(uri);

            if (!itemResource) {
                itemResource = makeResourceFromUriAddedToCollection(collection, uri);
            }
            return getResource(itemResource, options);
        });
}

/**
 * @class UpdateCollectionResourceItemOptions
 * @extends {UtilOptions}
 * @extends {EditMergeOptions}
 * @property {EditFormMergeStrategy} editFormCallback
 * @property {*} resolver
 * @property putStrategy
 */

/**
 * Ensures that a collection resource from a link relation is synchronised as a name
 * attribute with its items all to {@link stateFlagEnum.hydrated} state - this is a
 * pre-emptive load of all child resources.
 *
 * @param {LinkedRepresentation} resource the parent to the collection
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function tryGetCollectionResourceAndItems(resource, collectionName, rel, options = {}) {
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    return getNamedCollectionResource(resource, collectionName, rel, options)
        .then(collection => {

            if (!collection) {
                return Promise.resolve(undefined);
            }
            return _(collection)
                .mapWaitAll(item => getResource(item, options))
                .then(() => collection);
        });
}

/**
 * Ensures the the named child collection of each of the collection items is populated.
 * @param collection
 * @param collectionName
 * @param rel
 * @param options
 */
export function tryGetNamedCollectionResourceAndItemsOnCollectionItems(collection, collectionName, rel, options = {}) {
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    return _(collection)
        .mapWaitAll(item => getNamedCollectionResource(item, collectionName, rel, options)
            .then(childCollection => tryGetCollectionResourceItems(childCollection, options))
        )
        .then(collection => collection);
}

/**
 * Ensures that a child collection of each collection items is sparsely populated
 * @param {CollectionRepresentation} collectionResource the parent to the collection
 * @param {string} childCollectionName the name of the collection in the parent collection items
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function tryGetNamedCollectionResourceOnCollectionItems(collectionResource, childCollectionName, rel, options) {

    return _(collectionResource)
        .mapWaitAll(item => tryGetNamedCollectionResource(item, childCollectionName, rel, options));
}

/**
 * Ensures that a child collection of each collection items is sparsely populated
 * @param {CollectionRepresentation} collectionResource the parent to the collection
 * @param {string} singletonName the name of the collection in the parent collection items
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the array of singleton resources that succeed
 */
export function tryGetNamedSingletonResourceOnCollectionItems(collectionResource, singletonName, rel, options) {
    return _(collectionResource)
        .mapWaitAll(item => tryGetSingletonResource(item, singletonName, rel, undefined, options))
        // now discard any in the tryGet that returned the default value 'undefined'
        .then(result => _(result).reject(item => !item));
}

/**
 * @class EditFormMergeStrategy
 * @param {LinkedRepresentation} resource
 * @param {LinkedRepresentation} documentResource
 * @param {FormRepresentation} editForm
 * @param {UpdateCollectionResourceItemOptions} options
 * @return {Promise}
 */
export function defaultEditFormStrategy(resource, documentResource, editForm, options = {}) {

    const isTracked = (resource, trackedName) => {
        const resourceState = State.tryGet(resource);

        if (resourceState) {
            return resourceState.isTracked(trackedName);
        } else {
            return false;
        }
    };

    options = _({}).extend(options, {isTracked});

    return resourceMerger.editMerge(resource, documentResource, editForm, options)
        .catch(() => {
            log.error('[Merge] unknown merge error');
        });
}

/**
 * Tries to update a resource. If the server refuses an update the local version will not be mutated.
 *
 * @param {LinkedRepresentation} resource must include the 'edit-form' link relation
 * @param documentResource
 * @param {UpdateCollectionResourceItemOptions} options
 * @return {Promise} with the updated resource
 */
export function updateResource(resource, documentResource, options = {}) {

    /* @type {EditMergeOptions} */
    options = _({}).extend(options, {
        undefinedWhenNoUpdateRequired: true
    });

    if (!documentResource) {
        log.warn(`No document provided to update for resource ${link.getUri(resource, /self/)}`);
        return Promise.resolve(resource);
    }
    const mergeStrategy = options.editForm || defaultEditFormStrategy;

    return tryGetSingletonResource(resource, 'editForm', /edit-form/, undefined, options)
        .then(editForm => {

            if (!editForm) {
                log.info(`Resource has no edit form ${link.getUri(resource, /self|canonical/)}`);
                // return Promise.resolve(resource);
                editForm = {items: []};
            }

            return mergeStrategy(resource, documentResource, editForm, options)
                .then(merged => {
                    if (merged) {
                        return getResourceState(resource)
                            .updateResource(resource, toWireRepresentation(merged), options);
                    } else {
                        log.info(`No update required ${link.getUri(resource, /canonical|self/)}`);
                        return Promise.resolve(resource);
                    }
                })
                .catch(err => {
                    log.error(`Merge error: edit-form on ${link.getUri(resource, /self|canonical/)}`, err);
                });
        })
        .catch(() => {
            // with a tryGet we should never get here (alas that is not always the case)
            log.error(`Unexpected error on 'edit-form': on ${link.getUri(resource, /self|canonical/)}`/*, err, resource*/);
        });
}

export function tryUpdateResource(resource, documentResource, editFormCallback, options = {}) {
    return updateResource(resource, documentResource, editFormCallback, options);
}

/**
 *
 * @param {CollectionRepresentation} collection the collection to add the new resource to
 * @param {createDocument} document The document provided to create form callback to make the create data
 * @param {CreateCollectionResourceItemOptions} options
 * @return {LinkedRepresentation} a sparsely populated resource representation
 */
export function createCollectionResourceItem(collection, document, options = {}) {
    const mergeStrategy = options.createForm || defaultCreateFormStrategy;

    return getCollectionResource(collection, options)
        .then(collectionOrCreateForm => {

            if (!options.contributeonly) {
                return getSingletonResource(collectionOrCreateForm, 'createForm', /create-form/, options)
                    .then(createFormResource => mergeStrategy(document, createFormResource, options))
                    .then(mergedResource => {
                        if (_(mergedResource).isEmpty()) {
                            log.warn(`Unexpected empty item '${link.getUri(document, /self|canonical/)}' in '${link.getUri(collection, /self|canonical/)}' on mapping '${options.mappedTitle}'`);
                            return Promise.resolve(collection);
                        }
                        return getResourceState(collection)
                            .createResource(collection, mergedResource, options);
                    });
            } else {

                // // data for uri returns a json array of Uris
                return getResourceState(collection)
                    .createResource(collection, [link.getUri(document, /canonical|self/)], options);
            }

        })
        .then(resource => {
            return State.addItemToCollectionResource(collection, () => resource);
        });
}

/**
 *
 * @param {LinkedRepresentation} resource
 * @param {UtilOptions} options
 * @return {Promise} contains the original resource {@link LinkedRepresentation}
 */
export function deleteResource(resource, options = {}) {

    return getResourceState(resource)
        .deleteResource(resource, options)
        .then(() => resource);
}

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {LinkedRepresentation} item
 * @param {UtilOptions} options
 * @return {Promise}
 */
export function deleteCollectionItem(collection, item, options = {}) {

    return getCollectionResource(collection, options)
        .then((collectionResource) => {
            const itemResource = _(collectionResource).findResourceInCollection(item);
            if (itemResource) {
                return deleteResource(itemResource, options);
            } else {
                const reason = `Item not found (${link.getUri(item, /self/)}in collection ${link.getUri(collection, /self/)}`;
                log.error(reason, options);
                return Promise.reject(reason);
            }
        })
        .then((resource) => {
            return getResourceState(collection)
                .removeItemFromCollectionResource(collection, resource);
        });
}

/**
 * Returns a representation from a model that is already hydrated and looks close as possible to what
 * it looked like when it came over the wire. In this case, it removes the state attribute.
 *
 * @param {LinkedRepresentation} obj
 * @param {[Number|String]=} space number of spaces in the pretty print JSON
 * @return {LinkedRepresentation} obj
 */
export function toJson(obj, space) {
    return JSON.stringify(obj, ToJsonReplacer, space);
}


