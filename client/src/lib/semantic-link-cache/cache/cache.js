import _ from '../mixins/index';
import {stateFlagEnum} from './stateFlagEnum';
import * as SparseResource from './SparseResource';
import {resourceMerger} from '../sync/ResourceMerger';
import * as link from 'semantic-link';
import {log} from 'logger';
import State from './State';
import {filter} from 'semantic-link/lib/filter';

/**
 *
 * This is just a utility that has a series of helpers that allows the client to layout the network of data
 * in a way that it wants allowoing the client-side application cache data to have self-consistency.
 *
 * Three types of apps
 * ====================
 *
 * Type One: retrieve each time
 * ----------------------------
 *
 * A first approach to have very little application caching and rely of the intermediary caches. The
 * implementation is to make requests buy using each resource itself and then binding the view to it.
 *
 * @example (humble) api browser using semantic link directly
 *
 * // retrieves and the view binds to the model
 * const apiUri = link.getUri('HEAD', 'api')
 * link.get(apiUri)
 *     .then(resource => model = resource);
 *
 * Type Two: retrieving from the application cache
 * -----------------------------------------------
 *
 * Another approach is to build up client-side cache from the root of the api. Each time a client view
 * needs a resource it goes through the cache to find it and then bind to it.
 *
 * @example
 *
 * const apiUri = link.getUri('HEAD', 'api');
 * const $api = cache.makeSparseResourceFromUri(apiUri);
 *
 * ...
 *
 * cache.getResource($api, 'self');  <-- goes out
 * cache.getResource($api, 'self');  <-- does not go out
 */

/**
 * @class CreateFormMergeStrategy
 * @param {LinkedRepresentation} resource
 * @param {FormRepresentation} createForm
 * @param {CreateCollectionResourceItemOptions} options
 * @return {Promise}
 */
const defaultCreateFormStrategy = (resource, createForm, options) =>
    resourceMerger.createMerge(resource, createForm, options);

/**
 * @class CreateCollectionResourceItemOptions
 * @extends {UtilOptions}
 * @property {CreateFormMergeStrategy} createFormCallback
 * @property createForm
 * @property {*} resolver
 */

const toWireRepresentation = resource => State.delete(resource);

/**
 * Returns the resource state. This shifts the context of the network of data the specified resource.
 * @param {LinkedRepresentation} resource
 * @return {State}
 */
const getResourceState = resource => State.get(resource);
/*

/**
 * A factory that creates a {@link State} for a resource in at a given state {@link stateFlagEnum} as
 * a named attribute of an object
 * @param state
 * @return {function():{Symbol, (state): State}}
 */
const defaultState = state => () => State.make(state);

/**
 *
 * @param {stateFlagEnum} state
 * @return {{stateFactory: *}} see {@link SparseResourceOptions}
 */
const makeSparseResourceOptions = (state) => {
    return {stateFactory: defaultState(state)};
};

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
    return State.makeItemToCollectionResource(collection, () => makeCollection(defaultValues));
}

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceUri
 * @param {*} defaultValues
 * @return {*|LinkedRepresentation}
 */
export function makeResourceFromUriAddedToCollection(collection, resourceUri, defaultValues) {
    return State.makeItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri, defaultValues));
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
    feedRepresentation.items.forEach(item => makeCollectionResourceItemByUri(collection, item.id, {name: item.title}));
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
    _(itemsUriList).each(uri => makeCollectionResourceItemByUri(collection, uri));
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
 * Add a resource into a collection in the tree where the value is known (including the URI)
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceUri
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation}
 * @obsolete
 */
export function makeCollectionResourceItemByUri(collection, resourceUri, defaultValues) {
    return State.makeItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri, defaultValues));
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
 * @return {Promise<LinkedRepresentation>}
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
 * Get a collection resource with the items list synchronised.
 *
 * Note: At worst, this is only one call back to the server because the request returns a feed representation and then
 * the items list are synchronised with items at worst in {@link stateFlagEnum.locationOnly}.
 * @example
 *
 *    Collection
 *
 *    +-----+
 *    | hydrated
 *    |     |
 *    +-----+
 *        X   items   +---+
 *        X  <------+ |locationOnly/mapped-title
 *        X           +---+
 *
 * @param {CollectionRepresentation} collection
 * @param {UtilOptions} options
 * @return {Promise<CollectionRepresentation>} collection is {@link stateFlagEnum.hydrated}, sparsely
 * populate the items as {@link LinkedRepresentation} to {@link stateFlagEnum.locationOnly} in the current
 * set (but not refresh the item set itself)
 */
