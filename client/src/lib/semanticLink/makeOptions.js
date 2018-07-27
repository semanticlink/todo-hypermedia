import _ from './mixins/underscore';

/**
 * wrapper to lessen syntax
 * @param options
 * @param obj
 * @return {{}}
 */
export const makeOptions = (options, obj) => {
    return {...options, ...obj};
};
export default makeOptions;

/**
 *
 * @param {UtilOptions} options
 * @param {int} size
 * @return {*}
 */
export const makeOptionsWithChildBatchSize = (options, size) =>
    makeOptions(options, {childStrategyBatchSize: size});
/**
 *
 * @param {UtilOptions} options
 * @param {int} size
 * @return {*}
 */
export const makeOptionsWithBatchSize = (options, size) =>
    makeOptions(options, {batchSize: size});

/**
 * Make options where the resource in the collection will be searched by only on link relation (default: canonical|self)
 *
 * @param {UtilOptions} options
 * @param collection
 * @param resource
 * @param rel
 * @return {UtilOptions}
 */
export const makeOptionsWithFindResourceInCollectionByLinkRelOnly = (options, collection, resource, rel) =>
    makeOptions(options, {
        findResourceInCollectionStrategy: (collection, resource, rel) => _(collection).findResourceInCollectionByUri(resource, rel)
    });

/**
 * Make options where the resource in the collection will be searched by link relation or name attribute
 *
 * @param {UtilOptions} options
 * @param collection
 * @param resource
 * @param rel
 * @return {UtilOptions}
 */
export const makeOptionsWithFindResourceInCollection = (options, collection, resource, rel) =>
    makeOptions(options, {
        findResourceInCollectionStrategy: (collection, resource, rel) => _(collection).findResourceInCollection(resource, rel)
    });

/**
 * Make options where the resource uses a 'title' (rather than a 'name' and 'description'
 * style of identification).
 *
 * In general resources are moving towards a name and description, rather than just a title.
 *
 * @param {UtilOptions} options the options to extend
 * @return {UtilOptions}
 */
export const makeOptionsWithMappingToTitle = options =>
    makeOptions(options, {mappedTitle: 'title'});

/**
 * Make options where the resource uses a 'name' (rather than a 'name' and 'description'
 * style of identification).
 *
 * This options is the default but will need to be used when synchronising a tree and an ancester (parent)
 * has set the mappedTitle to something else.
 *
 * @param {UtilOptions} options the options to extend
 * @return {UtilOptions}
 */
export const makeOptionsWithMappingToName = options =>
    makeOptions(options, {mappedTitle: 'name'});

/**
 * Make options where the resource uses an empty 'name'
 *
 * This option is need for resources that have no title such as virtual resource
 *
 * @param {UtilOptions} options the options to extend
 * @return {UtilOptions}
 */
export const makeOptionsWithEmptyMapping = options =>
    makeOptions(options, {mappedTitle: ''});

/**
 * Make options where the resource will be force created on provisioning
 *
 * @param {UtilOptions} options the options to extend
 * @param {boolean=true} defaultValue
 * @return {UtilOptions}
 */
export const makeOptionsWithForceCreate = (options, defaultValue = true) =>
    makeOptions(options, {forceCreate: defaultValue});

/**
 * Make options for messages that need to be displayed to the user
 *
 * @param {UtilOptions} options the options to extend
 * @param {{success:string,error:string}} messages
 * @return {UtilOptions}
 */
export const makeOptionsWithMessages = (options, messages) =>
    makeOptions(options, messages);

/**
 * Make options for messages that need to be displayed to the user
 * @param {UtilOptions} options
 * @param {string} success
 * @param {string} error
 * @return {UtilOptions}
 */
export const makeOptionsWithStandardMessages = (options, success = '', error = '') =>
    makeOptionsWithMessages(options, {success, error});

const capitalise = (str) => {
    return str.charAt(0).toUpperCase() + this.slice(1);
};
const lowercase = (str) => {
    return str.charAt(0).toLowerCase() + this.slice(1);
};

/**
 * Make options fpr message that need to  be displayed to the user
 * @param options
 * @param {LinkedRepresentation} resource
 * @param {string} type - the stringly type of the resource that would make sense to a user
 * @param {string} action the type of action [create, delete, update]
 * @return {UtilOptions}
 */
export const makeOptionsWithResourceMessages = (options, resource, type, action) =>
    makeOptionsWithMessages(options, {
        success: `${capitalise(type)} ${lowercase(action)}d: '${resource.name}'`,
        error: `Unable to ${lowercase(action)} ${lowercase(type)} '${resource.name}'`
    });

/**
 * Make options fpr message that need to  be displayed to the user
 * @param options
 * @param {LinkedRepresentation} resource
 * @param {string} type - the stringly type of the resource that would make sense to a user
 */
export const makeOptionsWithCreateMessages = (options, resource, type) =>
    makeOptionsWithResourceMessages(options, resource, type, 'create');

/**
 * Make options fpr message that need to  be displayed to the user
 * @param options
 * @param {LinkedRepresentation} resource
 * @param {string} type - the stringly type of the resource that would make sense to a user
 */
export const makeOptionsWithUpdateMessages = (options, resource, type) =>
    makeOptionsWithResourceMessages(options, resource, type, 'update');

/**
 * Make options fpr message that need to  be displayed to the user
 * @param options
 * @param {LinkedRepresentation} resource
 * @param {string} type - the stringly type of the resource that would make sense to a user
 */
export const makeOptionsWithDeleteMessages = (options, resource, type) =>
    makeOptionsWithResourceMessages(options, resource, type, 'delete');
