import _ from 'underscore';
import State from 'semantic-link-cache/cache/State';
import StateEnum from 'semantic-link-cache/cache/StateEnum';
import * as link from 'semantic-link/lib/index';

/**
 * @class SparseResourceOptions
 * @extends UtilOptions
 * @property {{Symbol:State}} stateFactory creates a state object to be added to a resource
 */

/**
 * A set of factory methods to create sparse resources
 */

/**
 * Default mapped title between feed items and sparse resources
 * @type {string}
 */
export const mappedTitle = 'name';


/**
 * Default state factory will return an empty object
 * @returns {function():{}}
 */
export const defaultStateFactory = () => {
    return {};
};

/**
 *
 * @param {StateEnum} state
 * @return {{stateFactory: *}} see {@link SparseResourceOptions}
 */
export const makeSparseResourceOptions = (state) => {
    return {stateFactory: () => State.make(state)};
};

/**
 *
 * @param {FeedItemRepresentation} feedItem
 * @param {string=} resourceTitleAttributeName
 * @return {{links: *[]}}
 */
export const makeFromFeedItem = (feedItem, resourceTitleAttributeName) => {

    let localResource = {
        links: [{
            rel: 'self',
            href: feedItem.id
        }]
    };
    localResource[resourceTitleAttributeName || mappedTitle] = feedItem.title;
    return localResource;
};

/**
 * Make a new, sparsely populated {@link LinkedRepresentation}.
 * @param {SparseResourceOptions} options
 * @param {?*} defaultValues
 * @return {LinkedRepresentation|CollectionRepresentation}
 */
export const makeLinkedRepresentation = (options, defaultValues) => {
    options = options || {};

    if (!options.stateFactory) {
        options.stateFactory = defaultStateFactory;
    }

    return Object.assign(options.stateFactory(), {links: []}, defaultValues);
};

/**
 * Make a new, sparsely populated {@link LinkedRepresentation} potentially with {@link State} and
 * link relation 'self' populated with from given uri. if href is undefined the resource should be in state virtual
 * @param {string} uri
 * @param {SparseResourceOptions} options
 * @param {?*} defaultValues
 * @return {LinkedRepresentation|CollectionRepresentation}
 */
export const makeFromUri = (uri, options, defaultValues) => {

    const localResource = {links: [{rel: 'self', href: uri}]};
    return makeLinkedRepresentation(options, Object.assign(localResource, defaultValues));
};

/**
 * Make a new, sparsely populated {@link CollectionRepresentation} potentially with {@link State}
 *
 * @param {SparseResourceOptions} options
 * @param {?*} defaultValues
 * @param {?[{rel:string,href:string,title:string}]}defaultItems
 * @return {CollectionRepresentation}
 */
export const makeCollection = (options, defaultValues, defaultItems = []) => {
    return makeLinkedRepresentation(options, Object.assign({items: defaultItems}, defaultValues));
};

/**
 * Make a new, sparsely populated {@link CollectionRepresentation} potentially with {@link State} and
 * link relation 'self' populated with from given uri
 *
 * @param {string} uri
 * @param {SparseResourceOptions} options
 * @param {?*} defaultValues
 * @return {CollectionRepresentation}
 */
export const makeCollectionFromUri = (uri, options, defaultValues) => {
    return makeFromUri(uri, options, Object.assign({items: []}, defaultValues));
};

/**
 * Given a feed item (an item from a list of 'things') create a local linked representation resource.
 * This is the first stage at hydrating (state synchronisation) a remote resource locally.
 *
 * @param {FeedItemRepresentation} feedItem The item from the list
 * @param {string=} resourceTitleAttributeName=name an option name for the title from the feed (e.g. 'name')
 * @param {SparseResourceOptions} options
 * @param {?*} defaultValues default values to populate the locate representation of the resource with
 * @return {LinkedRepresentation}
 */
export const makeResourceFromFeedItem = (feedItem, resourceTitleAttributeName, options, defaultValues) => {
    const localResource = makeFromFeedItem(feedItem, resourceTitleAttributeName);
    return makeLinkedRepresentation(options, Object.assign(localResource, defaultValues));
};

/**
 * All items provided as a feedItems are transformed into sparse resources.
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceTitleAttributeName an option name for where the title from the feed
 *    item should be mapped to a {@link LinkedRepresentation}
 * @param {SparseResourceOptions} options
 * @return {CollectionRepresentation}
 */
export const makeCollectionItemsFromFeedItems = (collection, resourceTitleAttributeName, options) => {
    collection.items = _(collection.items).map(
        item => makeResourceFromFeedItem(item, resourceTitleAttributeName, options));
    return collection;
};

/*


/**
 * Make a new, sparsely populated {@link LinkedRepresentation} with {@link State} and
 * link relation 'self' populated with from given uri
 *
 * @param {string} uri
 * @param {*=} defaultValues
 * @param {stateFlagEnum=} state
 * @return {LinkedRepresentation}
 */
export const makeSparseResourceFromUri = (uri, defaultValues, state) => {
    if (!uri) {
        state = StateEnum.virtual;
    }
    return makeFromUri(uri, makeSparseResourceOptions(state || StateEnum.locationOnly), defaultValues);
};

/**
 * Make a new, sparsely populated {@link CollectionRepresentation} with {@link State} and
 * link relation 'self' populated with from given uri
 *
 * @param {string} uri
 * @param {*=} defaultValues
 * @param {StateEnum=} state
 * @return {CollectionRepresentation}
 */
