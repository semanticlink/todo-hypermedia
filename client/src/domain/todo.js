import {get, link} from 'semantic-network';
import {log} from 'logger';
import {FieldType} from 'semantic-network/interfaces';

/**
 * Get the first level of todos (regardless of tenants)
 *
 * @param {ApiRepresentation} apiResource
 * @param {CacheOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>} sparsely populated
 */
export const getTodoList = (apiResource, options) => {

    log.debug('Looking for todos on me');

    return get(apiResource, /me/, options)
        .then(user => get(user, /todos/, options));
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
    // TODO: why is batchSize not down in the next query?
    return get(userTenantsCollection, {rel: /todos/, includeItems: true, batchSize: 1, ...options})
        .then(userTenantsTodos => get(
            userTenantsTodos,
            {
                ...options,
                iterateOver: true,
                rel: /tags/,
                includeItems: true,
            }));
};

/**
 * Get the todo list items based on a uri starting from the root
 *
 * Context: (api)
 * Looks for: -(me)-[todos...{self:$todoUri}]-[todos...]
 *
 * @param {ApiRepresentation} apiResource
 * @param {Uri} todoUri
 * @param {CacheOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>}
 */
export const getTodoListAndItemsByUri = (apiResource, todoUri, options) => {

    return getTodoListByUri(apiResource, todoUri, {...options, includeItems: false})
        .then(itemResource => get(itemResource, {...options, rel: /todos/}));
};

export const getTodos = (todoCollection, options) => {
    return get(todoCollection, {...options, includeItems: true});
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
export const getTodoListByUri = (apiResource, todoUri, options) => {
    return getTodoList(apiResource, options)
        .then(todosList => get(todosList, {...options, where: todoUri}));
};

/**
 * Creates a new in-memory object based on a form.
 *
 * This is a simple implementation that:
 *  - doesn't support types (everyone is simple a null
 *  - doesn't support default values (because forms don't have that yet)
 *
 * @param {TodoCollectionRepresentation} todoResource
 * @returns {Promise<TodoRepresentation>}
 */
export const defaultTodo = todoResource => {
    return get(todoResource)
        .then(todoCollection => get(todoCollection, /create-form/))
        .catch(() => log.error(`No create form for on '${link.getUri(todoResource, /self/)}'`))
        .then(/** @type {FormRepresentation} */form => {
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
        .catch(log.error);
};