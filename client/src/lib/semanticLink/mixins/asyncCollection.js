import {normalise} from './collection';

/**
 *
 * @param {CollectionRepresentation|*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {LinkedRepresentation|Promise<any>}
 */
export const reduceWaitAll = (collection, iterator, context = {}) =>
    normalise(collection).reduce(
        (promise, item) => promise.then(result => iterator(result, item)),
        Promise.resolve(context)
    );

/**
 * Support for waiting for 1 or more angular promises.
 *
 * @param {Promise[]|Promise} [collection]
 * @return {Promise}
 */
export const waitAll = collection => Promise.all(normalise(collection));

/**
 * Support performing an action on a collection and waiting for them to complete.
 *
 * @param {CollectionRepresentation|*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise}
 */
export const mapWaitAll = (collection, iterator, context) => Promise.all(normalise(collection).map(iterator, context));

/**
 *
 * @param resource
 * @param callbackFunction
 * @param keyReplacer
 * @return {Promise<any>}
 */
export const mapObjectWaitAll = (resource, callbackFunction, keyReplacer) => {

    keyReplacer = keyReplacer || (val => val);

    let document = {};

    return mapWaitAll(
        Object.keys(resource),
        key => callbackFunction(resource[key], key)
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
export const flattenWaitAll = collection => Promise.all(
    [].concat(...normalise(collection)));

/**
 * Support performing mapping a T[] to a S[][], flattening to S[] and waiting for them to complete.
 *
 * @param {*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise}
 */
export const mapFlattenWaitAll = (collection, iterator, context) => Promise.all(
    [].concat(...normalise(collection).map(iterator, context)));

/**
 * This is like Promise.all(), however this implementation will
 * execute (call) the items sequentially.  The result of the last
 * item is available to the next item.
 *
 * @param {*[]|*} collection
 * @param {function(*, *):{Promise}} callbackFunction
 * @return {Promise}
 */
export const sequentialWaitAll = (collection, callbackFunction) =>
    normalise(collection)
        .reduce((promise, item) =>
            promise.then(result => callbackFunction(result, item)), Promise.resolve(null));

export const AsyncCollectionMixins = {
    reduceWaitAll,
    waitAll,
    mapWaitAll,
    mapObjectWaitAll,
    flattenWaitAll,
    mapFlattenWaitAll,
    sequentialWaitAll,
};