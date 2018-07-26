import {nodMaker, _} from 'semanticLink';

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
