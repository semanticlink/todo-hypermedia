import _ from 'underscore';

/**
 * @class SparseResourceOptions
 * @extends UtilOptions
 * @property {{Symbol:State}} stateFactory creates a state object to be added to a resource
 */

/**
 * A set of factory methods to create sparse resources
 */
export default class SparseResource {

    /**
     * Default mapped title between feed items and sparse resources
     * @type {string}
     */
    static get mappedTitle () {
        return 'name';
    }

    /**
     * Default state factory will return an empty object
     * @returns {function():{}}
     */
    static get defaultStateFactory () {
        return () => {
            return {};
        };
    }

    /**
     *
     * @param {FeedItemRepresentation} feedItem
     * @param {string=} resourceTitleAttributeName
     * @return {{links: *[]}}
     */
    static makeFromFeedItem (feedItem, resourceTitleAttributeName) {

        let localResource = {
            links: [{
                rel: 'self',
                href: feedItem.id
            }]
        };
        localResource[resourceTitleAttributeName || SparseResource.mappedTitle] = feedItem.title;
        return localResource;
    }

    /**
     * Make a new, sparsely populated {@link LinkedRepresentation}.
     * @param {SparseResourceOptions} options
     * @param {?*} defaultValues
     * @return {LinkedRepresentation|CollectionRepresentation}
     */
    static makeLinkedRepresentation (options, defaultValues) {
        options = options || {};

        if (!options.stateFactory) {
            options.stateFactory = SparseResource.defaultStateFactory;
        }

        return Object.assign(options.stateFactory(), {links: []}, defaultValues);
    }

    /**
     * Make a new, sparsely populated {@link LinkedRepresentation} potentially with {@link State} and
     * link relation 'self' populated with from given uri. if href is undefined the resource should be in state virtual
     * @param {string} uri
     * @param {SparseResourceOptions} options
     * @param {?*} defaultValues
     * @return {LinkedRepresentation|CollectionRepresentation}
     */
    static makeFromUri (uri, options, defaultValues) {

        const localResource = {links: [{rel: 'self', href: uri}]};
        return SparseResource.makeLinkedRepresentation(options, Object.assign(localResource, defaultValues));
    }

    /**
     * Make a new, sparsely populated {@link CollectionRepresentation} potentially with {@link State}
     *
     * @param {SparseResourceOptions} options
     * @param {?*} defaultValues
     * @param {?[{rel:string,href:string,title:string}]}defaultItems
     * @return {CollectionRepresentation}
     */
    static makeCollection (options, defaultValues, defaultItems = []) {
        return SparseResource.makeLinkedRepresentation(options, Object.assign({items: defaultItems}, defaultValues));
    }

    /**
     * Make a new, sparsely populated {@link CollectionRepresentation} potentially with {@link State} and
     * link relation 'self' populated with from given uri
     *
     * @param {string} uri
     * @param {SparseResourceOptions} options
     * @param {?*} defaultValues
     * @return {CollectionRepresentation}
     */
    static makeCollectionFromUri (uri, options, defaultValues) {
        return SparseResource.makeFromUri(uri, options, Object.assign({items: []}, defaultValues));
    }

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
    static makeResourceFromFeedItem (feedItem, resourceTitleAttributeName, options, defaultValues) {
        const localResource = SparseResource.makeFromFeedItem(feedItem, resourceTitleAttributeName);
        return SparseResource.makeLinkedRepresentation(options, Object.assign(localResource, defaultValues));
    }

    /**
     * All items provided as a feedItems are transformed into sparse resources.
     *
     * @param {CollectionRepresentation} collection
     * @param {string} resourceTitleAttributeName an option name for where the title from the feed
     *    item should be mapped to a {@link LinkedRepresentation}
     * @param {SparseResourceOptions} options
     * @return {CollectionRepresentation}
     */
    static mapFeedItemsToCollectionItems (collection, resourceTitleAttributeName, options) {
        collection.items = _(collection.items).map(
            item => SparseResource.makeResourceFromFeedItem(item, resourceTitleAttributeName, options));
        return collection;
    }
}