export function getCollectionResource(collection, options) {
    return getResourceState(collection)
        .getCollectionResource(collection, options);
}

/**
 * Given a collection of local resources, get the resource identified by the uri
 * or add it if it doesn't exist and ensure that it is @link stateFlagEnum.hydrated} (its state is synchronised
 * with the server).
 *
 * This may result in a partially synchronised collection.
 *  * @example
 *
 *    Collection
 *
 *    +-----+
 *    |     |        resource
 *    |     |          uri
 *    +-----+
 *        X    item   +---+
 *        X  <------+ |   |
 *        X           +---+
 *
 * @param {CollectionRepresentation} collection
 * @param {string} itemUri the id of the resource
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation>} collection item synchronised with the server
 */
export function getCollectionResourceItemByUri(collection, itemUri, options) {
    return getResourceState(collection)
        .makeItemOnCollectionResource(collection, itemUri, options)
        .then(resource => getResource(resource, options));
}

/**
 * Given a collection of local resources, get the resource identified by the self/canonical
 * link relation and ensure that it is synchronised with the server.
 *
 * This may result in a partially synchronised collection.
 * @example
 *
 *    Collection
 *
 *    +-----+
 *    |     |        resource
 *    |     |
 *    +-----+
 *        X    item   +---+
 *        X  <------+ |   |
 *        X           +---+
 *
 * @param {CollectionRepresentation} collection
 * @param {LinkedRepresentation} resource
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation>} promise contains a {@link LinkedRepresentation}
 */
export function getCollectionResourceItem(collection, resource, options) {
    return getCollectionResourceItemByUri(collection, link.getUri(resource, /canonical|self/), options);
}

/**
 * Get a singleton resource on a parent resource added  as a named attribute on the parent resource.
 *
 * Note: this overrides conflicting attributes
 *
 * @example
 *
 *
 *     parent      singleton
 *     Resource    Resource
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |
 *     |          |
 *     +----------+
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} singletonName the attribute name on the parent resource
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation>} promise contains a {@link LinkedRepresentation}
 */
export function getSingletonResource(parentResource, singletonName, rel, options) {

    return getResource(parentResource, options)
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
 * Get a singleton resource on a parent resource added  as a named attribute on the parent resource.
 *
 * Note: this overrides conflicting attributes
 *
 * @example
 *
 *
 *     parent      singleton
 *     Resource    Resource
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |
 *     |          |
 *     +----------+
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} singletonName the attribute name on the parent resource
 * @param {string|RegExp} rel the link relation name
 * @param {LinkedRepresentation} defaultValue
 * @param {UtilOptions=} options
 * @return {Promise} promise contains a {@link LinkedRepresentation}, which could be a {@link FeedRepresentation}
 */
