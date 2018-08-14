import Bottleneck from 'bottleneck';
import axios from 'axios';
import {log} from 'logger';

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

        this.requests = {};

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
    schedule(params) {
        return this._limiter.schedule(params);
    }

    /**
     * This method wraps the limiter scheduler because it cannot deal with multiple requests at the same time on
     * the same 'id'. This queues up subsequent requests and then resolves them upon the original request.
     *
     * Note: this is a naive implementation of queue clearing.
     *
     * TODO: cancelled promises need to be cleared out of this queue too
     *
     * @param {AxiosRequestConfig} config
     * @return {Promise<AxiosResponse>}
     */
    request(config) {

        const id = config.url;

        if (!this.requests[id]) {

            const p = new Promise((resolve, reject) => {

                this.schedule({id: config.url}, config)
                    .then(result => {
                        log.debug(`[RequestResolver] resolved '${id}' (${this.requests[id].promises.length} subsequent requests)`);

                        // resolving with chain through to the subsequent requests
                        resolve(result);

                        // clean up the requests
                        delete this.requests[id];
                    })
                    .catch(reject);
            });

            this.requests[id] = {
                request: p,
                promises: []
            };

            log.debug(`[RequestResolver] add key '${id}'`);
            return p;
        } else {

            // construct an array of promises that will be resolved with the original request value
            const p = new Promise((resolve, reject) => {
                this.requests[id].request
                    .then(resolve)
                    .catch(reject);
            });
            this.requests[id].promises.push(p);
            log.debug(`[RequestResolver] queued '${id}' (${this.requests[id].promises.length} in queue)`);
            return p;
        }

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