/**
 * Local storage keys
 * @type {{AUTH_TOKEN: string}}
 */
const KEY = {
    /**
     * JWT Access token returned from the api for username/password
     */
    AUTH_TOKEN: 'bearer',
};

export default class BearerTokenService {

    /**
     *
     * Sets access token in the locale storage
     * @param {string} token
     */
    static set token(token) {
        // save bearer token so that when users do a full refresh
        // we can save the token across a refresh
        localStorage.setItem(KEY.AUTH_TOKEN, token);
    }

    /**
     *
     * Sets access token in the locale storage
     */
    static get token() {
        return localStorage.getItem(KEY.AUTH_TOKEN);
    }

}