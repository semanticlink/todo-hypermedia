import {getUri} from 'semantic-link';
import {log} from 'logger';
import {pooledTagResourceResolver} from 'domain/tags';
import {uriMappingResolver, sync, query} from 'semantic-link-cache';
import {getTodosWithTagsOnTenantTodos} from 'domain/todo';


/***********************************
 *
 * Retrieve tenant information
 * ===========================
 */


/**
 * Get the tenants that an authenticated user has access to
 *
 * Context: (api)
 * Access: -(me)-[tenants...]
 *
 * @param {ApiRepresentation} apiResource
 * @param {UtilOptions?} options
 * @returns {Promise<TenantCollectionRepresentation>}
 */
export const getTenantsOnUser = (apiResource, options) =>
    query.get(apiResource, {rel: /me/, ...options})
        .then(user => query.get(user, {rel: /tenants/, includeItems: true, ...options}));
/**
 * Get the users that exist on a user tenant
 *
 * @param userTenantsCollection
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTenantUsers = (userTenantsCollection, options) =>
    query.get(userTenantsCollection, {rel: /users/, includeItems: true, ...options});

/**
 * Loads up a tenant to be copied with todos and users
 *
 * @param {TenantRepresentation} tenant
 * @param {UtilOptions?} options
 * @returns {Promise<TenantRepresentation>}
 */
export const getUserTenant = (tenant, options) =>
    Promise.all([getTodosWithTagsOnTenantTodos(tenant.todos, options), getTenantUsers(tenant, options)])
        .then(() => tenant);

/***********************************
 *
 * Sync
 * ====
 */

/**
 * Sync the tenant in the context of the tenant collection
 *
 * @param {TenantCollectionRepresentation} tenantCollection
 * @param {*|TenantCollectionRepresentation} aTenant
 * @param {{function(TenantRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions?} options
 * @returns {Promise}
 */
const syncTenantStrategy = (tenantCollection, aTenant, strategies, options) =>
    sync.getResourceInCollection(tenantCollection, aTenant, strategies, options);

/**
 * Sync the todos in the context of a user collectoin
 *
 * @param {UserCollectionRepresentation} user
 * @param {*|UserCollectionRepresentation} aUser
 * @param {{function(UserCollectionRepresentation, LinkedRepresentation, UtilOptions):Promise}[]} strategies
 * @param {UtilOptions?} options
 * @returns {Promise}
 */
const syncTodosStrategy = (user, aUser, strategies, options) =>
    sync.getNamedCollectionInNamedCollection(user, 'todos', /todos/, aUser, strategies, options);

/**
 * Sync the tags in the context of a todo
 *
 * @param {TodoRepresentation} todo
 * @param {*|TodoCollectionRepresentation} aTodo
 * @param {UtilOptions?} options
 * @returns {Promise}
 */
const syncTagsStrategy = (todo, aTodo, options) =>
    sync.getNamedCollectionInNamedCollection(todo, 'tags', /tags/, aTodo, [], options);


/**
 * Clone a graph of tenant todo lists
 *
 * Context: (api)-(me)-[tenants]
 * Access: [todos...]-[todos...]-[tags]
 * Pool: (api)-[tags]
 *
 * @param {ApiRepresentation} apiResource
 * @param {TenantRepresentation} aTenant
 * @param {UtilOptions?} options
 * @returns {Promise<TenantCollectionRepresentation>}
 */
export const syncTenant = (apiResource, aTenant, options) => {

    if (!aTenant) {
        throw new Error('Tenant is empty');
    }

    log.debug(`[Tenant] start create ${aTenant.name} ${aTenant.code}`);

    return getTenantsOnUser(apiResource, options)
        .then(userTenants => {
            log.debug(`[Tenant] users loaded ${getUri(userTenants, /self/)}`);
            return syncTenantStrategy(
                userTenants,
                aTenant,
                [
                    (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                        usersRepresentation,
                        usersDocument,
                        [
                            (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                                usersRepresentation,
                                usersDocument,
                                [
                                    (todoRepresentation, todoDocument, options) =>
                                        syncTagsStrategy(todoRepresentation, todoDocument, options)

                                ],
                                options)
                        ],
                        options),
                ],
                {
                    ...options,
                    ...pooledTagResourceResolver(apiResource),
                    resolver: uriMappingResolver
                });
        });

};