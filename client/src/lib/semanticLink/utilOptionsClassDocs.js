/**
 *     *******************************************************************
 *     *                                                                 *
 *     *     This is a JSDoc documentation stub. Don't delete it.        *
 *     *                                                                 *
 *     *******************************************************************
 */

/**
 * @class UtilOptions
 * @property {Promise|undefined} timeout
 * @property {injectableNodMakerService} resolver
 * @property {string} mappedTitle=name defaults to {@link ResourceState.mappedTitle}
 * @property {string|RegExp|string[]|RegExp[]} rel relationshipType
 * @property {function(LinkedRepresentation,*):string} getUri allows swapping out to the tryGetUri (single string) or filter (array link relations)
 * @property {int} childStrategyBatchSize
 * @property {int} batchSize
 * @property {boolean} forceLoad ensures that the resource is refreshed
 * @property {boolean} forceCreate ensures that the resource is created
 * @property {boolean} readonly marks a collection as readonly (read/write) - you don't get to add or create items
 * @property {boolean} contributeonly marks a collection as mutable - you only get to remove/add items from the collection (you may or may not be able to delete/create the items themselves)
 * @property {function(LinkedRepresentation, ?UtilOptions):Promise} deleteFactory
 * @property {function(resource:LinkedRepresentation, rel:string, cancellable:?Promise):Promise} getFactory
 * @property {function(resource:LinkedRepresentation, data:*, options: ?UtilOptions):Promise} putFactory
 * @property {function(link{href:string,title:string}:LinkedRepresentation} resourceFactory
 * @property {string} message usually a success message to be displayed to the user
 * @property {Array<function(*,*)>} resourceResolver array of resolvers that that are keyed on link relation to factory resources
 * @property {function(*):string[]} uriListResolver array of resolvers that that are keyed on link relation to factory uri lists
 * @property {function(collection:CollectionRepresentation, document:LinkedRepresentation):LinkedRepresentation} findResourceInCollectionStrategy - locate a resource in a collection
 */
