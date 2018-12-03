import {cache} from 'semantic-link-cache';
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

    return cache.getSingleton(apiResource, 'me', /me/, options)
        .then(user => cache.getNamedCollection(user, 'todos', /todos/, options));
};

/**
 * Get the todos on the todo list
 * @param {TodoCollectionRepresentation} todoCollection
 * @param {UtilOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>}
 */
export const getTodos = (todoCollection, options) => {

    log.debug(`Looking for todos on list ${link.getUri(todoCollection, 'self')}`);

    return cache.getNamedCollectionAndItems(todoCollection, 'todos', /todos/, options);
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

    return cache.tryGetNamedCollectionAndItemsOnCollectionItems(userTenantsCollection, 'todos', /todos/, options)
        .then(todosCollection => mapWaitAll(todosCollection, item =>
            cache.tryGetNamedCollectionOnCollectionItems(item, 'tags', /tags/, options)));
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
        .then(itemResource => cache.getNamedCollectionAndItems(itemResource, 'todos', /todos/, options));
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
    return cache
        .getResource(todoResource)
        .then(todoCollection => cache.getSingleton(todoCollection, 'createForm', /create-form/))
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