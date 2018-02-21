/**
 * This is a documentation file of the client-side artifact representations
 */

/**
 * @class ApiRepresentation
 * @extends LinkedRepresentation
 * @property {string} version
 */

/**
 * @class TenantCollectionRepresentation
 * @extends CollectionRepresentation
 * @property {LinkedRepresentation[]} tenants
 * @property {TenantSearchRepresentation} searchForm
 */

/**
 * @class TenantRepresentation
 * @extends LinkedRepresentation
 * @property {LinkedRepresentation[]} name
 */

/**
 * @class TenantSearchRepresentation
 * @extends LinkedRepresentation
 * @property {LinkedRepresentation[]} search
 */


/**
 * @class TodoCollectionRepresentation
 * @extends CollectionRepresentation
 */

/**
 * @class TodoRepresentation
 * @extends LinkedRepresentation
 * @property {string} name
 * @property {boolean} completed
 */