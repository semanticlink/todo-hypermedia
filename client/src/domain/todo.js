import {get} from 'semantic-network';
import * as link from 'semantic-link';
import {log} from 'logger';
import {findResourceInCollectionByUri} from 'semantic-network/mixins/collection';
import {sequentialWaitAll} from 'semantic-network/mixins/asyncCollection';
import {FieldType} from 'semantic-network/interfaces';

/**
 * Get the first level of todos (regardless of tenants)
 *
 * @param {ApiRepresentation} apiResource
 * @param {CacheOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>} sparsely populated
 */
export const getTodoList = (apiResource, options) => {

    log.debug('Looking for todos on root');

    return get(apiResource, /me/, options)
        .then(user => get(user, /todos/, options));
};

/**
 * Get the todos on the todo list
 * @param {TodoCollectionRepresentation} todoCollection
 * @param {CacheOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>}
 */
export const getTodos = (todoCollection, options) => {

    log.debug(`Looking for todos on list ${link.getUri(todoCollection, 'self')}`);

    return get(todoCollection, /todos/, {includeItems: true, ...options});
};

/**
 *
 * Context: (user)-[todos...]
 * Looks for: -[todos...]+->[tags...]
 * @param {TenantCollectionRepresentation} userTenantsCollection
 * @param {CacheOptions?} options
 * @returns {Promise}
 */
export const getTodosWithTagsOnTenantTodos = (userTenantsCollection, options) => {
    return get(userTenantsCollection, {rel: /todos/, includeItems: true, batchSize: 1, ...options})
        .then(todosCollection =>
            sequentialWaitAll(todosCollection, (_, item) => get(item, {rel: /tags/, includeItems: true, ...options})));
};

/**
 * Get the todo list items based on a uri starting from the root
 *
 * Context: (api)
 * Looks for: -(me)-[todos...{self:$todoUri}]-[todos...]
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} todoUri
 * @param {CacheOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTodoListByUri = (apiResource, todoUri, options) => {

    return getNamedListByUri(apiResource, todoUri, options)
        .then(itemResource => get(itemResource, {rel: /todos/, includeItems: true, ...options}));
};

/**
 * Get the name todo list based on a uri starting from the root
 *
 * Context: (api)
 * Looks for: -(me)-[todos...{self:$todoUri}]
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} todoUri
 * @param {CacheOptions?} options
 * @returns {Promise<LinkedRepresentation>}
 */
export const getNamedListByUri = (apiResource, todoUri, options) => {
    return getTodoList(apiResource, options)
        .then(todosList => findResourceInCollectionByUri(todosList, todoUri));
};

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
export const defaultTodo = todoResource => {
    return get(todoResource)
        .then(todoCollection => get(todoCollection, /create-form/))
        .catch(() => log.error(`No create form for on '${link.getUri(todoResource, /self/)}'`))
        .then(/** type {FormRepresentation} */form => {
            const obj = {};

            if (form && form.items) {
                [...form.items].forEach(item => {
                    obj[item.name] = item.type === FieldType.Text ? '' : null;
                });
            } else {
                log.warn(`Form has no fields: '${link.getUri(form, /self/)}'`);
            }

            return obj;
        })
        .catch((err) => log.error(err));
};