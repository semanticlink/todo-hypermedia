import {_} from 'semantic-link-cache';
import {getUri} from 'semantic-link';
import {log} from 'logger';
import {pooledTagResourceResolver} from 'domain/tags';
import {findResourceInCollection} from 'semantic-link-cache/mixins/collection';
import {uriMappingResolver, sync, cache} from 'semantic-link-cache';


export const getTenants = root => cache.getNamedCollectionResource(root, 'tenants', /tenants/);

const getTags = root => cache.tryGetCollectionAndItems(root, 'tags', /tags/);

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


const syncUsersStrategy = (tenant, aTenant, strategies, options) =>
    sync.getNamedCollectionInNamedCollection(tenant, 'users', /users/, aTenant, strategies, options);

const syncTodosStrategy = (user, aUser, strategies, options) =>
    sync.getNamedCollectionInNamedCollection(user, 'todos', /todos/, aUser, strategies, options);

const syncTagsStrategy = (todo, aTodo, root, options) =>
    sync.getNamedCollectionInNamedCollection(todo, 'tags', /tags/, aTodo, [], options);

const syncTenantStrategy = (tenantCollection, aTenant, strategies, options) =>
    sync.getResourceInCollection(tenantCollection, aTenant, strategies, options);

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
 * @param {UtilOptions} options
 * @return {Promise|*|{x, links}}
 */
export const createOrUpdateUsersOnTenant = (tenant, aTenant, root, options) => {

    const tenantRepresentation = findResourceInCollection(tenant, aTenant, 'self');

    log.debug(`Update tenant ${tenantRepresentation.name} --> ${getUri(tenantRepresentation, 'self')}`);

    return syncTenant(tenantRepresentation, aTenant, root, options);
};

export const createTenantOnRoot = (root, aTenant, options) => {

    if (!aTenant) {
        throw new Error('Tenant is empty');
    }

    log.debug('[Tenant] start create on root');

    return getTenants(root)
        .then(tenantCollection => {
            log.debug(`[Tenant] root loaded ${getUri(tenantCollection, /self/)}`);
            return syncTenantStrategy(
                tenantCollection,
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
                                    (todoRepresentation, todoDocument, options) =>
                                        syncTagsStrategy(todoRepresentation, todoDocument, root, options)
                                ],
                                options)
                        ],
                        options)
                ],
                {
                    ...options,
                    ...pooledTagResourceResolver(root),
                    resolver: uriMappingResolver
                });
        });


};