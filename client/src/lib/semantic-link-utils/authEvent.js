
/**
 * *********************
 *
 * Event messages that we are registering globally
 *
 * *********************
 */

/**
 * HTTP response 401 not authorised. This event should be triggered when an http call returns a 401 response.
 * This should be handled with on-demand authentication and then the original request retried. Any requests
 * in between are to be queued in the meantime.
 *
 * @type {string}
 */
export const authRequired = 'event:auth-required';

/**
 * HTTP response 401 not authorised. This event should be triggered once the user has made successfully authenticated.
 *
 * @type {string}
 */
export const authConfirmed = 'event:auth-confirmed';

/**
 * The browser cannot find an connection out to the internet.
 *
 * @type {string}
 */
export const offline = 'event:http-offline';

/**
 * We are waiting for the browser to become online again. This is mostly used when there is a dialog
 * alerting the user that we are waiting for the http connection to come back.
 *
 * @type {string}
 */
export const checking = 'event:http-checking';

/**
 * After the http has gone offline this event follows when it comes back online and the queue of requests
 * is attempted.
 *
 * @type {string}
 */
export const restored = 'event:http-restored';

/**
 * HTTP response 500 (internal server error). This event should be triggered when there is a server error.
 *
 * @type {string}
 */
export const serverError = 'event:http-500';
