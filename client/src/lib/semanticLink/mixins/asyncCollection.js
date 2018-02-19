import _ from 'underscore';

/**
 * Takes a resource and normalises it to a valid iterable. Particularly if you hand in a resource that isn't a
 * collection then it will return an empty iterable (array).
 * @param {*|[]|CollectionRepresentation} objOrCollection takes a collection and returns the items if available
 * @return {*}
 */
const normalise = objOrCollection => {
    if (!objOrCollection) {
        return [];
    } else if (_(objOrCollection).isArray()) {
        return objOrCollection;
    } else if (objOrCollection.items) {
        return objOrCollection.items;
    } else {
        return [];
    }
};

/**
 * A helper to determine if an object is a promise. WARNING: This will return
 * false if a deferred style object is provided.
 *
 * @param {*=} obj
 * @return {boolean}
 */
const isPromiseLike = obj => obj && _(obj.then).isFunction();

const reduceWaitAll = (collection, iterator, context = {}) =>
    _(normalise(collection))
        .reduce(
            (promise, item) => promise.then(result => iterator(result, item)),
            Promise.resolve(context)
        );

/**
 * Support for waiting for 1 or more angular promises.
 *
 * @param {Promise[]|Promise} [collection]
 * @return {Promise}
 */
const waitAll = collection => Promise.all(normalise(collection));

/**
 * Support performing an action on a collection and waiting for them to complete.
 *
 * @param {CollectionRepresentation|*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise}
 */
const mapWaitAll = (collection, iterator, context) => Promise.all(_(normalise(collection)).map(iterator, context));

const mapObjectWaitAll = (resource, callbackFunction, keyReplacer) => {

    keyReplacer = keyReplacer || (val => val);
    const keys = _(resource).keys();

    let document = {};

    return _(keys)
        .mapWaitAll(key => callbackFunction(resource[key], key)
            .then(value => {
                document[keyReplacer(key)] = value;
                return resource;
            }))
        .then(() => document);

};

/**
 * Support performing an action on a collection and waiting for them to complete.
 *
 * @param {*[]|*|function():Promise} collection
 * @return {Promise}
 */
const flattenWaitAll = collection => Promise.all(_(normalise(collection)).flatten());

/**
 * Support performing mapping a T[] to a S[][], flattening to S[] and waiting for them to complete.
 *
 * @param {*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise}
 */
const mapFlattenWaitAll = (collection, iterator, context) => Promise.all(
    _(normalise(collection))
        .chain()
        .map(iterator, context)
        .flatten()
        .value()
);

/**
 * This is like Promise.all(), however this implementation will
 * execute (call) the items sequentially.  The result of the last
 * item is available to the next item.
 *
 * @param {*[]|*} collection
 * @param {function(*, *):{Promise}} callbackFunction
 * @return {Promise}
 */
const sequentialWaitAll = (collection, callbackFunction) =>
    _(normalise(collection))
        .reduce((promise, item) =>
            promise.then(result => callbackFunction(result, item)), Promise.resolve(null)
        );

/**
 * @mixin
 */
export const AsyncCollectionMixins = {
    isPromiseLike,
    reduceWaitAll,
    waitAll,
    mapWaitAll,
    mapObjectWaitAll,
    flattenWaitAll,
    mapFlattenWaitAll,
    sequentialWaitAll,

};
_.mixin(AsyncCollectionMixins);

/**
 * @mixes {AsyncCollectionMixins}
 */
export default _;
