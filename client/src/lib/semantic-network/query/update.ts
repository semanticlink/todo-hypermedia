import {QueryOptions} from "./interfaces";
import {instanceOfCollection, instanceOfUriList} from "./utils";
import {updateCollection, updateResource} from "../cache";
import {Representation} from "../interfaces";

/**
 * Update a resource. When not a collection then PUT, when a collection PATCH using {@link UriList}
 *
 * @param resource resource must include the 'edit-form' link relation
 * @param document the values to make the new state into
 * @param options query options to remove an item from a collection
 * @returns the resource that was logically deleted
 */
export function update<T extends Representation>(resource: T, document: T, options?: QueryOptions): Promise<T | never> {

    // PATCH
    if (instanceOfCollection(resource)) {

        if (!instanceOfUriList(document)) {
            throw new Error('To update a collection, a document of type UriList must be supplied');
        }

        return updateCollection(resource, document, options)
    }

    // PUT
    return updateResource(resource, document, options);
}