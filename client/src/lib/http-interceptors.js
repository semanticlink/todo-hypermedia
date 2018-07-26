import axios from 'axios';
import {log} from 'semanticLink';
import EventBus, {loginConfirmed, loginRequired, offline, serverError} from './util/EventBus';
import {httpQueue} from './util/HTTPQueue';
import * as authorization from 'auth-header';

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
export const setJsonWebTokenOnHeaders = (token) => {

    if (!token) {
        log.info('Authentication: no access token found');
    }

    log.info('Authentication: setting access token');

    axios.interceptors.request.use(
        config => {
            config.withCredentials = true;
            config.headers[AUTHORIZATION_HEADER] = authorization.format({scheme: JWT, token: token});
            return config;
        },
        err => Promise.reject(err));
    return Promise.resolve();

};

/**
 * Clears the bearer token in the headers for this axios instance (part of logging out process)
 */
export const clearJsonWebTokenOnHeaders = () => {

    log.info('Authentication: clearing access token');

    axios.interceptors.request.use(
        config => {
            delete config.withCredentials;
            delete config.headers[AUTHORIZATION_HEADER];
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
 * Name of the Authorization header.
 *
 * @see https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Authorization
 * @type {string}
 */
export const AUTHORIZATION_HEADER = 'Authorization';

/**
 * We are using JSON Web Token (JWT and not Java Web Tokens) authentication  here (for now)
 * @see https://tools.ietf.org/html/rfc7235
 * @example www-authenticate: JSONWebToken realm="api", uri=http://example.com/authenticate/jwt
 * @type {string}
 */
export const JWT = 'jwt';

/**
 * Name of the realm when matching for Auth0 provider
 * @example www-authenticate: Bearer realm="auth0", rel=authenticate, uri=http://example.com
 * @type {string}
 */
export const API_AUTH0_REALM = 'api-auth0';

/**
 * @class WWWAuthenticateHeader
 * @property {string} scheme Default: jwt
 * @property {?string} token Usually used in conjunction with scheme 'Bearer'
 * @property {string} params.realm
 * @property {?string} params.rel
 * @property {string} params.uri
 * @property {?string} params.error [invalid_request|invalid_token|insufficient_scope] these may be included
 * @property {?string} params.error_description
 * @property {?string} params.error_uri
 *
 * @see from AspNetCore Authentication JwtBearerHandler https://github.com/aspnet/Security/blob/master/src/Microsoft.AspNetCore.Authentication.JwtBearer/JwtBearerHandler.cs#L63
 *
 * @example
 *
 * Success
 *
 * WWW-Authenticate: jwt realm="api-auth0", uri=https://example.com/authenticate/auth0
 *
 * Error
 *
 * WWW-Authenticate: jwt realm="api-auth0", error="invalid_token", error_description="The access token expired"
 * @see Error Codes: https://tools.ietf.org/html/rfc6750#section-3.1
 */


/**
 * Known error codes for invalid token
 * @see Error Codes: https://tools.ietf.org/html/rfc6750#section-3.1
 * @type {{ERROR: string, DESCRIPTION: string}}
 */
export const INVALID_TOKEN = {
    ERROR: 'invalid_token',
    DESCRIPTION: 'The token is expired'
};


/**
 * Looks inside the 401 response www-authenticate header and returns the header details
 *
 * @example www-authenticate: Bearer realm="api", rel=authenticate, uri=http://example.com
 *
 * TODO: this does not implement multiple www-authenticate headers
 * TODO: this does not deal with underlying implementation of multiple realms in one header
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader}
 */
const parseErrorForAuthenticateHeader = error => {
    if (!error && !error.response) {
        log.error('This does not look like an Axios error');
        return;
    }

    const wwwAuthenticate = error.response.headers[WWW_AUTHENTICATE_HEADER];
    if (!wwwAuthenticate) {
        log.error(`No www-authenticate header for bearer token on ${error.config.url}`);
        return;
    }

    /**
     * @example www-authenticate: jwt realm="api-auth0", uri=http://example.com/authenticate/auth0
     * @type {{scheme: string, token: string, params: {realm: string, rel: string, uri: string}}}
     */
    const auth = authorization.parse(wwwAuthenticate);

    if (!auth && auth.scheme === JWT && auth.params.rel === API_AUTH0_REALM) {
        log.error(`No '${JWT}' scheme on realm '${API_AUTH0_REALM}' with link rel found: '${wwwAuthenticate}'`);
    }

    return auth;
};

/**
 * Looks inside the 401 response wwww-authenticate headers to see if it is an expired token that needs renewal
 * @param error
 * @returns {boolean}
 */
export const renewToken = error => {
    const header = parseErrorForAuthenticateHeader(error);
    return header.params.error === INVALID_TOKEN.ERROR && header.params.error_description === INVALID_TOKEN.DESCRIPTION;
};

/**
 * Looks inside the 401 response www-authenticate header and gets the link relation
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.params.rel} rel
 */
export const getBearerLinkRelation = error => parseErrorForAuthenticateHeader(error).params.rel;


/**
 * Looks inside the 401 response www-authenticate header and gets the representation uri
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.params.uri} uri
 */
export const getAuthenticationUri = error => parseErrorForAuthenticateHeader(error).params.uri;

/**
 * Looks inside the 401 response www-authenticate header and gets the realm
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.params.realm} realm
 */
export const getAuthenticationRealm = error => parseErrorForAuthenticateHeader(error).params.realm;

/**
 * Looks inside the 401 response www-authenticate header and gets the scheme
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.scheme} scheme
 */
export const getAuthenticationScheme = error => parseErrorForAuthenticateHeader(error).scheme;



