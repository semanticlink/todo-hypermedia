import {cache} from '../semantic-link-cache';
import * as link from "semantic-link";
import {CollectionRepresentation, LinkedRepresentation} from "semantic-link";
import {getCollection, getNamedCollection, getSingleton} from "../semantic-link-cache/cache";

const items = 'items';
/**
 *
 */
type Representation = (CollectionRepresentation | any) & LinkedRepresentation;

/**
 * Include the items in a collection to be {@link StateEnum.hydrated}
 */
type Include = 'items' | true;

/**
 * Options for be able to traverse the semantic network of data
 */
type GetOptions = {
    /**
     * If this is set then the get will perform a `tryGet` and return default representation on failure
     */
    defaultRepresentation?: Representation,
    /**
     * Identifies the child resource in a collection by its identity (either as 'self' link rel or a Uri)
     */
    where?: Representation | string,
    /**
     *
     */
    rel?: string,

    include?: Include
} & {} | any;

/**
 * A guard to detect whether the object is a {@link CollectionRepresentation}
 *
 * @see https://stackoverflow.com/questions/14425568/interface-type-check-with-typescript
 * @param object
 * @returns whether the object is an instance on the interface
 * @private
 */
function instanceOfCollection(object: any): object is CollectionRepresentation {
    return items in object;
}


function getOnContext<T extends Representation>(context: T, resource: T, options: GetOptions)
    : Promise<T> {

    const {rel, include, defaultRepresentation, where} = options;

    if (instanceOfCollection(context)) {

        if (where != undefined) {
            if (typeof where === 'string') {
                return cache.getNamedCollectionItemByUri(resource, rel, rel, where, options);
            } else {
                throw new Error('Loading an item based on a representation on where is not implemented');
            }
        }

        if (instanceOfCollection(resource) && rel != undefined && include === items) {
            // @ts-ignore (cannot understand why this has type warning)
            return cache.tryGetNamedCollectionAndItemsOnCollectionItems(resource, rel, rel, options);
        }

        if (include === items) {
            return cache.getNamedCollectionAndItems(resource, rel, rel, options);
        }

        return cache.getNamedCollection(resource, rel, rel, options);
    }
    return defaultRepresentation != undefined
        ? cache.tryGetSingleton(resource, rel, rel, defaultRepresentation, options)
        : cache.getSingleton(resource, rel, rel, options);
}

/**
 *
 * @param resource
 * @param options
 * @returns the target resource which may be the originating or target resource
 */
export function get<T extends Representation>(resource: T | T[], options?: GetOptions): Promise<T | T[]> {

    const {rel, where, defaultRepresentation, include}: GetOptions = {...options, opts: <GetOptions>{}};

    // named resources
    if (rel != undefined) {

        // T[]
        if (resource instanceof Array) {
            return cache.getNamedCollectionOnSingletons(resource, rel, rel, options) as Promise<T[]>;
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

        if (where != undefined) {

            if (typeof where === 'string') {
                return cache.getCollectionItemByUri(resource, where, options);
            }
            // else Representation
            return cache.getCollectionItem(resource, where, options);
        }

        return cache.getCollection(resource, options);
    }

    // singleton
    return defaultRepresentation
        ? cache.tryGetResource(resource, defaultRepresentation, options)
        : cache.getResource(resource, options);
}
