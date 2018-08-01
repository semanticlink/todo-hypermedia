import {nodMaker, _} from 'semanticLink';
import {getUri} from 'semantic-link';
import {nodSynchroniser} from 'semanticLink/NODSynchroniser';
import {log} from 'logger';
import {pooledTagResourceResolver} from '../domain/tags';
import {findResourceInCollection} from 'semanticLink/mixins/collection';


export const getTenants = apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/);

const getTags = apiResource => nodMaker.tryGetCollectionResourceAndItems(apiResource, 'tags', /tags/);

export const getTenantAndTodos = apiResource => {
    return Promise.all([getTenants(apiResource), getTags(apiResource)])
        .then(([tenantCollection]) => nodMaker.tryGetNamedCollectionResourceAndItemsOnCollectionItems(tenantCollection, 'users', /users/))
        .then(tenantCollectionItems => _(tenantCollectionItems)
            .mapWaitAll(tenant => _(tenant)
                .mapFlattenWaitAll(user => nodMaker.tryGetCollectionResourceAndItems(user, 'todos', /todos/)))
        )
        .then(userTodoCollectionItems => _([].concat(...userTodoCollectionItems))
            .mapWaitAll(userTodo => _(userTodo)
                .mapWaitAll(todo => nodMaker.tryGetCollectionResourceAndItems(todo, 'tags', /tags/))))
        .then(() => apiResource);
};

/**
 * Takes a tenant and updates on the collection
 *
 * @param {CollectionRepresentation} tenantCollection
 * @param {LinkedRepresentation} tenantDocument
 * @param {ApiRepresentation} apiResource
 * @param options
 * @return {Promise|*|{x, links}}
 */
export const createOrUpdateUsersOnTenant = (tenantCollection, tenantDocument, apiResource, options) => {

    const tenantRepresentation = findResourceInCollection(tenantCollection, tenantDocument, 'self');

    log.debug(`Update tenant ${tenantRepresentation.name} --> ${getUri(tenantRepresentation, 'self')}`);

    const syncUsersStrategy = (tenantRepresentation, tenantDocument, strategies, options) => {
        return nodSynchroniser.getNamedCollection(tenantRepresentation, 'users', /users/, tenantDocument, strategies, options);
    };

    const syncTodosStrategy = (userRepresentation, userDocument, strategies, options) => {
        return nodSynchroniser.getNamedCollection(userRepresentation, 'todos', /todos/, userDocument, strategies, options);
    };

    const syncTagsStrategy = (todoRepresentation, todoDocument, strategies, options) => {
        return nodSynchroniser.patchUriListOnNamedCollection(
            todoRepresentation,
            'tags',
            /tags/,
            todoDocument.tags.items.map(item => getUri(item, 'self')),
            {
                ...options,
                ...{contributeonly: true},
                ...pooledTagResourceResolver(apiResource)
            });
    };

    return nodSynchroniser.getResource(
        tenantRepresentation,
        tenantDocument,
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
};