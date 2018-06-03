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

/**
 * Name of the WWW-Authenticate header
 * @see https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/WWW-Authenticate
 * @type {string}
 */
export const WWW_AUTHENTICATE_HEADER = 'www-authenticate';

/**
 * (simple) Regular expression that matches to the link relation for the 'api' realm.
 *
 * Note: hard-coded to realm="api"
 *
 * In practice, we could have options of the client looking for the linke relation 'api' and/or providing in
 * the `www-authenticate` header as 'api'
 *
 * This regex matches two and returns in an array:
 *
 * @example
 *
 *  www-authtenticate: Bearer realm="api", rel="authtenticate", api="http://example.com/"
 *
 *    match[1]: authenticate
 *    match[2]: http://example.com/
 *
 *
 * @type {RegExp}
 */
export const MATCH_WWW_AUTHENTICATE_HEADER = /Bearer realm="api", rel="([^,]*)" api="([^,]*)"/;

/**
 * Looks inside the 401 response www-authenticate header and gets the link relation
 * @param {AxiosError} error
 * @returns {string|undefined} token
 */
export const getBearerLinkRelation = error => {

    if (!error && !error.response) {
        log.error('This does not look like an Axios error');
        return;
    }

    const wwwAuthenticate = error.response.headers[WWW_AUTHENTICATE_HEADER];
    if (!wwwAuthenticate) {
        log.error(`No www-authenticate header for bearer token on ${error.config.url}`);
        return;
    }

    const token = wwwAuthenticate.match(MATCH_WWW_AUTHENTICATE_HEADER)[1];

    if (!token) {
        log.warn(`No Bearer token on realm 'api' with link rel found: '${wwwAuthenticate}'`);
    }

    return token;
};


/**
 * Looks inside the 401 response www-authenticate header and gets the representation uri
 * @param {AxiosError} error
 * @returns {string|undefined} uri
 */
export const getAuthenticationUri = error => {

    if (!error && !error.response) {
        log.error('This does not look like an Axios error');
        return;
    }

    const wwwAuthenticate = error.response.headers[WWW_AUTHENTICATE_HEADER];
    if (!wwwAuthenticate) {
        log.error(`No www-authenticate header for authentication on ${error.config.url}`);
        return;
    }

    const uri = wwwAuthenticate.match(MATCH_WWW_AUTHENTICATE_HEADER)[2];

    if (!uri) {
        log.warn(`No representation uri on realm 'api' found: '${wwwAuthenticate}'`);
    }

    return uri;
};



