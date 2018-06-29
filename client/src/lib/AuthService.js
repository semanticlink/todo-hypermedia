import auth0 from 'auth0-js';
import { log } from 'logger';

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
 *
 * @type {AuthServiceConfiguration}
 */
export const AUTH_CONFIG = {
    clientID: '3CYUtb8Uf9NxwesvBJAs2gNjqYk3yfZ8',
    domain: 'rewire-sample.au.auth0.com',
    audience: 'todo-rest-test'
};

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
    EXPIRES_AT: 'expires_at'
};

export default class AuthService {

    /**
     * @param {AuthServiceConfiguration=} options
     */
    constructor(options) {

        /**
         * offline_access - returns refresh token
         * @see https://auth0.com/docs/libraries/auth0js/v9#webauth-authorize-
         * @type {string}
         */
        const requestedScopes = 'openid profile offline_access';

        /**
         *  @type {AuthServiceConfiguration}
         */
        const opts = Object.assign(
            {},

            {
                responseType: 'token id_token',
                scope: requestedScopes,
                leeway: 30,
                redirectUri: window.location.origin,
                nonce: AuthService.makeNonce()
            },
            AUTH_CONFIG,
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
     * @returns {Promise<any>}
     */
    login() {

        log.debug('Opening popup login window');

        return new Promise((response, reject) => {
            this.auth0.popup.authorize({}, (err, authResult) => {
                if (!err) {
                    AuthService.setSession(authResult);
                    return response(authResult);
                } else {
                    log.error('[Auth] ', err);
                    return reject(err);
                }

            });
        });

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
     * At this stage, call this on every entry into the application to check to
     * see if we have returned from auth0 authentication.
     *
     * The problem here is that auth0 takes over the hash (which we also use for client-side routing)
     * and so we can't leave this up to the router. We must intercept and stop the process which means
     * closing the window.
     *
     */
    handleAuthentication() {
        this.auth0.parseHash((err, /** @type {AuthResult} */authResult) => {

            if (err) {
                // this is the expected behaviour that the user is mostly not coming from auth0
            } else if (!authResult) {
                log.error('[Auth] unexpected empty result.');
            } else {
                log.debug('Auth token found in the hash');
                this.close(authResult);
            }
        });

    }

    /**
     *
     * Sets access token, user profile and expiration in the locale storage
     * @param {AuthResult} authResult
     */
    static setSession(authResult) {
        // Set the time that the access token will expire at
        const expiresAt = JSON.stringify((authResult.expiresIn * 1000) + new Date().getTime());
        localStorage.setItem(KEY.ACCESS_TOKEN, authResult.accessToken);
        localStorage.setItem(KEY.ID_TOKEN, authResult.idToken);
        localStorage.setItem(KEY.EXPIRES_AT, expiresAt);
    }

}

let authService = new AuthService();

export { authService };