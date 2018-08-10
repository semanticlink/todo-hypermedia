import {cache} from 'semantic-link-cache';
import * as link from 'semantic-link';
import {TEXT} from 'semantic-link-utils/form-type-mappings';
import {log} from 'logger';

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TenantRepresentation>}
 */
const getTenant = (apiResource, tenantUri) => {
    return cache
        .getResource(apiResource)
        .then(apiResource => cache.getNamedCollectionResource(apiResource, 'tenants', /tenants/))
        .then(tenants => cache.getCollectionResourceItemByUri(tenants, tenantUri));
};

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TodoCollectionRepresentation>}
 */
const getTodos = (apiResource, tenantUri) => {
    log.debug(`Looking for todos in tenant ${tenantUri}`);

    return cache.getSingletonResource(apiResource, 'me', /me/)
        .then(me => cache.getNamedCollectionResourceAndItems(me, 'todos', /todos/));
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
const defaultTodo = todoResource => {
    return cache
        .getResource(todoResource)
        .then(todoCollection => cache.getSingletonResource(todoCollection, 'createForm', /create-form/))
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

export {getTenant, getTodos, defaultTodo};