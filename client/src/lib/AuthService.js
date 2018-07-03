import auth0 from 'auth0-js';
import { log } from 'logger';
import axios from 'axios';
import { getAuthenticationUri } from './http-interceptors';


/**
 * Authorisation config for Auth0
 *
 * @class AuthServiceConfiguration
 * @see WebAuth.baseOptions
 * @property {string} clientID Your Auth0 client ID.
 * @property {string} audience The default audience to be used for requesting API access.
 * @property {string} connection Specifies the connection to use rather than presenting all connections available to the application.
 * @property {string} scope The scopes which you want to request authorization for. These must be separated by a space. You can request any of the standard OIDC scopes about users, such as profile and email, custom claims that must conform to a namespaced format, or any scopes supported by the target API (for example, read:contacts). Include offline_access to get a Refresh Token.
 * @property {string} responseType It can be any space separated list of the values code, token, id_token. It defaults to 'token', unless a redirectUri is provided, then it defaults to 'code'.
 * @property {string} clientID Your Auth0 client ID.
 * @property {string} redirectUri The URL to which Auth0 will redirect the browser after authorization has been granted for the user.
 * @property {string} state An arbitrary value that should be maintained across redirects. It is useful to mitigate CSRF attacks and for any contextual information (for example, a return URL) that you might need after the authentication process is finished. For more information, see the state parameter documentation.
 * @property {string} nonce An arbitrary value shared between the request to Auth0 and then used again the popup callback to ensure it is the same sequence.
 *
 * @see https://auth0.com/docs/libraries/auth0js/v9
 */
/**
 * Authorisation config for Auth0
 *
 * @class AuthPopupCallbackConfiguration
 * @see Popup.callback
 * @property {string} state An arbitrary value that should be maintained across redirects. It is useful to mitigate CSRF attacks and for any contextual information (for example, a return URL) that you might need after the authentication process is finished. For more information, see the state parameter documentation.
 * @property {string} nonce An arbitrary value shared between the request to Auth0 and then used again the popup callback to ensure it is the same sequence.
 *
 * @see https://auth0.com/docs/libraries/auth0js/v9
 */

/**
 * Authorisation Result from auth0
 *
 * @example
 *
 *  accessToken : "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IlFUSkNSamhFTmp ..."
 *  appState : "lMiL50O5V6viNSDzvzUovnopu9fKlX.U"
 *  expiresIn : 7200
 *  idToken : "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IlFUSkNSamhFTmpRM09U ..."
 *  idTokenPayload :
 *     atHash : "m3_GRr-tVC7Kvxn5T9URGg"
 *     aud : "3CYUtb8Uf9NxwesvBJAs2gNjqYk3yfZ8"
 *     exp : 1530263423
 *     iat : 1530227423
 *     iss : "https://rewire-sample.au.auth0.com/"
 *     name : "test-todo@rewire.nz"
 *     nickname : "test-todo"
 *     nonce : "1530226973997"
 *     picture : "https://s.gravatar.com/avatar/573f1ac81980b35916d97376cb1adba5?s=480&r=pg&d=https%3A%2F%2Fcdn.auth0.com%2Favatars%2Fte.png"
 *     sub : "auth0|5b32b696a8c12d3b9a32b138"
 *     updatedAt : "2018-06-28T23:10:20.911Z"

 *  refreshToken : null
 *  scope : "openid profile"
 *  state : "lMiL50O5V6viNSDzvzUovnopu9fKlX.U"
 *  tokenType : "Bearer"
 *
 * @class AuthResult
 * @property {string} accessToken
 * @property {string} appState
 * @property {string} expiresIn
 * @property {string} idToken
 * @property {string} idTokenPayload.atHash
 * @property {string} idTokenPayload.aud
 * @property {string} idTokenPayload.exp
 * @property {string} idTokenPayload.iat
 * @property {string} idTokenPayload.iss
 * @property {string} idTokenPayload.name
 * @property {string} idTokenPayload.nickname
 * @property {string} idTokenPayload.nonce
 * @property {string} idTokenPayload.picture
 * @property {string} idTokenPayload.sub
 * @property {string} idTokenPayload.updatedAt
 * @property {string} refereshToken
 * @property {string} scope
 * @property {string} state
 * @property {string} tokenType
 */


/**
 * Local storage keys
 * @type {{ACCESS_TOKEN_KEY: string, ID_TOKEN_KEY: string, EXPIRES_AT_KEY: string}}
 */
