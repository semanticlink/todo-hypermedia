import {
    Cancellable,
    CollectionRepresentation,
    Link,
    LinkedRepresentation,
    RelationshipType,
    Uri
} from "semantic-link";
import {Representation} from "./query/interfaces";
import {UriResolver} from "./sync/interfaces";

export type UriList = string | string[];

/**
 * The current types of form inputs that are supported from semantic link
 *
 * @remarks
 *
 * Note: these are hard coded in {@link ResourceMerger} and have avoided enums because of the mix of typescript and javascript
 */
export type FormType =
    'http://types/text' |
    'http://types/text/password' |
    'http://types/text/email' |
    'http://types/text/check' |
    'http://types/text/date' |
    'http://types/text/datetime' |
    'http://types/text/select';

export interface FormItem {
    readonly type: FormType | string;
    readonly name: string;
    readonly description?: string;
    readonly required?: boolean;
    readonly items?: FormItem[];
}

export interface FormRepresentation extends LinkedRepresentation {
    items: FormItem[]
}

export interface UtilOptions extends CacheOptions, StateOptions, SyncOptions, DifferencerOptions, EditMergeOptions {

}

export interface StateOptions {
    mappedTitle: string;
    rel: RelationshipType
    /**
     * Allows for specific implementation to override the default {@link getUri} implementation from semantic link.
     * @param resource
     * @param rel
     */
    getUri: <T extends Representation>(resource: T, rel: RelationshipType) => Uri
    /**
     * Allows for a specific implementation to override the default semantic link implementation
     * @param link
     */
    resourceFactory: <T extends Representation>(link: Link) => T
}

export interface CacheOptions {
    timeout: Promise<any | void>

    deleteFactory: <T extends Representation>(resource: T, options?: UtilOptions) => Promise<void>
    getFactory: <T extends Representation>(resource: T, rel: RelationshipType, cancellable: Cancellable) => Promise<T | void>
    putFactory: <T extends Representation>(resource: T, data: T, options?: UtilOptions) => Promise<T | void>
    postFactory: <T extends Representation>(resource: T, data: T, options?: UtilOptions) => Promise<T | void>
}

/**
 * Messages of a string format that are propagated up to the user when syncing networks of data
 */
export type UserMessage = string;

/**
 * A resolver on {@link UriList}
 */
export interface UriListResolver {
    (resource: Representation): UriList
}

export interface ResourceResolver {
    (resource: any): Representation
}

export interface SyncOptions {


    /**
     * Set the size of the batches of requests on `sync`.
     *
     * When `sync` is moving through its child strategies the requests can be either sequential or parallel. Currently,
     * a non-zero number sets the strategy as sequential. The default value is 'undefined' or 0 to invoke a parallel
     * strategy.
     */
    childStrategyBatchSize: number
    batchSize: number
    /**
     * When set to true, the next check on the resource ensures that it flushes through the stack
     */
    forceLoad: boolean
    /**
     * Ensures that the resource is created
     */
    forceCreate: boolean
    /**
     * Marks a collection as read-only - you'd don't get the ability to add items
     */
    readonly: boolean
    /**
     * Marks a collection as mutable - you only get to remove/add items from the collection (you may or may
     * not be able to delete/create the items themselves)
     */
    contributeonly: boolean

    /**
     * General
     */
    message: UserMessage
    error: UserMessage
    success: UserMessage

    findResourceInCollectionStrategy: <T extends Representation>(collection: CollectionRepresentation, document: T) => T

    resolver: UriResolver
    resourceResolver: ResourceResolver
    uriListResolver: UriListResolver
}

/**
 * A set of comparators for matching resources in the network of data differencer (@link Differencer}
 *
 * You can also add your own. TODO: no registration, just code them in
 *
 * @example
 *
 * Specific maters for role-filters on a report template. It requires that both the role and the filter match
 * from the link relations:
 *
 * {
 *
 *    "links": [
 *        {
 *            "rel": "self",
 *            "href": "http://localhost:1080/role/filter/408"
 *        },
 *        {
 *            "rel": "up",
 *            "href": "http://localhost:1080/report/template/4991"
 *        },
 *        {
 *            "rel": "filter",
 *            "href": "http://localhost:1080/filter/1"
 *        },
 *        {
 *            "rel": "role",
 *            "href": "http://localhost:1080/role/11"
 *
 *    ]
 * }
 *
 *  byLinkRelation(lvalue, rvalue) {
 *      return link.matches(lvalue, /^role$/) === link.matches(rvalue, /^role$/) &&
 *          link.matches(lvalue, /^filter$/) === link.matches(rvalue, /^filter$/);
 *  }
 *
 */
export interface Comparator {
    (lvalue: Representation, rvalue: Representation): boolean;
}

/**
 * A data structure provided by the {@link diffCollection} method as a return value
 * in the promise.
 */
export interface DifferencerOptions {
    /**
     * A function that should create a single resource. It must return a promise with the resource (created on the collection resource)
     * @param createDataDocument
     */
    createStrategy: <T extends Representation>(createDataDocument: T) => Promise<T>
    /**
     * Conditionally update the given resource using the provides document. The implementation can
     *  choose to not update the resource if its state is equivalent. return a promise with no parameters
     * @param resource
     * @param update
     */
    updateStrategy: <T extends Representation>(resource: T, update: T) => Promise<void>
    /**
     * Delete a resource
     * @param resource
     */
    deleteStrategy: <T extends Representation>(resource: T) => Promise<void>

    /**
     * A set of comparators for matching resources in the network of data (@link Differencer} to compute the identity
     * of an object based on a transformation
     */
    comparators: Comparator[]
}

/**
 * Options available when merging resource via a three-way merger
 */
export interface EditMergeOptions {
    /**
     * When merging resources, these are additional fields that can be added by default from the form resource.
     *
     * @remarks
     * The current implementation always includes the 'name' attribute
     */
    defaultFields?: string[]
    /**
     * When 'true' return 'undefined' from the edit merge rather than the merged document.
     */
    undefinedWhenNoUpdateRequired?: boolean
    /**
     * On resource state, there are fields that are added by the cache - these are tracked fields. This overrides the
     * default implementation when needed which overwrites all fields.
     *
     * @param resource
     */
    isTracked: (resource: LinkedRepresentation) => boolean
}