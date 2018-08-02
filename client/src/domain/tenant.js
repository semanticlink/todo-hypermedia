import {nodMaker, _} from 'semanticLink';
import {getUri} from 'semantic-link';
import {nodSynchroniser} from 'semanticLink/NODSynchroniser';
import {log} from 'logger';
import {pooledTagResourceResolver} from '../domain/tags';
import {findResourceInCollection} from 'semanticLink/mixins/collection';
import {uriMappingResolver} from 'semanticLink/UriMappingResolver';


export const getTenants = root => nodMaker.getNamedCollectionResource(root, 'tenants', /tenants/);

const getTags = root => nodMaker.tryGetCollectionResourceAndItems(root, 'tags', /tags/);

export const getTenantAndTodos = root => {
    return Promise.all([getTenants(root), getTags(root)])
        .then(([tenantCollection]) => nodMaker.tryGetNamedCollectionResourceAndItemsOnCollectionItems(tenantCollection, 'users', /users/))
        .then(tenantCollectionItems => _(tenantCollectionItems)
            .mapWaitAll(tenant => _(tenant)
                .mapFlattenWaitAll(user => nodMaker.tryGetCollectionResourceAndItems(user, 'todos', /todos/)))
        )
        .then(userTodoCollectionItems => _([].concat(...userTodoCollectionItems))
            .mapWaitAll(userTodo => _(userTodo)
                .mapWaitAll(todo => nodMaker.tryGetCollectionResourceAndItems(todo, 'tags', /tags/))))
        .then(() => root);
};


const syncUsersStrategy = (tenant, aTenant, strategies, options) => {
    return nodSynchroniser.getNamedCollection(tenant, 'users', /users/, aTenant, strategies, options);
};

const syncTodosStrategy = (user, aUser, strategies, options) => {
    return nodSynchroniser.getNamedCollection(user, 'todos', /todos/, aUser, strategies, options);
};

const syncTagsStrategy = (todo, aTodo, strategies, options) => {
    return nodSynchroniser.patchUriListOnNamedCollection(
        todo,
        'tags',
        /tags/,
        aTodo.tags.items.map(item => getUri(item, 'self')),
        {
            ...options,
            ...{contributeonly: true},
            ...pooledTagResourceResolver(root)
        });
};


function syncTenant(tenantRepresentation, aTenant, options) {
    return nodSynchroniser.getResource(
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
                            (todoRepresentation, todoDocument, options) => syncTagsStrategy(todoRepresentation, todoDocument, [], options)
                        ],
                        options)
                ],
                options)
        ],
        options);
}

/**
 * Takes a tenant and updates on the collection
 *
 * @param {CollectionRepresentation} tenant
 * @param {LinkedRepresentation} aTenant
 * @param {ApiRepresentation} root
 * @param options
 * @return {Promise|*|{x, links}}
 */
export const createOrUpdateUsersOnTenant = (tenant, aTenant, root, options) => {

    const tenantRepresentation = findResourceInCollection(tenant, aTenant, 'self');

    log.debug(`Update tenant ${tenantRepresentation.name} --> ${getUri(tenantRepresentation, 'self')}`);

    return syncTenant(tenantRepresentation, aTenant, options);
};

const syncTenantStrategy = (tenantCollection, aTenant, strategies, options) => {
    return nodSynchroniser.getResourceInCollection(tenantCollection, aTenant, strategies, options);
};

export const createTenantOnRoot = (root, aTenant, options) => {

    if (!aTenant) {
        throw new Error('Tenant is empty');
    }

    log.debug('[Tenant] start create on root');

    return getTenants(root)
        .then(tenantCollection => {
            log.debug('[Tenant] root loaded');
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
                                    (todoRepresentation, todoDocument, options) => syncTagsStrategy(todoRepresentation, todoDocument, [], options)
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