const KEY = {
    /**
     * JWT Access token returned from the auth0
     */
    ACCESS_TOKEN: 'access_token',
    /**
     * JWT token that stores all the user profile information
     *
     * @example: containing information is
     *
     *     atHash : "m3_GRr-tVC7Kvxn5T9URGg"
     *     aud : "3CYUtb8Uf9NxwesvBJAs2gNjqYk3yfZ8"
     *     exp : 1530263423
     *     iat : 1530227423
     *     iss : "https://rewire-sample.au.auth0.com/"
     *     name : "test-todo@rewire.nz"
     *     nickname : "test-todo"
     *     nonce : "1530226973997"
     *     picture : "https://s.gravatar.com/avatar/573f1ac81980b35916d97376cb1adba5?s=480&r=pg&d=https%3A%2F%2Fcdn.auth0.com%2Favatars%2Fte.png"
     *     sub : "auth0|5b32b696a8c12d3b9a32b138"
     *     updatedAt : "2018-06-28T23:10:20.911Z"
     */
    ID_TOKEN: 'id_token',
    /**
     * Datetime when the token expire
     */
    EXPIRES_AT: 'expires_at',
    /**
     * Client configuration is the serialised {@link AuthConfigurationRepresentation}
     */
    CLIENT_CONFIGURATION: 'clientConfig'
};

export default class AuthService {

    /**
     * @param {AuthServiceConfiguration|Auth0ConfigurationRepresentation=} options
     */
    constructor(options) {

        // update the client configuration to the last know set so that we can
        // get a valid auth0 client (ie with state) upon return into the site
        // and without a www-authenticate challenge
        options = options ? options : AuthService.clientConfiguration;

        if (options) {
            AuthService.clientConfiguration = options;
        }

        /**
         *  @type {AuthServiceConfiguration}
         */
        const opts = Object.assign(
            {
                domain: '',
                clientID: '',
                audience: '',
                scope: '',
                redirectUri: window.location.origin,
                nonce: AuthService.makeNonce()
            },
            options);

        this.auth0 = new auth0.WebAuth(opts);

        log.debug('[Auth0] loaded');
    }

    /**
     * Each request needs its own nonce so that it
     * can used on either side of the auth request/response.
     *
     * Note that the response does return this nonce in authResult.idTokenPayload.nonce
     */
    static makeNonce() {
        return new Date().getTime().toString();
    }

    /**
     * Login via auth0 popup window
     * @param {Function} cb
     */
    login(cb) {

        log.debug('Opening popup login window');

        this.auth0.popup.authorize({}, (err, /** @type {AuthResult} */authResult) => {
            if (authResult) {
                AuthService.setSession(authResult);
            }

            // KLUDGE: false negative of entering into a page when we don't want authentication
            if (err && err.code === null) {
                return;
            }

            cb(err, authResult);
        });
    }

    /**
     * Renew the access token
     */
    renewToken(){
        new Error('Not implemented');
    }


    /**
     * Auto closes the popup window upon return from auth0
     *
     * In practice, this close method will invoke the callback in the login() method.
     *
     * @see https://community.auth0.com/t/auth0-js-popup-autoclose/7365/2
     * @param {AuthResult} authResult
     */
    close(authResult) {
        log.debug('Closing popup window');

        /**
         * Both are mandatory (and undocumented)
         */
        this.auth0.popup.callback(/** @type {AuthPopupCallbackConfiguration} */{
            nonce: authResult.idTokenPayload.nonce,
            state: authResult.state
        });
    }

    /**
     * @class AuthError
     * @property {string} error
     * @property {string} errorMessage
     */

    /**
     * At this stage, call this on every entry into the application to check to
     * see if we have returned from auth0 authentication.
     *
     * The problem here is that auth0 takes over the hash (which we also use for client-side routing)
     * and so we can't leave this up to the router. We must intercept and stop the process which means
     * closing the window.
     *
     */
    handleAuthentication() {
        this.auth0.parseHash((/** @type {AuthError}*/err, /** @type {AuthResult} */authResult) => {

            //ignore the err because handleAuthentication is called on all entries to the site
            if (err) {
                log.warn(`[Auth] ${err.error}: ${err.errorMessage}`);
            }

            if (authResult) {
                this.close(authResult);
            }

        });

    }

    /**
     * Auth0 client configuration from the api
     * @returns {Auth0ConfigurationRepresentation}
     */
    static get clientConfiguration() {
        return JSON.parse(localStorage.getItem(KEY.CLIENT_CONFIGURATION));
    }

    /**
     * Auth0 client configuration from the api
     * @param {Auth0ConfigurationRepresentation} configuration
     */
    static set clientConfiguration(configuration) {
        localStorage.setItem(KEY.CLIENT_CONFIGURATION, JSON.stringify(configuration));
    }

    /**
     *
     * Sets access token, user profile and expiration in the locale storage
     * @param {AuthResult} authResult
     */
    static setSession(authResult) {
        // Set the time that the access token will expire at
        const expiresAt = (authResult.expiresIn * 1000) + new Date().getTime();
        localStorage.setItem(KEY.ACCESS_TOKEN, authResult.accessToken);
        localStorage.setItem(KEY.ID_TOKEN, authResult.idToken);
        localStorage.setItem(KEY.EXPIRES_AT, expiresAt);
    }

    /**
     * Removes all user auth session which effectively logouts out the user
     */
    static clearSession() {
        localStorage.removeItem(KEY.ACCESS_TOKEN);
        localStorage.removeItem(KEY.ID_TOKEN);
        localStorage.removeItem(KEY.EXPIRES_AT);
    }

