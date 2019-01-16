import {query} from 'semantic-link-cache';
import * as link from 'semantic-link';
import {TEXT} from './form-type-mappings';
import {log} from 'logger';
import {findResourceInCollectionByUri} from 'semantic-link-cache/mixins/collection';
import {mapWaitAll} from 'semantic-link-cache/mixins/asyncCollection';

/**
 * Get the first level of todos (regardless of tenants)
 *
 * @param {ApiRepresentation} apiResource
 * @param {UtilOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>} sparsely populated
 */
export const getTodoList = (apiResource, options) => {

    log.debug('Looking for todos on root');

    return query.get(apiResource, /me/, options)
        .then(user => query.get(user, /todos/, options));
};

/**
 * Get the todos on the todo list
 * @param {TodoCollectionRepresentation} todoCollection
 * @param {UtilOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>}
 */
export const getTodos = (todoCollection, options) => {

    log.debug(`Looking for todos on list ${link.getUri(todoCollection, 'self')}`);

    return query.get(todoCollection, /todos/, {includeItems: true, ...options});
};

/**
 *
 * Context: (user)-[todos...]
 * Looks for: -[todos...]+->[tags...]
 * @param {TenantCollectionRepresentation} userTenantsCollection
 * @param {UtilOptions?} options
 * @returns {Promise}
 */
export const getTodosWithTagsOnTenantTodos = (userTenantsCollection, options) => {
    return query.get(userTenantsCollection, {rel: /todos/, includeItems: true, ...options})
    /*
      return cache.tryGetNamedCollectionAndItemsOnCollectionItems(userTenantsCollection, 'todos', /todos/, options)
    */
        .then(todosCollection => mapWaitAll(todosCollection, item =>
            query.get(item, {rel: /tags/, includeItems: true, ...options})));
};

/**
 * Get the todo list items based on a uri starting from the root
 *
 * Context: (api)
 * Looks for: -(me)-[todos...{self:$todoUri}]-[todos...]
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} todoUri
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTodoListByUri = (apiResource, todoUri, options) => {

    return getNamedListByUri(apiResource, todoUri, options)
        .then(itemResource => query.get(itemResource, {rel: /todos/, includeItems: true, ...options}));
};

/**
 * Get the name todo list based on a uri starting from the root
 *
 * Context: (api)
 * Looks for: -(me)-[todos...{self:$todoUri}]
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} todoUri
 * @param {UtilOptions?} options
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
    return query
        .get(todoResource)
        .then(todoCollection => query.get(todoCollection, /create-form/))
        .catch(() => log.error(`No create form for on '${link.getUri(todoResource, /self/)}'`))
        .then(form => {
            const obj = {};

            if (form && form.items) {
                [...form.items].forEach(item => {
                    obj[item.name] = item.type === TEXT ? '' : null;
                });
            } else {
                log.warn(`Form has no fields: '${link.getUri(form, /self/)}'`);
            }

            return obj;
        })
        .catch((err) => log.error(err));
};