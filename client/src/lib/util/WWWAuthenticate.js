import {_} from '../semanticLink';
import {log} from 'logger';

const wwwAuthenticateHeader = 'www-authenticate';

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
 * @constructor
 */
class WWWAuthenticate {

    constructor() {
        /**
         * The list of known headers in lower case.
         *
         * @type {string[]}
         * @readonly
         */
        this.knownHeaders = [
            'location',
            'accept',
            wwwAuthenticateHeader,
            'content-type'
        ];
    }

    /**
     * Firefox managed to corrupt the headers when there are multiple headers
     * with the same value. It appears to loose the header name off all
     * but the first entry, and then re-parses the result so that the
     * header value is a random value up to the first colon in the header value.
     *
     * Chrome supports it just fine.
     * @private
     */
    rewriteBrokenFirefoxHeadersAndSelect(headers, selectHeader) {
        let lastKnownHeader; // starts out unknown
        const knownHeaders = this.knownHeaders;

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
     * Negotiate, Basic realm="cemplicity.com", Resource uri="/Authenticator", Bearer realm="cemplicity.com"
     *
     * @param {string} headerValues www-authenticate header values as a command separated string
     * @param {string|'Resource'|'Basic'|'Bearer'} negotiateType type of authentication to  parse for
     * @private
     * @return {string|undefined} negotiate type
     */
    matchNegotiateType(headerValues, negotiateType) {
        negotiateType = negotiateType || 'Resource';

        // The split won't pull out additional whitespace. Return the first result
        // the matches the requested type.
        return _(headerValues.split(','))
            .chain()
            .map(function (type) {
                const authType = /^\s*(\w+)\s+(.*)/.exec(type);
                if (_(authType).size() >= 2 && authType[1].toLowerCase() === negotiateType.toLowerCase()) {
                    switch (negotiateType) {
                        case 'Resource':
                            // Extract the url from the 'Resource=uri=$1'
                            const url = /^\s*uri=([^\s]*)/.exec(authType[2]);
                            if (_(url).size() >= 2) {
                                return url[1];
                            }
                            log.info('Header: WWW-Authenticate Resource must have a uri');
                            break;
                        default:
                            log.error('Unsupported authentication method');
                            break;

                    }
                }
            })
            .find()
            .value();
    }

    /**
     *
     * @param {object|Http} response An http response object
     * @param {string|'Resource'|'Basic'|'Bearer'} negotiateType type of authentication to parse for
     * @return {string|undefined} negotiate type
     */
    getAuthenticateUri(response, negotiateType) {

        const w3AuthHeaderValues = response.headers[wwwAuthenticateHeader];
        if (w3AuthHeaderValues) {
            const resourceUri = this.matchNegotiateType(w3AuthHeaderValues, negotiateType);
            if (resourceUri) {
                return resourceUri;
            }
        }
        const fixedW3AuthHeaderValues = this.rewriteBrokenFirefoxHeadersAndSelect(response.headers, wwwAuthenticateHeader);
        return this.matchNegotiateType(fixedW3AuthHeaderValues, negotiateType);
    };

}

export default WWWAuthenticate;

export let wwwAuthenticate = new WWWAuthenticate();
