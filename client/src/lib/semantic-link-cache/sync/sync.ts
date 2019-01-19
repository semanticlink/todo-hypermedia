import {Representation} from "../query/interfaces";
import {NamedResourceSync, ResourceSync, SyncOptions} from "./interfaces";
import {
    getResource,
    getResourceInCollection,
    getResourceInNamedCollection,
    getSingleton,
    getCollectionInNamedCollection,
    getNamedCollectionInNamedCollection
} from "./syncLinkedRepresentation";
import {instanceOfCollection} from "../query/utils";
import {relTypeToCamel} from "../mixins/linkRel";


function instanceOfResourceSync<T>(obj: any): obj is ResourceSync<T> {
    return obj.resource !== 'undefined' && !obj.rel;
}

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
 *  @example
 *
 *      ```sync({resource: parentResource, rel, document: documentCollection})```
 *
 *      parent     resource              document
 *      Resource   Collection            Collection
 *
 *     +----------+
 *     |          |            sync
 *     |          +-----+                +-----+
 *     |     Named|     |  <-----------+ |     |
 *     |          |     |                |     |
 *     |          +-----+                +-----+
 *     |          |   X                     X
 *     |          |   X items               X items
 *     +----------+   X                     X
 *
 * @example
 *
 *  ```sync({resource: parentResource, rel, document: parentDocument, documentRel})```
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
export function sync<T extends Representation>(syncAction: NamedResourceSync<T> | ResourceSync<T>): Promise<T> | never {

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
    const {rel, name = relTypeToCamel(namedCfg.rel), documentRel} = namedCfg;

    if (!rel) {
        throw new Error('Sync of a named resource must have a rel specified in the options');
    }

    if (instanceOfCollection(document)) {
        return instanceOfCollection(resource) && !documentRel
            ? getCollectionInNamedCollection(resource, name, rel, document, strategies, options)
            : getNamedCollectionInNamedCollection(resource, name, rel, document, strategies, options);
    }

    return instanceOfCollection(resource)
        ? getResourceInNamedCollection(resource, name, rel, document, strategies, options)
        : getSingleton(resource, name, rel, document, strategies, options);

}
