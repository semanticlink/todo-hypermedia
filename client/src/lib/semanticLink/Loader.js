import Bottleneck from 'bottleneck';
import axios from 'axios';
import log from './Logger';

const CancelToken = axios.CancelToken;
const source = CancelToken.source();

/**
 * @class LoaderOptions
 * @properties {number} maxConcurrent=5
 * @properties {number} minTime=0
 * @properties {number} highWater=-1
 */

/**
 * Loading service to allow for rate limiting concurrent requests and
 * being able to cancel some or all requests.
 *
 * Wraps bottleneck and axios cancellables in the background.
 *
 * @class Loader
 */
export default class Loader {

    /**
     * @param {LoaderOptions=} options
     */
    constructor (options) {

        this._currentOptions = options;

        this._limiter = Loader.limiterFactory(options);

        this.schedule = this._limiter.schedule;

        const cancellable = axios.CancelToken;
        this._cancelleable = source.token;
        this._cancel = () => cancellable.cancel;

        this._limiter.on('error', error => {
            log.error(`[Limiter] Error: ${error}`);
        });

        this._limiter.on('debug', (message) => {
            log.debug(`[Limiter] ${message}`);
        });
    }

    static limiterFactory (options) {
        log.debug('[Limiter] Create');
        return new Bottleneck(Object.assign({}, options, Loader.defaultOptions));
    }

    /**
     * @returns {LoaderOptions}
     */
    static get defaultOptions () {
        return {
            // num of jobs that can be running at the same time
            maxConcurrent: 5,
            // immediately launch the next job
            minTime: 0,
            // default: how long can the queue get? At this stage unlimited
            highWater: null,
            // this is actaully the default
            strategy: Bottleneck.strategy.LEAK,
            // use es6 promise over the default Bluebird
            Promise: Promise
        };
    }

    get limiter () {
        return this._limiter;
    }

    get queued () {
        return this._limiter.nbQueued();
    }

    get cancellable () {
        return this._cancelleable;
    }

    clearAll () {
        // stop shuts everything down
        this._limiter.stopAll();
        // this will abort any xhr requests
        this._cancel();
        // unfortunately, we still need one! TODO: ask library for update to be able to clear queues and keep running
        this._limiter = Loader.limiterFactory(this._currentOptions);
    }

    onEmpty (callback) {
        this._limiter.on('empty', callback);
    }

}

export let loader = new Loader();