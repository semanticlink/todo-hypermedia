import {_} from 'semantic-link-cache';
import {getUri} from 'semantic-link';
import {log} from 'logger';
import {pooledTagResourceResolver} from 'domain/tags';
import {findResourceInCollection} from 'semantic-link-cache/mixins/collection';
import {uriMappingResolver, sync, cache} from 'semantic-link-cache';
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
    cache.getSingleton(apiResource, 'me', /me/, options)
        .then(user => cache.getNamedCollectionAndItems(user, 'tenants', /tenants/, options));

/**
 *
 * Context: (api)
 * Access: -[tenants...]
 * @param apiResource
 * @param {UtilOptions?} options
 * @returns {Promise<TenantCollectionRepresentation>}
 */
export const getTenants = (apiResource, options) => cache.getNamedCollection(apiResource, 'tenants', /tenants/, options);

/**
 *
 * @param apiResource
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation|undefined>}
 */
const getTags = (apiResource, options) => cache.tryGetCollectionAndItems(apiResource, 'tags', /tags/, options);

/**
 * @obsolete
 */
export const getTenantAndTodos = root => {
    return Promise.all([getTenants(root), getTags(root)])
        .then(([tenantCollection]) => cache.tryGetNamedCollectionAndItemsOnCollectionItems(tenantCollection, 'users', /users/))
        .then(tenantCollectionItems => _(tenantCollectionItems)
            .mapWaitAll(tenant => _(tenant)
                .mapFlattenWaitAll(user => cache.tryGetCollectionAndItems(user, 'todos', /todos/)))
        )
        .then(userTodoCollectionItems => _([].concat(...userTodoCollectionItems))
            .mapWaitAll(userTodo => _(userTodo)
                .mapWaitAll(todo => cache.tryGetCollectionAndItems(todo, 'tags', /tags/))))
        .then(() => root);
};

/**
 * Get the users that exist on a user tenant
 *
 * @param userTenantsCollection
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTenantUsers = (userTenantsCollection, options) => {
    return cache.getNamedCollectionAndItems(userTenantsCollection, 'users', /users/, options);
};

/**
 * Loads up a tenant to be copied
 *
 * @param {TenantRepresentation} tenant
 * @param {UtilOptions?} options
 * @returns {Promise<TenantRepresentation>}
 */
export const hydrateUserTenant = (tenant, options) => {
    return Promise.all([
        getTodosWithTagsOnTenantTodos(tenant.todos, options),
        getTenantUsers(tenant, options)])
        .then(() => tenant);

};

/***********************************
 *
 * Create single tenant updates
 * ============================
 */


const syncTenantStrategy = (tenantCollection, aTenant, strategies, options) =>
    sync.getResourceInCollection(tenantCollection, aTenant, strategies, options);

const syncUsersStrategy = (tenant, aTenant, strategies, options) =>
    sync.getNamedCollectionInNamedCollection(tenant, 'users', /users/, aTenant, strategies, options);

const syncTodosStrategy = (user, aUser, strategies, options) =>
    sync.getNamedCollectionInNamedCollection(user, 'todos', /todos/, aUser, strategies, options);

const syncTagsStrategy = (todo, aTodo, root, options) =>
    sync.getNamedCollectionInNamedCollection(todo, 'tags', /tags/, aTodo, [], options);


/**
 * Clone a graph of tenant todo lists on root
 *
 * Context: (api)-(me)-[tenants]
 * Access: [todos...]-[todos...]-[tags]
 * Pool: (api)-[tags]
 *
 * @param {ApiRepresentation} apiResource
 * @param {TenantRepresentation} aTenant
 * @param {UtilOptions?} options
 * @returns {Promise<TenantCollectionRepresentation | never>}
 */
export const createTenant = (apiResource, aTenant, options) => {

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
                    /*
                                        (tenantRepresentation, tenantDocument, options) => syncUsersStrategy(
                                            tenantRepresentation,
                                            tenantDocument,
                                            [],
                                            options),
                    */
                    (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                        usersRepresentation,
                        usersDocument,
                        [
                            (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                                usersRepresentation,
                                usersDocument,
                                [
                                    (todoRepresentation, todoDocument, options) =>
                                        syncTagsStrategy(todoRepresentation, todoDocument, apiResource, options)

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

/***********************************
 *
 * Single tenant updates
 * =====================
 */

/**
 * @deprecated
 */
const syncTenant = (tenantRepresentation, aTenant, root, options) =>
    sync.getResource(
        tenantRepresentation,
        aTenant,
        [
            (tenantRepresentation, tenantDocument, options) => syncUsersStrategy(
                tenantRepresentation,
                tenantDocument,
                [
                    (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                        usersRepresentation,
                        usersDocument,
                        [
                            (todoRepresentation, todoDocument, options) => syncTagsStrategy(todoRepresentation, todoDocument, root, options)
                        ],
                        options)
                ],
                options)
        ],
        options);

/**
 * Takes a tenant and updates on the collection
 *
 * @param {CollectionRepresentation} tenant
 * @param {LinkedRepresentation} aTenant
 * @param {ApiRepresentation} root
 * @param {UtilOptions?} options
 * @return {Promise|*|{x, links}}
 */
export const createOrUpdateUsersOnTenant = (tenant, aTenant, root, options) => {

    const tenantRepresentation = findResourceInCollection(tenant, aTenant, 'self');

    log.debug(`Update tenant ${tenantRepresentation.name} --> ${getUri(tenantRepresentation, 'self')}`);

    return syncTenant(tenantRepresentation, aTenant, root, options);
};
