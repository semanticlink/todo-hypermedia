import * as link from "semantic-link";
import {CollectionRepresentation, LinkedRepresentation, RelationshipType, Uri} from "semantic-link";
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
} from "../semantic-link-cache/cache";
import {relTypeToCamel} from "../semantic-link-cache/mixins/linkRel";

const items = 'items';
/**
 *
 */
type Representation = (CollectionRepresentation | any) & LinkedRepresentation;

/**
 * Options for be able to traverse the semantic network of data
 */
type QueryOptions = {
    /**
     * If this is set then the get will perform a `tryGet` and return default representation on failure
     */
    defaultRepresentation?: Representation,
    /**
     * Identifies the child resource in a collection by its identity (either as 'self' link rel or a Uri)
     */
    where?: Representation | Uri,
    /**
     * Identifies the link rel to follow to add {@link LinkedRepresentation} onto the resource.
     */
    rel?: RelationshipType,
    /**
     * The name of the attribute that the {@link LinkedRepresentation} is added on the resource. Note: this
     * value is defaulted based on the {@link QueryOptions.rel} if not specified. If the {@link QueryOptions.rel} is
     * an array then this value must be explicitly set.
     */
    name?: string
    /**
     * Alters the hydration strategy for collections. By default collections are sparsely populated (that is
     * the `items` attribute has not gone to the server to get all the details for each item).
     * {@link QueryOptions.include} currently flags that it should go and fetch each item.
     */
    includeItems?: boolean
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
 *
 * @param resource
 * @param options
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

        if (where != undefined) {

            if (typeof where === 'string') {
                return getCollectionItemByUri(resource, where, options);

            }
            // else Representation
            return getCollectionItem(resource, where, options);
        }

        return getCollection(resource, options);
    }

    // singleton
    return defaultRepresentation
        ? tryGetResource(resource, defaultRepresentation, options)
        : getResource(resource, options);
}
