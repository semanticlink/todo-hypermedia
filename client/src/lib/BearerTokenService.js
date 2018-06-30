/**
 * Local storage keys
 * @type {{AUTH_TOKEN: string}}
 */
const KEY = {
    /**
     * JWT Access token returned from the api
     */
    AUTH_TOKEN: 'auth',
};

export default class BearerTokenService {

    /**
     *
     * Sets access token in the locale storage
     * @param {string} token
     */
    static setToken(token) {
        // save bearer token so that when users do a full refresh
        // we can save the token across a refresh
        localStorage.set(KEY.AUTH_TOKEN, token);
    }

    /**
     *
     * Sets access token in the locale storage
     */
    static getToken() {
        localStorage.get(KEY.AUTH_TOKEN);
    }

}