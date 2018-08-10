import axios from 'axios';

/**
 * Currently, this is implemented using Axios and AxiosRequestConfig
 */
class HTTPQueue {

    constructor () {
        /**
         * Holds all the requests which failed due to net:ERR_CONNECTION_REFUSED response,
         * so they can be re-requested in future, once login is completed.
         * @type {Array.<AxiosRequestConfig, PromiseFactory>}
         */
        this.buffer = [];
    }

    /**
     * This callback type is called `PromiseFactory`.
     *
     * see Advanced mistake #3: promises vs promise factories
     *   https://pouchdb.com/2015/05/18/we-have-a-problem-with-promises.html
     *
     * @callback PromiseFactory
     * @return {Promise}
     */

    /**
     * Add a request config so that it can be retried later on.
     *
     * @param {AxiosRequestConfig} config request config (usually from a response.config)
     */
    pushToBuffer (config) {
        this.buffer.push(config);
    }

    /**
     *
     * @param {AxiosRequestConfig} config request config (usually from a response.config)
     * @returns {Promise.<T>}
     */
    retry (config) {
        return axios(config);
    }

    /**
     *
     * @param {Promise} retry optional to allow for testing
     * @returns {Promise.<T>}
     */
    retryAll (retry = this.retry) {

        const all = Promise.all(this.buffer.map(request => {
            return retry(request);
        }));

        this.buffer = [];

        // while we dequeue all the requests, we are only returning the first. In practice,
        // there is likely to be only one when not in a provisioning mode. We should also see that
        // there is at least one. Hence, reject reject if the list is empty.
        return all
            .then(results => results.filter(result => !!result)[0]
                || Promise.reject('No representation returned'));
    }
}

export default HTTPQueue;

export let httpQueue = new HTTPQueue();