export const makeSparseCollectionResourceFromUri = (uri, defaultValues, state) => {
    if (!uri) {
        state = StateEnum.virtual;
    }
    return makeCollectionFromUri(uri, makeSparseResourceOptions(state || StateEnum.locationOnly), defaultValues);
};

/**
 * Add a collection resource into the tree where the value is unknown (including the URI)
 *
 * @param {LinkedRepresentation} resource a parent resource container
 * @param {string} collectionResourceName
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {CollectionRepresentation}
 */
export const makeUnknownCollectionAddedToResource = (resource, collectionResourceName, defaultValues) => {
    return (resource => State.get(resource))(resource)
        .addCollectionResourceByName(
            resource,
            collectionResourceName,
            () => makeCollection(makeSparseResourceOptions(StateEnum.unknown), defaultValues));
};

/**
 * Add a resource into a collection in the tree where the value is unknown (including the URI)
 *
 * @param {CollectionRepresentation} collection
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation}
 */
export const makeUnknownResourceAddedToCollection = (collection, defaultValues) => {
    return State.makeItemToCollectionResource(collection, () => makeCollection(makeSparseResourceOptions(StateEnum.unknown), defaultValues));
};


/**
 * Add a resource into the tree where the value is unknown (including the URI)
 *
 * @param {LinkedRepresentation} resource the parent resource that will act as the container
 *   for the named child resource.
 * @param {string} resourceName the name of the child resource in the container
 * @param {LinkedRepresentation=} defaultValues optional default values for the resource
 * @return {LinkedRepresentation} resource existing as a child
 */
export const makeUnknownResourceAddedToResource = (resource, resourceName, defaultValues) => {
    return (resource => State.get(resource))(resource)
        .addResourceByName(
            resource,
            resourceName,
            () => makeLinkedRepresentation(makeSparseResourceOptions(StateEnum.unknown), defaultValues));
};


/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string} resourceUri
 * @param {*} defaultValues
 * @return {*|LinkedRepresentation}
 */
export const makeResourceFromUriAddedToCollection = (collection, resourceUri, defaultValues) => {
    return State.makeItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri, defaultValues));
};

/**
 *
 * @param {CollectionRepresentation} collection
 * @param {string[]} uriList
 * @return {CollectionRepresentation}
 */
export const makeCollectionItemsFromUriListAddedToCollection = (collection, uriList) => {
    const resourceState = (resource => State.get(resource))(collection);
    uriList.forEach(resourceUri =>
        resourceState.addItemToCollectionResource(collection, () => makeSparseResourceFromUri(resourceUri)));
    return collection;
};


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
 * Takes a feed representation and converts to a sparse collection representation
 *
 * @param {CollectionRepresentation} collection
 * @param {FeedRepresentation} feedRepresentation
 * @return {CollectionRepresentation}
 */
export const makeCollectionItemsFromFeedAddedToCollection = (collection, feedRepresentation) => {
    feedRepresentation.items.forEach(item => makeCollectionResourceItemByUri(collection, item.id, {name: item.title}));
    return collection;
};

/**
 * Adds a, or uses an existing, named collection on resource and then adds items into the collection based on a uri list
 *
 * @param {LinkedRepresentation} resource
 * @param {string} collectionResourceName
 * @param {string} collectionUri
 * @param {string[]} itemsUriList
 * @param {StateEnum} state
 * @return {LinkedRepresentation}
 */
export const makeNamedCollectionFromUriAndResourceFromUriList = (resource, collectionResourceName, collectionUri, itemsUriList, state) => {
    let collection = (resource => State.get(resource))(resource)
        .addCollectionResourceByName(
            resource,
            collectionResourceName,
            () => {
                if (!collectionUri) {
                    state = StateEnum.virtual;
                }
                return makeCollectionFromUri(collectionUri, makeSparseResourceOptions(state || StateEnum.locationOnly));
            });

    // only add items not currently loaded
    _(itemsUriList).each(uri => makeCollectionResourceItemByUri(collection, uri));
    return collection;
};

/**
 * Add a singleton list (array) of sparse LinkedRepresentations based on an attribute that has a uriList.
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string} itemsUriListName
 * @return {Promise} contains an array of sparsely populated resources
 */
export const makeSingletonSparseListFromAttributeUriList = (resource, singletonName, itemsUriListName) => {

    return State.get(resource)
        .then(resource => {

            if (!resource[singletonName]) {
                makeUnknownCollectionAddedToResource(resource, singletonName);
            }

            _(resource[itemsUriListName]).map(uri => {
                if (!resource[singletonName].items.find(item => link.getUri(item, /canonical|self/) === uri)) {
                    resource[singletonName].items.splice(resource[singletonName].length, 0, makeSparseResourceFromUri(uri));
                }
            });

            return resource[singletonName];
        });

};

/**
 * Add a singleton sparse LinkedRepresentations based on an uri.
 * @param {LinkedRepresentation} resource
 * @param {string} singletonName
 * @param {string} uri
 * @return {Promise} contains an array of populated resources
 */
export const makeSingletonSparseFromUri = (resource, singletonName, uri) => {
    return State.get(resource)
        .then(resource => {

            if (!resource[singletonName]) {
                resource[singletonName] = makeSparseResourceFromUri(uri);
            }

            return resource[singletonName];
        });
};

