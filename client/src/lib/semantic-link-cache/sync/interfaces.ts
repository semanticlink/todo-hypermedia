import {Representation} from "../query/interfaces";
import {LinkedRepresentation, RelationshipType, Uri} from "semantic-link";
import {SyncOptions} from "../interfaces";

export interface SyncResult<T extends Representation> {
    resource: T;
    document: T;
    options: SyncOptions;
}

// export type StrategyType = <T extends Representation>(resource: T, document: T, options: SyncOptions) => Promise<void>;
export type StrategyType = <T extends Representation>(syncResult: SyncResult<T>) => Promise<void>;

export interface ResourceSync<T extends Representation> {
    resource: T;
    document: T;
    strategies?: StrategyType[];
    options?: SyncOptions;
}

export interface NamedResourceSync<T extends Representation> extends ResourceSync<T> {
    /**
     * Link rel on the parent (ie context) resource to be followed
     */
    rel: RelationshipType;
    /**
     * The attribute name of the named resource that is added to the in-memory resource. This is an override value
     * where the default is a stringly-type of {@link rel}.
     */
    name?: string;
}

/**
 * Action to make on resource
 */
export type SyncInfoAction = 'create' | 'update' | 'delete';

/**
 * Internal data structure for working out which action to perform on documents when syncing.
 * @private
 */
export interface SyncInfo {
    resource: Representation
    document: Representation
    action: SyncInfoAction
}

/**
 *   An array of 4 values for the form [SyncInfo[], createlist, updatelist, deletelist].
 *
 *   Where the first collection is the collection resource grouped with the document data. The createlist is
 *   an array describing the resources created. The update list is an array that describes the resources
 *   updated. The delete list describes the resources deleted.</p>
 *
 *   The create list items are an array of two values. The first item is the new item returned from the
 *   createStrategy promise. The second item is the create data provided to the createStrategy.
 *
 *   The update list item are an array of two values. The first value is the resource from the
 *   collection resource that is provided to the updateStrategy. The second item is the update
 *   data from the document resource used to update the resource.
 *
 *   The delete list items are an array with a single value. The value is the resource from
 *   the collection resource prior to being deleted.
 */
export type SyncResultItem = [SyncInfo[], [LinkedRepresentation[], LinkedRepresentation[]], [LinkedRepresentation[], LinkedRepresentation[]], LinkedRepresentation[]];

export type SyncInfoResult = Promise<SyncResultItem[]>;

/**
 * Used for provisioning the resources (network of data) based on providing new document (resources). Based
 * on a difference set this class synchronises between the client version and the api
 * @private
 */
export interface UriResolver {
    /**
     * Based on a known {@link Uri} what should it now be resolved to
     * @param key
     */
    resolve: (key: Uri) => Uri
    /**
     * Add a new {@link Uri} into the resolver for later resolution
     * @param key
     * @param value
     */
    add: (key: Uri, value: Uri) => void
    /**
     * Update (or add new) an existing {@link Uri} value for a known resolver key
     * @param key
     * @param value
     */
    update: (key: Uri, value: Uri) => void
    /**
     * Remove the know {@link Uri} entry so that it is no longer resolved
     * @param key
     */
    remove: (key: Uri) => void
}