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
        .then(apiResource => cache.getNamedCollection(apiResource, 'tenants', /tenants/))
        .then(tenants => cache.getCollectionItemByUri(tenants, tenantUri));
};

/**
 * use the organisation from a provided list (when authenticated)
 *
 * Warning: this design has a flaw. Switching between tenant todos will conflict
 *
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TodoCollectionRepresentation>}
 */
const getTodos = (apiResource, tenantUri) => {
    log.debug(`Looking for todos in tenant ${tenantUri}`);

    return Promise.all([cache.getSingleton(apiResource, 'me', /me/), getTenant(apiResource, tenantUri)])
    // note this usese 'code' to select the title on the link relation
        .then(([me, tenant]) => cache.getNamedCollectionByTitle(me, 'todos', /todos/, tenant.code));
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

export {getTenant, getTodos, defaultTodo};