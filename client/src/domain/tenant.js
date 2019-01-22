import {getUri} from 'semantic-link';
import {log} from 'logger';
import {pooledTagResourceResolver} from 'domain/tags';
import {get, uriMappingResolver, sync} from 'semantic-link-cache';
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
 * @param {CacheOptions?} options
 * @returns {Promise<TenantCollectionRepresentation>}
 */
export const getTenantsOnUser = (apiResource, options) =>
    get(apiResource, /me/, options)
        .then(user => get(user, /tenants/, {includeItems: true, ...options}));
/**
 * Get the users that exist on a user tenant
 *
 * @param userTenantsCollection
 * @param {CacheOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTenantUsers = (userTenantsCollection, options) =>
    get(userTenantsCollection, /users/, {includeItems: true, ...options});

/**
 * Loads up a tenant to be copied with todos and users
 *
 * @param {TenantRepresentation} tenant
 * @param {CacheOptions?} options
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
 * @param {CacheOptions?} options
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
                strategies: [syncResult => sync({
                    ...syncResult,
                    rel: /todos/,
                    strategies: [syncResult => sync({
                        ...syncResult,
                        rel: /todos/,
                        strategies: [syncResult => sync({...syncResult, rel: /tags/})],
                    })],
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