import { nodMaker } from 'semanticLink';
import { log } from 'logger';

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TenantRepresentation>}
 */
const getTenant = (apiResource, tenantUri) => {
    return nodMaker
        .getResource(apiResource)
        .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /tenants/))
        .then(tenants => nodMaker.getCollectionResourceItemByUri(tenants, tenantUri));
};

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TodoCollectionRepresentation>}
 */
const getTodos = (apiResource, tenantUri) => {
    log.debug(`Looking for todos in tenant ${tenantUri}`);

    return getTenant(apiResource, tenantUri)
        .then(tenant => nodMaker.getNamedCollectionResourceAndItems(tenant, 'todos', /todos/));
};

/**
 * Default values for a todo item. This *could/should* be retrieved from a create-form on the collection
 * @type {TodoRepresentation}
 */
const DEFAULT_TODO = { name: '', completed: false };

/**
 * Creates a new in-memory object based on a form.
 *
 * This is a simple implementation that:
 *  - doesn't support types (everyone is simple a null
 *  - doesn't support default values (because forms don't have that yet)
 *
 * @param todoResource
 * @returns {Promise<any>}
 */
const defaultTodo = todoResource => {
    return nodMaker
        .getResource(todoResource)
        .then(todoCollection => nodMaker.getSingletonResource(todoCollection, 'createForm', /create-form/))
        .then(createForm => {
            const obj = {};
            createForm.items.forEach(item => obj[item] = null);
            return obj;
        });
}


export { getTenant, getTodos, DEFAULT_TODO, defaultTodo };