    /**
     * Checks whether the user has a current access token that is past its expiry time.
     *
     * This assumes that the if there is an advance datetime so too is there an access token to send.
     * @returns {boolean}
     */
    static get isAuthenticated() {
        return new Date().getTime() < AuthService.tokenExpiresAt;
    }

    /**
     * The access token provided from auth0
     *
     * The Access Token is a credential that can be used by an application to access an API. It can
     * be any type of token (such as an opaque string or a JWT) and is meant for an API. Its purpose
     * is to inform the API that the bearer of this token has been authorized to access the API and
     * perform specific actions (as specified by the scope that has been granted). The Access Token
     * should be used as a Bearer credential and transmitted in an HTTP Authorization header to the API.
     *
     * @see https://auth0.com/docs/tokens/access-token
     * @returns {string | null}
     */
    static get accessToken() {
        return localStorage.getItem(KEY.ACCESS_TOKEN);
    }

    /**
     * The token granting token provided from auth0 to be used on reissue (and other functions)
     *
     * A Refresh Token is a special kind of token that contains the information required to obtain a
     * new Access Token or ID Token. Usually, a user will need a new Access Token only after the
     * previous one expires, or when gaining access to a new resource for the first time. Refresh Tokens are
     * subject to strict storage requirements to ensure that they are not leaked. Also, Refresh Tokens
     * can be revoked by the Authorization Server.
     *
     * @see https://auth0.com/docs/tokens/refresh_token
     * @returns {string | null}
     */
    static get RefreshToken() {
        throw Error('Not implemented');
    }


    /**
     * The token granting token provided from auth0 that contains the user profile
     *
     * The ID Token is a JSON Web Token (JWT) that contains user profile information
     * (like the user's name, email, and so forth), represented in the form of claims.
     * These claims are statements about the user, which can be trusted if the consumer
     * of the token can verify its signature.
     *
     * @see https://auth0.com/docs/tokens/id-token
     * @returns {string | null}
     */
    static get IdToken() {
        return localStorage.getItem(KEY.ID_TOKEN);
    }

    /**
     * The expiry time of the access token . It useds Javascript date getTime() method which returns the
     * numeric value corresponding to the time for the specified date according to universal time.
     * The value returned by the getTime method is the number of milliseconds since 1 January 1970 00:00:00.
     * @returns {number | null}
     */
    static get tokenExpiresAt() {
        return parseInt(localStorage.getItem(KEY.EXPIRES_AT));
    }

    /**
     * Authorisation config for Auth0
     *
     * @class Auth0ConfigurationRepresentation
     * @extends LinkedRepresentation
     * @property {string} clientID Your Auth0 client ID.
     * @property {string} audience The default audience to be used for requesting API access.
     * @property {string} domain The default audience to be used for requesting API access.
     * @property {string[]} scope The scopes which you want to request authorization for. These must be separated by a space. You can request any of the standard OIDC scopes about users, such as profile and email, custom claims that must conform to a namespaced format, or any scopes supported by the target API (for example, read:contacts). Include offline_access to get a Refresh Token.
     * @property {string[]} responseType It can be any space separated list of the values code, token, id_token. It defaults to 'token', unless a redirectUri is provided, then it defaults to 'code'.
     * @property {string} clientID Your Auth0 client ID.
     * @property {number} leeway A value in seconds; leeway to allow for clock skew with regard to JWT expiration times.
     * @property {string} realm The realm to be matched with www-authenticate header.
     *
     * @see https://auth0.com/docs/libraries/auth0js/v9
     */

    /**
     *
     * Uses the www-authenticate header to load up the collection and the create form
     * ready for submission back to the api
     *
     * @param {AxiosError} error 401 trapped error with www-authenticate header
     * @returns {Promise<AxiosResponse<Auth0ConfigurationRepresentation>>}
     */
    static loadFrom401JsonWebTokenChallenge(error) {
        let authenticationConfigurationUri = getAuthenticationUri(error);
        return axios.get(authenticationConfigurationUri);
    }

    /**
     * Transform an across the wire {@link Auth0ConfigurationRepresentation} into the configuration for this service
     * @param {Auth0ConfigurationRepresentation} cfg
     * @returns {AuthServiceConfiguration}
     */
    static toConfiguration(cfg) {
        return {
            clientID: cfg.clientID,
            domain: cfg.domain,
            audience: cfg.audience,
            scope: cfg.scope.join(' '),
            leeway: cfg.leeway,
            responseType: cfg.responseType.join(' ')
        };
    }

    /**
     * Factory method to create an AuthService based on the {@link Auth0ConfigurationRepresentation}
     * @param {Auth0ConfigurationRepresentation} cfg
     * @returns {AuthService}
     */
    static makeFromRepresentation(cfg) {
        return new AuthService(AuthService.toConfiguration(cfg));
    }

}

let authService = new AuthService();

export { authService };