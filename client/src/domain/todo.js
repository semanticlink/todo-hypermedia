import {log, nodMaker, SemanticLink} from 'semanticLink';
import {TEXT} from '../lib/form-type-mappings';

/**
 * use the organisation from a provided list (when authenticated)
 * @param {ApiRepresentation} apiResource
 * @param {string} tenantUri
 * @returns {Promise<TenantRepresentation>}
 */
const getTenant = (apiResource, tenantUri) => {
    return nodMaker
        .getResource(apiResource)
        .then(apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/))
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

    return nodMaker.getSingletonResource(apiResource, 'me', /me/)
        .then(me => nodMaker.getNamedCollectionResourceAndItems(me, 'todos', /todos/));
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
    return nodMaker
        .getResource(todoResource)
        .then(todoCollection => nodMaker.getSingletonResource(todoCollection, 'createForm', /create-form/))
        .catch(() => log.error(`No create form for on '${SemanticLink.getUri(todoResource, /self/)}'`))
        .then(form => {
            const obj = {};

            if (form && form.items) {
                [...form.items].forEach(item => {
                    obj[item.name] = item.type === TEXT ? '' : null;
                });
            } else {
                log.warn(`Form has no fields: '${SemanticLink.getUri(form, /self/)}'`);
            }

            return obj;
        })
        .catch((err) => log.error(err));
};

export {getTenant, getTodos, defaultTodo};