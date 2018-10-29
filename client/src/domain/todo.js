import {cache} from 'semantic-link-cache';
import * as link from 'semantic-link';
import {TEXT} from 'semantic-link-utils/form-type-mappings';
import {log} from 'logger';
import {findResourceInCollectionByUri} from 'semantic-link-cache/mixins/collection';

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TenantRepresentation>}
 */
export const getTenant = (apiResource, tenantUri) => {
    return cache
        .getResource(apiResource)
        .then(apiResource => cache.getNamedCollection(apiResource, 'tenants', /tenants/))
        .then(tenants => cache.getCollectionItemByUri(tenants, tenantUri));
};
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
 * @param todoListResource
 * @param {UtilOptions?} options
 * @returns {Promise<TodoCollectionRepresentation>}
 */
export const getTodos = (todoListResource, options) => {

    log.debug(`Looking for todos on list ${link.getUri(todoListResource, 'self')}`);

    return cache.getNamedCollectionAndItems(todoListResource, 'todos', /todos/, options);
};


/**
 * Get the todolist based on a uri starting from the root
 *
 * Looks for: (api)-(me)-[todos...{self:$todoUri}]-[todos...]
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} todoUri
 * @param {UtilOptions?} options
 * @returns {Promise<CollectionRepresentation>}
 */
export const getTodoListByUri = (apiResource, todoUri, options) => {

    return getTodoList(apiResource, options)
        .then(todosList => {

            const itemResource = findResourceInCollectionByUri(todosList, todoUri);

            return cache.getNamedCollectionAndItems(itemResource, 'todos', /todos/, options);
        });
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