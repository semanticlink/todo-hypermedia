import {getUri} from 'semantic-link';
import {log} from 'logger';
import {pooledTagResourceResolver} from 'domain/tags';
import {query, uriMappingResolver} from 'semantic-link-cache';
import {sync} from 'semantic-link-cache/sync/sync';
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
    query.get(apiResource, /me/, options)
        .then(user => query.get(user, /tenants/, {includeItems: true, ...options}));
/**
 * Get the users that exist on a user tenant
 *
 * @param userTenantsCollection
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTenantUsers = (userTenantsCollection, options) =>
    query.get(userTenantsCollection, /users/, {includeItems: true, ...options});

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
 * Clone a graph of aTenant todo lists
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
            return sync({
                resource: userTenants,
                document: aTenant,
                strategies: [(userCollection, users, options) => sync({
                    resource: userCollection,
                    rel: /todos/,
                    document: users,
                    strategies: [(todoListCollection, todoLists, options) => sync({
                        resource: todoListCollection,
                        rel: /todos/,
                        document: todoLists,
                        strategies: [(todoCollection, todos, options) => sync({
                            resource: todoCollection,
                            rel: /tags/,
                            document: todos,
                            options
                        })],
                        options
                    })],
                    options
                }),
                ],
                options: {
                    ...options,
                    ...pooledTagResourceResolver(apiResource),
                    resolver: uriMappingResolver
                }
            });
        });

};