export function tryGetSingletonResource(parentResource, singletonName, rel, defaultValue, options = {}) {
    // TODO: allow for explicit default value in interface. Underlying library has changed implementation to return default
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    if (!link.getUri(parentResource, rel, undefined)) {
        log.debug(`Missing uri for rel '${rel}' - resolving with default value`);
        return Promise.resolve(defaultValue);
    }

    if (parentResource[singletonName]) {
        return tryGetResource(parentResource[singletonName], defaultValue, options);
    } else {
        return getResourceState(parentResource)
        // add a sparsely populated resource as a named attribute and return it
            .makeSingletonResource(parentResource, singletonName, rel, options)
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
 * Get a collection resource added as a named attribute on the parent resource.
 *
 * (see {@link getCollectionResource} for hydration behaviour of the collection)
 *  @example
 *
 *      parent     resource
 *      Resource   Collection
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName the name of a {@link FeedRepresentation}
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions} options
 * @return {Promise<CollectionRepresentation>} promise contains a {@link CollectionRepresentation}
 *
 */
export function getNamedCollectionResource(parentResource, collectionName, collectionRel, options) {
    if (parentResource[collectionName]) {
        return getCollectionResource(parentResource[collectionName], options);
    } else {
        return getResource(parentResource)
            .then((resource) => getResourceState(resource)
                .makeCollectionResource(resource, collectionName, collectionRel, options))
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
 * Get a collection resource added as a named attribute on the parent resource.
 *
 * (see {@link getCollectionResource} for hydration behaviour of the collection)
 *  @example
 *
 *      parent     resource
 *      Resource   Collection
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName the name of a {@link FeedRepresentation}
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions} options
 * @return {Promise<CollectionRepresentation|undefined>} promise contains a {@link CollectionRepresentation}
 */
export function tryGetNamedCollectionResource(parentResource, collectionName, collectionRel, options = {}) {
    // TODO: allow for explicit default value in interface. Underlying library has changed implementation to return default
    options = {...options, getUri: link.getUri};
    return getNamedCollectionResource(parentResource, collectionName, collectionRel, options);
}

/**
 *
 * Get a collection resource on each of the resources added as a named attribute on the parent singleton resource.
 *
 * (see {@link getCollectionResource} for hydration behaviour of the collection)
 *  @example
 *
 *      singleton
 *      parent      resource
 *      Resources   Collection (per singleton)
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *
 * @param {LinkedRepresentation[]} singletons
 * @param {string} collectionName
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation[]>} promise contains original singletons
 */
export function getNamedCollectionResourceOnSingletons(singletons, collectionName, collectionRel, options) {
    return _(singletons)
        .mapWaitAll(singleton =>
            getResource(singleton)
                .then(resource => tryGetNamedCollectionResource(resource, collectionName, collectionRel, options)))
        .catch(err => {
            log.info('Singleton error:', err);
            return Promise.resolve(singletons);
        })
        .then(() => singletons);
}

/**
 *
 * Get a collection resource on each of the resources added as a named attribute on the parent singleton resource.

 *
 * (see {@link getCollectionResource} for hydration behaviour of the collection)
 *  @example
 *
 *      singleton
 *      parent      resource
 *      Resources   Collection (per singleton)
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X
 *     |          |   X items
 *     +----------+   X
 *
 * @param {LinkedRepresentation[]} singletons
 * @param {string} collectionName
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation[]>} promise contains original singletons
 */
export function tryGetNamedCollectionResourceOnSingletons(singletons, collectionName, collectionRel, options = {}) {
    // TODO: allow for explicit default value in interface. Underlying library has changed implementation to return default
    options = {...options, getUri: link.getUri};

    return _(singletons)
        .mapWaitAll(singleton =>
            getResource(singleton)
                .then(resource => tryGetNamedCollectionResource(resource, collectionName, collectionRel, options)))
        .catch(err => {
            log.info('Singleton error:', err);
            return Promise.resolve(singletons);
        })
        .then(() => singletons);
}

/**
 * Get a collection resource and all its items. This is a pre-emptive load of all item resources.
 *
 * @example
 *
 *    collection     item
 *    Resource       Resource
 *
 *    +-----+
 *    |     |
 *    |     |
 *    +-----+
 *        X   items   +---+
 *        X  <------+ |hydrated
 *        X           +---+
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
 * Get a collection resource and all its items. This is a pre-emptive load of all item resources.
 *
 * @example
 *
 *    collection     item
 *    Resource       Resource
 *
 *    +-----+
 *    |     |
 *    |     |
 *    +-----+
 *        X   items   +---+
 *        X  <------+ |hydrated
 *        X           +---+
 *
 * @param {LinkedRepresentation} collection
 * @param {UtilOptions=} options
 * @return {Promise<CollectionRepresentation>}
 */
export function getCollectionResourceAndItems(collection, options) {

    return getCollectionResource(collection, options)
        .then(collection => tryGetCollectionResourceItems(collection, options));
}

/**
 * Get a collection resource and all its items added  as a named attribute on the parent resource. This
 * is a pre-emptive load of all item resources.
 *
 * @example
 *
 *      parent     collection    item
 *      Resource   Resource      Resource
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X items    +---+
 *     |          |   X <------+ |hydrated
 *     +----------+   X          +---+
 *
 * @param {LinkedRepresentation} parentResource
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function getNamedCollectionResourceAndItems(parentResource, collectionName, collectionRel, options) {

    return getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
        .then(collection => tryGetCollectionResourceItems(collection, options));
}

/**
 * Get a collection resource and all its items. This is a pre-emptive load of all item resources.
 *
 * @example
 *
 * Get a collection resource and all its items added  as a named attribute on the parent resource. This
 * is a pre-emptive load of all item resources.
 *
 * @example
 *
 *      parent     collection    item
 *      Resource   Resource      Resource
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X items    +---+
 *     |          |   X <------+ |hydrated
 *     +----------+   X          +---+
 *
 *
 * @param {LinkedRepresentation} parentResource the parent to the collection
 * @param {string} collectionName the name of the collection in the parent
 * @param {string|RegExp} collectionRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise<CollectionRepresentation|undefined>}
 */
export function tryGetCollectionResourceAndItems(parentResource, collectionName, collectionRel, options = {}) {
    // TODO: allow for explicit default value in interface. Underlying library has changed implementation to return default
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    return getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
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
 * Get a collection resource (based also on its 'title') and all its items added  as a named attribute on
 * the parent resource. This is a pre-emptive load of all item resources.
 *
 * This is used where there are multiple link relations of the same name and are different by 'title'
 *
 * Note: this is for more complex api designs using multiple link relations on a resource to representation collections.
 *
 * @example
 *
 *
 *      parent      resource
 *      Resource   Collection (where link rel and title)
 *
 *     +----------+
 *     |          |
 *     |          +-----+
 *     |     Named|     |
 *     |          |     |
 *     |          +-----+
 *     |          |   X items    +---+
 *     |          |   X <------+ |hydrated
 *     +----------+   X          +---+
 *
 * @example Match on rel --> todos, title --> 49004.rewire.example.nz
 *
 *    {
 *      "links": [
 *        {
 *          "rel": "self",
 *          "href": "http://localhost:5000/user/f58c6dd2a5"
 *        },
 *        {
 *          "rel": "todos",
 *          "href": "http://localhost:5000/user/tenant/5388881ce2/todo",
 *          "title": "586931.rewire.example.nz"
 *        },
 *        {
 *          "rel": "todos",
 *          "href": "http://localhost:5000/user/tenant/db384a9924/todo",
 *          "title": "49004.rewire.example.nz"
 *        },
 *     } ...
 *
 * @param {LinkedRepresentation} parentResource the parent to the collection
 * @param {string} collectionName the name of the collection in the parent container
 * @param {string|RegExp} collectionRel the link relation name
 * @param {string} title the title of the resource in the collection to be hydrated
 * @param {UtilOptions=} options
 * @return {Promise} with the item by uri from the collection resource of type {@link FeedRepresentation}
 */
export function getNamedCollectionByTitle(parentResource, collectionName, collectionRel, title, options) {

    /**
     * Override the semantic link getUri implementation that returns the first found href. However
     * because these options cascade through, ensure that the getUri is only used 'once' and then discarded
     */
    return getNamedCollectionResource(
        parentResource,
        collectionName,
        collectionRel,
        {
            ...options,
            getUri: _.once((links, relationshipType, mediaType) => {
                const [first,] = filter(links, relationshipType, mediaType).filter(link => link.title === title);
                if (first) {
                    log.debug(`[Cache] getUri override on rel '${collectionRel}' found title '${title}' using '${first.href}'`);
                    return first.href;
                } else {
                    log.debug(`No match on link rel '${collectionRel}' for title '${title}'`);
                }
            })
        })
        .then(collection => tryGetCollectionResourceItems(collection, options));

}

/**
 * Get an item resource (based on its 'self' uri link relation) from a named collection resource on a parent resource
 *
 * @example
 *
 *
 *      parent     collection    item
 *      Resource   Resource      Resource
 *
 *     +----------+
 *     |          |
 *     |          +-----+        (where link rel self == uri)
 *     |     Named|     |          +
 *     |          |     |          |
 *     |          +-----+          v
 *     |          |   X item     +---+
 *     |          |   X <------+ |hydrated
 *     +----------+   X          +---+
 *
 * @param {LinkedRepresentation} parentResource the parent resource to the collection
 * @param {string} collectionName the name of the collection in the parent
 * @param {string|RegExp} collectionRel the link relation name
 * @param {string} itemUri the uri of the item resource in the collection to be hydrated
 * @param {UtilOptions=} options
 * @return {Promise<LinkedRepresentation>} with the item by uri from the collection item resource
 */
export function getItemInNamedCollectionByUri(parentResource, collectionName, collectionRel, itemUri, options) {

    return getNamedCollectionResource(parentResource, collectionName, collectionRel, options)
        .then((collection) => {

            if (!collection) {
                throw new Error(`A collection should have been created ${link.getUri(parentResource, /self|canonical/)} with ${collectionName}`);
            }

            let itemResource = _(collection).findResourceInCollectionByRelOrAttribute(itemUri);

            if (!itemResource) {
                itemResource = makeResourceFromUriAddedToCollection(collection, itemUri);
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
 * Get each item resource in the named child collection resource on the parent collection items. This is a
 * pre-emptive load of the child collection items.
 *
 * @example
 *
 *   parent                      child
 *   collection      item        collection   item
 *   Resource        Resource    Resource     Resource
 *
 *   +-----+
 *   |     |
 *   |     |
 *   +-----+         +----------+
 *       X   each    |          |
 *       X   item    |          +-----+
 *       X <------+  |     Named|     |
 *                   |          |     |
 *                   |          +-----+
 *                   |          |   X   items  +---+
 *                   |          |   X <------+ |hydrated
 *                   +----------+   X          +---+
 *
 * @param {CollectionRepresentation} parentCollection
 * @param {string} childCollectionName the name of the collection on each item resource
 * @param {string|RegExp} childCollectionRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise<CollectionRepresentation>} original collection
 */
export function tryGetNamedCollectionResourceAndItemsOnCollectionItems(parentCollection, childCollectionName, childCollectionRel, options = {}) {
    // TODO: allow for explicit default value in interface. Underlying library has changed implementation to return default
    options = _({}).extend(options, {
        getUri: link.getUri
    });

    // TODO ?? should this return the child collection items ??
    return _(parentCollection)
        .mapWaitAll(item => getNamedCollectionResource(item, childCollectionName, childCollectionRel, options)
            .then(childCollection => tryGetCollectionResourceItems(childCollection, options))
        )
        .then(collection => collection);
}

/**
 * Get each item resource in the named child collection resource on the parent collection items. This is a
 * lazy load of child collection items.
 *
 * @example
 *
 *   parent                      child
 *   collection      item        collection   item
 *   Resource        Resource    Resource     Resource
 *
 *   +-----+
 *   |     |
 *   |     |
 *   +-----+         +----------+
 *       X   each    |          |
 *       X   item    |          +-----+
 *       X <------+  |     Named|     |
 *                   |          |     |
 *                   |          +-----+
 *                   |          |   X   items  +---+
 *                   |          |   X <------+ |locationOnly
 *                   +----------+   X          +---+
 *
 * @param {CollectionRepresentation} parentCollectionResource the parent to the collection
 * @param {string} childCollectionName the name of the collection in the parent collection items
 * @param {string|RegExp} rel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the collection resource of type {@link FeedRepresentation} (i.e. the collection resource object)
 */
export function tryGetNamedCollectionResourceOnCollectionItems(parentCollectionResource, childCollectionName, rel, options) {

    return _(parentCollectionResource)
        .mapWaitAll(item => tryGetNamedCollectionResource(item, childCollectionName, rel, options));
}

/**
 * Get singleton resource on a parent collection added as a named attribute on each item resource
 *
 * @example
 *
 *   parent                      child
 *   collection      item        singleton
 *   Resource        Resource    Resource
 *
 *   +-----+
 *   |     |
 *   |     |
 *   +-----+         +----------+
 *       X   each    |          |
 *       X   item    |          +-----+
 *       X <------+  |     Named|     |
 *                   |          |     |
 *                   |          +-----+
 *                   |          |
 *                   |          |
 *                   +----------+
 * @param {CollectionRepresentation} parentCollectionResource the parent to the collection
 * @param {string} childSingletonName the name of the collection in the parent collection items
 * @param {string|RegExp} childSingletonRel the link relation name
 * @param {UtilOptions=} options
 * @return {Promise} with the array of singleton resources that succeed
 */
export function tryGetNamedSingletonResourceOnCollectionItems(parentCollectionResource, childSingletonName, childSingletonRel, options) {
    return _(parentCollectionResource)
        .mapWaitAll(item => tryGetSingletonResource(item, childSingletonName, childSingletonRel, undefined, options))
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
            return State.makeItemToCollectionResource(collection, () => resource);
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
 * A replacer function to strip the state from a model
 * see https://developer.mozilla.org/en/docs/Web/JavaScript/Reference/Global_Objects/JSON/stringify
 *
 * @param key
 * @param value
 * @return {*|undefined} undefined to keep the key
 */
const ToJsonReplacer = (key, value) => key !== 'createForm' && key !== 'editForm' ? value : undefined;

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


