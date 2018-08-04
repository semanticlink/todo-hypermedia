import Bottleneck from 'bottleneck';
import axios from 'axios';
import log from './Logger';

const CancelToken = axios.CancelToken;
const source = CancelToken.source();

/**
 * @class LoaderOptions
 * @extends {module:bottleneck.ConstructorOptions}
 * @properties {number} maxConcurrent=5
 * @properties {number} minTime=0
 * @properties {number} highWater=-1
 */

/**
 * Loading service to allow for rate limiting and prioritising concurrent requests and
 * being able to cancel some or all requests.
 *
 * Wraps bottleneck and axios cancellables in the background using es6 promises.
 *
 */
export default class Loader {

    /**
     * @param {module:bottleneck.ConstructorOptions=} options
     */
    constructor(options) {

        this._currentOptions = options;

        this._limiter = Loader.limiterFactory(options);

        /**
         *
         * @type {CancelTokenStatic}
         */
        const cancelToken = axios.CancelToken;
        this._cancelleable = source.token;
        this._cancel = () => cancelToken.cancel;

        this._limiter.on(Loader.event.ERROR, error => {
            log.error(`[Limiter] Error: ${error}`);
        });

        this._limiter.on(Loader.event.DEBUG, (message) => {
            log.debug(`[Limiter] ${message}`);
        });
    }

    /**
     * Make a new limiter with the options
     * @param {module:bottleneck.ConstructorOptions} options
     * @return {module:bottleneck.Bottleneck}
     */
    static limiterFactory(options) {
        log.debug('[Limiter] Created');
        return new Bottleneck({...options, ...Loader.defaultOptions});
    }

    /**
     * @returns {module:bottleneck.ConstructorOptions}
     */
    static get defaultOptions() {
        return {
            // num of jobs that can be running at the same time
            maxConcurrent: 5,
            // immediately launch the next job
            minTime: 0,
            // default: how long can the queue get? At this stage unlimited
            highWater: null,
            // this is actually the default
            strategy: Bottleneck.strategy.LEAK,
            // use es6 promise over the default Bluebird
            Promise: Promise
        };
    }


    /**
     * @see {@link Bottleneck.on}
     * @return {{EMPTY: string, IDLE: string, DROPPED: string, DEPLETED: string, DEBUG: string, ERROR: string}}
     */
    static get event() {
        return {
            EMPTY: 'empty',
            IDLE: 'idle',
            DROPPED: 'dropped',
            DEPLETED: 'depleted',
            DEBUG: 'debug',
            ERROR: 'error',
        };
    }

    /**
     * Access to the limiter. Chain the methods of this instance if you require it
     *
     * @example loader.limiter.on(loader.event.DEBUG, () => {});
     * @example itemsInQueue = loader.limiter.queued();
     * @example loader.limiter.schedule( ...
     * @return {Bottleneck}
     */
    get limiter() {
        return this._limiter;
    }

    /**
     * Access to the cancel token
     * @return {CancelToken | *}
     */
    get cancellable() {
        return this._cancelleable;
    }

    /**
     * Current options in the limiter
     * @return {module:bottleneck.ConstructorOptions|*}
     */
    get currentOptions() {
        return this._currentOptions;
    }

    /**
     * Wrapper around the limiter schedule function
     *
     * @see Bottleneck.schedule
     * @param params
     * @return {*}
     */
    schedule(...params){
        return this._limiter.schedule(...params);
    }

    /**
     * Stop all current and pending requests and reset all queues.
     */
    clearAll() {
        // this will abort any xhr requests
        this._cancel();
        // unfortunately, we still need one! TODO: ask library for update to be able to clear queues and keep running
        this._limiter = Loader.limiterFactory(this._currentOptions);
    }

}

export let loader = new Loader();