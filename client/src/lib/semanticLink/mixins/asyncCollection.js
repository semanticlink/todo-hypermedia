import {normalise} from './collection';


/**
 * Concurrent support for waiting for one or more promises. It is likely that you will inline using Promise.all.
 * This was originally created for underscore chaining
 *
 * Note:
 *
 * The `Promise.all` underlying method returns a single Promise that resolves when all
 * of the promises in the iterable argument have resolved or when the iterable argument contains no promises.
 *
 * It rejects with the reason of the first promise that rejects.
 *
 * @example
 *
 *   return _(aSeries.items)
 *        .chain()
 *        .spliceAll(_(response.data.items).map(function(seriesValueFeedItem) {
 *              return makeBaseResourceFromFeedItem(seriesValueFeedItem);
 *        }))
 *        .map(function(aSeriesValue) {
 *              return link
 *                  .get(aSeriesValue, 'self')
 *                  .then(function(seriesValueResponse) {
 *                      var seriesValues = seriesValueResponse.data;
 *
 *                      highchart.updateSeries(
 *                          aSeries,
 *                          chartRepresentation.config.series,
 *                          seriesValues,
 *                          chartRepresentation.xAxes
 *                      );
 *
 *                      return updateResourceFromRepresentation(aSeriesValue, seriesValueResponse.data);
 *                  });
 *          })
 *        .waitAll()
 *        .value();
 *
 * @alias concurrentWait
 * @param {Promise[]|Promise} [promises]
 * @return {Promise}
 */
export const waitAll = promises => Promise.all(normalise(promises));


/**
 * Concurrent support performing an action on a collection and waiting for them to complete.
 *
 * Note:
 *
 * The `Promise.all` underlying method returns a single Promise that resolves when all
 * of the promises in the iterable argument have resolved or when the iterable argument contains no promises.
 *
 * It rejects with the reason of the first promise that rejects.
 *
 * @param {CollectionRepresentation|*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise<Array<T>>}
 */
export const mapWaitAll = (collection, iterator, context) => Promise.all(normalise(collection).map(iterator, context));

/**
 * Concurrent support performing an action on a resource attribute and waiting for them to complete. Optionally,
 * the attribute can be morphed at the same time.

 * Note:
 *
 * The `Promise.all` underlying method returns a single Promise that resolves when all
 * of the promises in the iterable argument have resolved or when the iterable argument contains no promises.
 *
 * It rejects with the reason of the first promise that rejects.
 *
 * @example
 *
 *  const doc = mapAttributeWaitAll(
 *      resource,
 *      (field, fieldname) =>  // some promise call,
 *      dashToCamel
 *    );
 *
 * @param resource
 * @param callbackFunction
 * @param keyReplacer
 * @return {Promise<any>}
 */
export const mapAttributeWaitAll = (resource, callbackFunction, keyReplacer) => {

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
 * Concurrent support performing an action on a collection and waiting for them to complete.
 *
 * Note:
 *
 * The `Promise.all` underlying method returns a single Promise that resolves when all
 * of the promises in the iterable argument have resolved or when the iterable argument contains no promises.
 *
 * It rejects with the reason of the first promise that rejects.
 *
 * @param {*[]|*|function():Promise} collection
 * @return {Promise}
 */
export const flattenWaitAll = collection => Promise.all(
    [].concat(...normalise(collection)));

/**
 * Concurrent support performing mapping a T[] to a S[][], flattening to S[] and waiting for them to complete.
 *
 * Note:
 *
 * The `Promise.all` underlying method returns a single Promise that resolves when all
 * of the promises in the iterable argument have resolved or when the iterable argument contains no promises.
 *
 * It rejects with the reason of the first promise that rejects.
 *
 * @param {*[]|*|function():Promise} collection
 * @param {function():Promise=} iterator
 * @param {*=} context
 * @return {Promise<Array>}
 */
export const mapFlattenWaitAll = (collection, iterator, context) => Promise.all(
    [].concat(...normalise(collection).map(iterator, context)));


/**
 * Sequential support for waiting for one or more promises based on collections items iterator
 * that returns a Promise. This is like Promise.all(), however, this implementation
 * will execute (call) the items sequentially.  The result of the last item is available
 * to the next item.
 *
 * In practice, this is used in return Promise<void> situations because you are just processing
 * an item one at a time.
 *
 * @alias wait
 * @param {Promise<T>[]} collection
 * @param {function(*, *):{Promise}} promise
 * @return {Promise<undefined|*>}
 */
export const sequentialWait = (collection, promise, context = {}) =>
    normalise(collection)
        .reduce(
            (acc, item) => acc.then(result => promise(result, item)),
            Promise.resolve(context || null));

/**
 * Sequential support for waiting for one or more promises based on collections items iterator
 * that returns a Promise. This is like Promise.all(), however, this implementation
 * will execute (call) the items sequentially.  All results are returned in an array
 * in order of execution
 *
 * @alias mapWait
 * @param promises
 * @return {Promise<Array<T>>}
 */
export const sequentialMapWait = (...promises) => {
    const results = [];

    const merged = promises.reduce(
        (acc, p) => acc.then(() => p).then(r => results.push(r)),
        Promise.resolve(null));

    return merged.then(() => results);
};

export const AsyncCollectionMixins = {
    waitAll,
    mapWaitAll,
    mapAttributeWaitAll,
    flattenWaitAll,
    mapFlattenWaitAll,
    sequentialWait,
    sequentialMapWait
};