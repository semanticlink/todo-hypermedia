import * as link from "semantic-link";
import {
    getCollection,
    getCollectionItem,
    getCollectionItemByUri,
    getNamedCollection,
    getNamedCollectionAndItems,
    getNamedCollectionItemByUri,
    getNamedCollectionOnSingletons,
    getResource,
    getSingleton,
    tryGetNamedCollectionAndItemsOnCollectionItems,
    tryGetResource,
    tryGetSingleton
} from "../cache";
import {relTypeToCamel} from "../mixins/linkRel";
import {QueryOptions, Representation} from "./interfaces";
import {instanceOfCollection} from "./utils";

/**
 * Helper part of get for duplicated code that deals with allowing transparently for whether the context
 * is a singleton or collection.
 *
 * @param context
 * @param resource
 * @param options
 * @private
 */
function getOnContext<T extends Representation>(context: T, resource: T, options: QueryOptions)
    : Promise<T> {

    const {rel, name, includeItems, defaultRepresentation, where} = options;

    if (instanceOfCollection(context)) {

        if (where != undefined) {
            if (typeof where === 'string') {
                return getNamedCollectionItemByUri(resource, name, rel, where, options);
            } else {
                throw new Error('Loading an item based on a representation on where is not implemented');
            }
        }

        if (instanceOfCollection(resource) && rel != undefined && includeItems) {
            // @ts-ignore (cannot understand why this has type warning)
            return tryGetNamedCollectionAndItemsOnCollectionItems(resource, name, rel, options);
        }

        if (includeItems) {
            return getNamedCollectionAndItems(resource, name, rel, options);
        }

        return getNamedCollection(resource, name, rel, options);
    }
    return defaultRepresentation != undefined
        ? tryGetSingleton(resource, name, rel, defaultRepresentation, options)
        : getSingleton(resource, name, rel, options);
}

/**
 * Get resources and related resources.
 *
 * @remarks
 *
 * This should be simply approach that covers most situations. If not, either drop down into the {@link cache} library
 * or extend this one.
 *
 * @param resource
 * @param options query options to specify the depth and width to be retrieved
 * @returns the target resource which may be the originating or target resource
 */
export function get<T extends Representation>(resource: T | T[], options?: QueryOptions): Promise<T | T[]> {

    // basic guard that allow users to just to pass in the rel either as a string or Regexp that
    // will get converted to a stringly name that is added the representation. The developer at
    // anytime can provide both
    if (options && options.rel) {
        options.name = options.name || relTypeToCamel(options.rel);
    }

    const {rel, name, where, defaultRepresentation}: QueryOptions = {...options, opts: <QueryOptions>{}};

    // named resources
    if (rel != undefined) {

        // T[]
        if (resource instanceof Array) {
            return getNamedCollectionOnSingletons(resource, name, rel, options) as Promise<T[]>;
        }

        // T existing
        if (resource[rel]) {
            return getOnContext(resource[rel], resource, options);
        } else {

            // T - to fetch
            // look ahead to be able to deal with whether it is a collection or not
            // shouldn't be too much of a penalty
            // @ts-ignore (difficulty inter-operating with js library)
            return link.get(resource, rel /*, options.cancellable */)
            // @ts-ignore
                .then(response => {
                    return getOnContext(response.data, resource, options);
                });
        }
    }

    // collection
    if (instanceOfCollection(resource)) {

        // find specific item in collection
        if (where != undefined) {

            // use uri for identity
            if (typeof where === 'string') {
                return getCollectionItemByUri(resource, where, options);

            }
            // use link rel 'self' for identity
            return getCollectionItem(resource, where, options);
        }

        return getCollection(resource, options);
    }

    // singleton
    return defaultRepresentation
        ? tryGetResource(resource, defaultRepresentation, options)
        : getResource(resource, options);
}