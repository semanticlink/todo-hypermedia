import _ from 'underscore';
import {log} from 'logger';

/**
 * Name of the WWW-Authenticate header
 * @see https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/WWW-Authenticate
 * @type {string}
 */
export const wwwAuthenticateHeader = 'www-authenticate';

/**
 * Amazon rewrite the header to serverless lambda functions proxied through API Gateway :-(
 * @type {string}
 */
export const amazonAuthenticateHeader = 'x-amzn-remapped-www-authenticate';

/**
 * This is a partial implementation for parsing the WWW-Authenticate header and
 * returning the negotiate type:
 *
 *  - Basic
 *  - Rest
 *  - Bearer
 *
 * Example from the http response header:
 *
 *   WWW-Authenticate: Negotiate, Basic realm="example.com", Rest https://example.com/authenticator, Bearer realm="example.com"
 *
 */


/**
 * The list of known headers in lower case.
 *
 * @type {string[]}
 * @readonly
 */
const knownHeaders = [
    'location',
    'accept',
    wwwAuthenticateHeader,
    amazonAuthenticateHeader,
    'content-type'
];


/**
 * Firefox managed to corrupt the headers when there are multiple headers
 * with the same value. It appears to loose the header name off all
 * but the first entry, and then re-parses the result so that the
 * header value is a random value up to the first colon in the header value.
 *
 * Chrome supports it just fine.
 * @private
 */
export function rewriteBrokenFirefoxHeadersAndSelect(headers, selectHeader) {
    let lastKnownHeader; // starts out unknown

    return _(headers)
        .chain()
        .map(function (value, key) {
            if (_(knownHeaders).contains(key.toLowerCase())) {
                lastKnownHeader = key;
                return [key, value];
            }
            if (lastKnownHeader) {
                // This is not 100% correct. We don't know if the corruption
                // actually ate a colon.
                return [lastKnownHeader, key + (value ? (':' + value) : '')];
            } else {
                log.warn('Failed to fix-up unknown header ' + key + '="' + value + '"');
                return [key, value];
            }
        })
        .filter(function (kv) {
            return kv[0].toLowerCase() === selectHeader.toLowerCase();
        })
        .map(function (kv) {
            return kv[1];
        })
        .value()
        .join(',');
}

/**
 * Takes a WWW-Authenticate string and matches the negotiate type
 *
 * Negotiate, Basic realm="example.com", Resource uri="/Authenticator", Bearer realm="example.com"
 *
 * @param {string} headerValues www-authenticate header values as a command separated string
 * @param {string|'Resource'|'Basic'|'Bearer'} negotiateType type of authentication to  parse for
 * @private
 * @return {string|undefined} negotiate type
 */
export function matchNegotiateType(headerValues, negotiateType) {
    negotiateType = negotiateType || 'Resource';

    // The split won't pull out additional whitespace. Return the first result
    // the matches the requested type.
    return _(headerValues.split(','))
        .chain()
        .map(function (type) {
            const authType = /^\s*(\w+)\s+(.*)/.exec(type);
            if (_(authType).size() >= 2 && authType[1].toLowerCase() === negotiateType.toLowerCase()) {
                if (negotiateType === 'Resource') {// Extract the url from the 'Resource=uri=$1'
                    const url = /^\s*uri=([^\s]*)/.exec(authType[2]);
                    if (_(url).size() >= 2) {
                        return url[1];
                    }
                    log.info('Header: WWW-Authenticate Resource must have a uri');
                } else {
                    log.error('Unsupported authentication method');
                }
            }
        })
        .find()
        .value();
}

/**
 * Decide which auth header to use 'www-authenticate' or 'x-amzn-remapped-www-authenticate'
 * @param {string[]} headers response headers
 * @return {string|undefined}
 */
export function parseAuthenticateHeader(headers) {
    if (headers[wwwAuthenticateHeader]) {
        return wwwAuthenticateHeader;
    } else if (headers[amazonAuthenticateHeader]) {
        return amazonAuthenticateHeader;
    } else {
        log.debug(`[Authenticate] no authenticate header found from [${[wwwAuthenticateHeader, amazonAuthenticateHeader].join(' ,')}]`);
    }
}


/**
 * @param {object|Http} response An http response object
 * @param {string|'Resource'|'Basic'|'Bearer'} negotiateType type of authentication to parse for
 * @return {string|undefined} negotiate type
 */
export function getAuthenticateUri(response, negotiateType) {

    const headers = response.headers;
    const headerToUse = parseAuthenticateHeader(headers);

    if (headerToUse) {
        const resourceUri = matchNegotiateType(headers[headerToUse], negotiateType);
        if (resourceUri) {
            return resourceUri;
        }
        const fixedW3AuthHeaderValues = rewriteBrokenFirefoxHeadersAndSelect(headers, headerToUse);
        return matchNegotiateType(fixedW3AuthHeaderValues, negotiateType);
    }
}