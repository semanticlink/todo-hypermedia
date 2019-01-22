import {QueryOptions} from "./interfaces";
import {instanceOfCollection} from "./utils";
import {deleteCollectionItem, deleteResource, updateResource} from "../cache";
import {Representation} from "../interfaces";

/**
 * Update a resource
 *
 * @param resource resource must include the 'edit-form' link relation
 * @param document the values to make the new state into
 * @param options query options to remove an item from a collection
 * @returns the resource that was logically deleted
 */
export function update<T extends Representation>(resource: T, document: T, options?: QueryOptions): Promise<T> {

    if (instanceOfCollection(resource)) {
        throw new Error(`PUT on collection not implemented here`);
    }

    return updateResource(resource, document, options);
}