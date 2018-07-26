import {nodMaker, link, _} from 'semanticLink';
import {log} from 'logger';
import SparseResource from 'semanticLink/SparseResource';

/**
 * Search for a tenant using the search form on the collection
 *
 * @param {TenantCollectionRepresentation} tenantCollection
 * @param tenantName
 * @returns {Promise.<CollectionRepresentation>} containing the search result collection
 */
export const searchForTenant = (tenantCollection, tenantName) => {

    if (!tenantName) {
        return Promise.resolve();
    }

    log.debug('Looking for search form on tenant collection');

    return nodMaker.getSingletonResource(tenantCollection, 'searchForm', /search/)
        .then(tenantSearchRepresentation => {

            let form = _(tenantSearchRepresentation.items).first();

            if (form) {

                /**
                 * Construct search based on the tenantSearchRepresentation. This is a simple form
                 * so we just grab the first item and haven't checks on type.
                 *
                 * In the future, we might ask for a field which is http://types/text
                 */
                const searchForm = {};
                searchForm[form.name] = tenantName;

                return link.post(tenantSearchRepresentation, /submit/, 'application/json', searchForm);
            } else {
                throw new Error(`Search form item does not exist on ${link.getUri(tenantCollection, /search/)}. Do you have the correct headers set?`);
            }
        })
        .then(createdResult => link.http.get(createdResult.headers.location))
        .then(searchResult => SparseResource.mapFeedItemsToCollectionItems(searchResult.data));
};

export const getTenants = apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/)

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
