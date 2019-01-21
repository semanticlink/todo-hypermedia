import {Representation} from "../query/interfaces";
import {NamedResourceSync, ResourceSync, SyncOptions} from "./interfaces";
import {
    getResource,
    getResourceInCollection,
    getResourceInNamedCollection,
    getSingleton,
    getCollectionInNamedCollection
} from "./syncLinkedRepresentation";
import {instanceOfCollection} from "../query/utils";
import {relTypeToCamel} from "../mixins/linkRel";
import {getUriListOnNamedCollection} from "./syncUriList";
import {UriList} from "../interfaces";
import {log} from "../index";


function instanceOfResourceSync<T>(obj: any): obj is ResourceSync<T> {
    return obj.resource !== 'undefined' && !obj.rel;
}


function instanceofUriList(obj: any): obj is UriList {
    return Array.isArray(obj) && typeof obj[0] === 'string';
}

export type SyncType<T> = ResourceSync<T> | NamedResourceSync<T>;

/**
 * Retrieves a resource (singleton or collection, either directly or through a link relation) and synchronises from
 * the given document. It then will recurse through all provides `strategies`.
 *
 * @example
 *
 *      ```sync({resource, document})```
 *
 *     Resource               Document
 *
 *                  sync
 *     +-----+                +-----+
 *     |     |  <-----------+ |     |
 *     |     |                |     |
 *     +-----+                +-----+
 *
 * @example
 *
 *  ```sync({resource: collection, document})```
 *
 *     resource
 *     Collection         Document
 *
 *     +-----+
 *     |     |
 *     |     |
 *     +-----+    sync
 *         X                +---+
 *         X  <-----------+ | x |
 *         X                +---+
 *           items
 *
 *  @example
 *
 *      ```sync(resource: parentResource, rel, document})```
 *
 *      parent      resource
 *      Resource    Collection        Document
 *
 *      +----------+
 *      |          |
 *      |          +-----+
 *      |     Named|     |
 *      |          |     |
 *      |          +-----+    sync
 *      |          |   X                +---+
 *      |          |   X  <-----------+ | x |
 *      +----------+   X                +---+
 *                       items
 *
 * @example
 *
 *  ```sync({resource: parentResource, rel, document: parentDocument})
 *
 *     parent     singleton           singleton   parent
 *     Resource    Resource            Document   Document
 *
 *     +----------+                            +---------+
 *     |          |            sync            |         |
 *     |          +-----+                +-----+         |
 *     |     Named|     |  <-----------+ |     |Named    |
 *     |          |     |                |     |         |
 *     |          +-----+                +-----+         |
 *     |          |                            |         |
 *     |          |                       ^    |         |
 *     +----------+                       |    +---------+
 *                                        |
 *                                        +
 *                                        looks for
 *
 * @example
 *
 *  ```sync({resource: parentResource, rel, document: parentDocument})```
 *
 *     parent      resource             document    parent
 *     Resource    Collection           Collection  Document
 *
 *     +----------+                            +----------+
 *     |          |            sync            |          |
 *     |          +-----+                +-----+          |
 *     |     Named|     |  <-----------+ |     |          |
 *     |          |     |                |     |          |
 *     |          +-----+                +-----+          |
 *     |          |   X                     X  |          |
 *     |          |   X items         items X  |          |
 *     +----------+   X                     X  +----------+
 *
 * @param syncAction
 */
export function sync<T extends Representation>(syncAction: SyncType<T>): Promise<T> | never {

    // shared configuration
    let cfg: ResourceSync<T> = <ResourceSync<T>>syncAction;
    const {resource, document, strategies = [], options = <SyncOptions>{}} = cfg;

    // resource or collection (directly)
    if (instanceOfResourceSync(syncAction)) {

        return instanceOfCollection(resource) && !instanceOfCollection(document)
            ? getResourceInCollection(resource, document, strategies, options)
            : getResource(resource, document, strategies, options);
    }

    // resource as named on a resource or collection
    // recast and extract the rel/name values
    const namedCfg = <NamedResourceSync<T>>syncAction;
    const {rel, name = relTypeToCamel(namedCfg.rel)} = namedCfg;

    if (!rel) {
        throw new Error('Sync of a named resource must have a rel specified in the options');
    }

    if (instanceofUriList(document)) {
        if (strategies) {
            log.warn('Strategies not available for uri-list');
        }
        return getUriListOnNamedCollection(resource, name, rel, document, options);
    }

    if (instanceOfCollection(document[name])) {
        return getCollectionInNamedCollection(resource, name, rel, document[name], strategies, options);
    }

    return instanceOfCollection(resource)
        ? getResourceInNamedCollection(resource, name, rel, document, strategies, options)
        : getSingleton(resource, name, rel, document, strategies, options);

}
