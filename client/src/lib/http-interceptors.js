import axios from 'axios';
import { log } from 'semanticLink';
import EventBus, { loginConfirmed, loginRequired, offline, serverError } from './util/EventBus';
import { httpQueue } from './util/HTTPQueue';

log.debug('[Axios] Setting up http interceptors');

/**
 * We are using axios as the http client. It allows us to register interceptors
 *  @see https://github.com/axios/axios#interceptors
 */

/**
 * Setup the headers on the xhr object so that we can play nice with the API
 */
axios.interceptors.request.use(
    config => {
        /*
         * We are going to be a monolingual JSON application
         */
        config.headers['Accept'] = 'application/json;q=1.0';
        config.headers['X-Requested-With'] = 'XMLHttpRequest';

        return config;
    },
    err => Promise.reject(err));

/**
 * Intercept http offline error message of "Network Error" so we can continue
 * with request processing after it comes back online. The Offline component
 * has subscribed to the `offline` message and continues processing.
 *
 * Note: `error` contains the original `config` to replay the original request.
 *
 * The order of this method MUST be before other error handling (eg 500, 401) because error in this scenario
 * does NOT return a response.
 */
axios.interceptors.response.use(
    response => response,
    error => {
        // axios returns a network error that we need to match against readyState < DONE (4)
        // to correctly trap network down errors. All other errors should be passed through.
        if (error.message === 'Network Error' && error.request.readyState <= 4) {
            const promise = Promise.resolve(error);
            httpQueue.pushToBuffer(error.config, promise);
            EventBus.$emit(offline, error);
        } else {
            return Promise.reject(error);
        }
    });

/**
 * Intercept 500 (server error) request so that we can login on-demand and then continue
 * with request processing.
 */
axios.interceptors.response.use(
    response => response,
    error => {
        if (error.response && 500 === error.response.status) {
            const promise = Promise.resolve(error);
            EventBus.$emit(serverError, error);
            return promise;
        } else {
            return Promise.reject(error);
        }
    });

/**
 * Intercept 401 (unauthorised) request so that we can login on-demand and then continue
 * with request processing.
 *
 * TODO: the buffer should really have a set of ignore URLs are blacklisted before pushing
 */
axios.interceptors.response.use(
    response => response,
    error => {

        if (error.response && 401 === error.response.status) {

            httpQueue.pushToBuffer(error.config);

            return new Promise((resolve, reject) => {

                // this event starts the process of logging in and MUST be handled
                EventBus.$emit(loginRequired, error);

                // the event handling the login MUST then trigger this event to be caught
                // eg EventBus.$emit(loginConfimed)
                EventBus.$on(loginConfirmed, () => {

                    log.info('Authentication: login confirmed (http-interceptor)');
                    httpQueue.retryAll()
                        .then(resolve)
                        .catch(reject);
                });

            });
        } else {
            return Promise.reject(error);
        }
    });

/**
 * Set the bearer token in the headers for this axios instance
 */
export const setBearerToken = (token) => {

    if (!token) {
        log.info('Authentication: no access token found');
    }

    log.info('Authentication: setting access token');

    axios.interceptors.request.use(
        config => {
            config.withCredentials = true;
            config.headers['Authorization'] = `Bearer ${token}`;
            return config;
        },
        err => Promise.reject(err));
    return Promise.resolve();

};

