import axios from 'axios';
import {log} from 'logger';
import {httpQueue} from './HTTPQueue';
import * as authorization from 'auth-header';
import {authConfirmed, authRequired, offline, serverError} from './authEvent';
import {eventBus} from './EventBus';
import {wwwAuthenticateHeader, amazonAuthenticateHeader, parseAuthenticateHeader} from './WWWAuthenticate';


/**
 * @class InterceptorsOptions
 * @property {boolean|undefined} queue401s when a 401 is intercepted add to queue for replay (or simply drop)
 */
/**
 *
 * @param options
 */
export const setInterceptors = options => {

    options = {
        queue401s: true,
        ...options,
    };

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

                log.debug(`[Network] ${error.message} on '${error.config.url}'`);

                const promise = Promise.resolve(error);
                httpQueue.pushToBuffer(error.config, promise);
                if (eventBus) {
                    eventBus.$emit(offline, error);
                } else {
                    log.warn('[Network] Event bus not created');
                }

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
                if (eventBus) {
                    eventBus.$emit(serverError, error);
                } else {
                    log.warn('[Network] Event bus not created');
                }

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

                if (options.queue401s) {
                    httpQueue.pushToBuffer(error.config);
                }

                return new Promise((resolve, reject) => {

                    if (eventBus) {
                        // this event starts the process of logging in and MUST be handled
                        eventBus.$emit(authRequired, error);

                        // the event handling the login MUST then trigger this event to be caught
                        // eg EventBus.$emit(authConfirmed)
                        eventBus.$on(authConfirmed, () => {

                            log.debug('[Authentication] login confirmed (http-interceptor)');
                            httpQueue.retryAll()
                                .then(resolve)
                                .catch(reject);
                        });
                    } else {
                        log.warn('[Network] Event bus not created');
                    }


                });
            } else {
                return Promise.reject(error);
            }
        });
};

/**
 * Hold a reference so that it can be rejected/cleared
 */
let jwtInterceptor;

/**
 * Set the bearer token in the headers for this axios instance
 */
export const setJsonWebTokenOnHeaders = (token) => {

    if (!token) {
        log.info('[Authentication] no access token found');
    }

    clearJsonWebTokenOnHeaders();

    log.debug(`[Authentication] setting token on www-authenicate header interceptor scheme '${JWT}'`);

    jwtInterceptor = axios.interceptors.request.use(
        config => {
            config.withCredentials = true;
            config.headers[AUTHORIZATION_HEADER] = authorization.format({scheme: JWT, token: token});
            return config;
        },
        err => Promise.reject(err));
};

/**
 * Clears the bearer token in the headers for this axios instance (part of logging out process)
 */
export const clearJsonWebTokenOnHeaders = () => {

    log.debug('[Authentication] clearing access token');

    if (jwtInterceptor) {
        log.debug('[Authentication] clear jwt token');
        axios.interceptors.request.eject(jwtInterceptor);
    } else {
        log.debug('[Authentication] no access token to clear');
    }
};


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
 *
 * Return type for the 'auth_header' node module
 *
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
 * @example Success
 *
 * WWW-Authenticate: jwt realm="api-auth0", uri=https://example.com/authenticate/auth0
 * @example Error
 *
 * WWW-Authenticate: jwt realm="api-auth0", error="invalid_token", error_description="The access token expired"
 *
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
 * @example x-amzn-remapped-www-authenticate: Bearer realm="api", rel=authenticate, uri=http://example.com
 *
 * TODO: this does not implement multiple www-authenticate headers
 * TODO: this does not deal with underlying implementation of multiple realms in one header
 * @param {AxiosError} error
 * @return {WWWAuthenticateHeader|undefined}
 */
const parseErrorForAuthenticateHeader = error => {
    if (!error && !error.response) {
        log.error('[Authentication] This does not look like an Axios error');
        return;
    }

    const wwwAuthenticate = parseAuthenticateHeader(error.response.headers);
    if (!wwwAuthenticate) {
        log.error(`[Authentication] No www-authenticate header for bearer token on ${error.config.url}`);
        return;
    }

    const headerValue = error.response.headers[wwwAuthenticate];
    log.debug(`[Authentication] parsing header '${wwwAuthenticate}' for '${headerValue}'`);
    /**
     * @example www-authenticate: jwt realm="api-auth0", uri=http://example.com/authenticate/auth0
     * @type {{scheme: string, token: string, params: {realm: string, rel: string, uri: string}}}
     */
    const auth = authorization.parse(headerValue);

    if (!auth && auth.scheme === JWT && auth.params.rel === API_AUTH0_REALM) {
        log.error(`[Authentication ] No '${JWT}' scheme on realm '${API_AUTH0_REALM}' with link rel found: '${wwwAuthenticate}'`);
    }

    return auth;
};

/**
 * Looks inside the 401 response www-authenticate headers to see if it is an expired token that needs renewal
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
 * @returns {WWWAuthenticateHeader.params.rel|string} rel
 */
export const getBearerLinkRelation = error => parseErrorForAuthenticateHeader(error).params.rel;


/**
 * Looks inside the 401 response www-authenticate header and gets the representation uri
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.params.uri|string} uri
 */
export const getAuthenticationUri = error => parseErrorForAuthenticateHeader(error).params.uri;

/**
 * Looks inside the 401 response www-authenticate header and gets the realm
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.params.realm|string} realm
 */
export const getAuthenticationRealm = error => parseErrorForAuthenticateHeader(error).params.realm;

/**
 * Looks inside the 401 response www-authenticate header and gets the scheme
 *
 * @param {AxiosError} error
 * @returns {WWWAuthenticateHeader.scheme|string} scheme
 */
export const getAuthenticationScheme = error => parseErrorForAuthenticateHeader(error).scheme;



