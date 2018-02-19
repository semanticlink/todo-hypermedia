import { log } from '../semanticLink/Logger';

let instance;

/**
 * This is trivial proof-of-concept implementation of client
 * side URI mapping.
 *
 * It is known NOT to work if the api URI is not prefixed
 * with the base api uri (which will happen if the api supports
 * distributed serving of content from multiple Uris).
 *
 * @example
 *
 *  clientUri: https://app.example.com
 *  apiUri:    https://api.example.com
 *
 *  We are currently on the the "home" page for a user who is tenanted to "4",
 *  thus we would see in the browser location a simplified version that is human readable.
 *
 *  --> https://app.example.com/home/a/tenant/4
 *
 *  This location is "stateless" because it contains enough information to reconstruct
 *  client-side state for the user. In long form, it is actually:
 *
 *  --> https://app.example.com/home/a/https://api.example.com/tenant/4
 *
 *  This maps to:
 *
 *  client-side view in router:  /home/a/ --> eg Home (as a component/view)
 *  api: it will fetch https://api.example.com/tenant/4
 *
 *  Two things about the routing:
 *
 *  1. '/home/a/' uses the convention of '/a/' so that we have a small delimiter to
 *     be able to pivot between the client and api state
 *
 *  2. Because routing is regex-based, we need to extend the syntax out to capture a
 *     parameter that is the api uri
 *
 *     --> /home/a/:apiUri
 *
 *  The result is that we need to read routing as:
 *
 *    ---> /[client-side view]/a/[apiUri]
 *
 *  The problem this class solves is the mapping between the entry and
 *  exit from clients-side views. We need to intercept at point marked ** below:
 *
 *  Entry:
 *
 *      1. Incoming Uri
 *      2. Router matches route, spread across params
 *      3. ** need to expand apiUri
 *      4. Consume in view (eg hydrate network of data)
 *
 * @example
 *
 *      https://client.example.com/home/a/tenant/4
 *
 *      beforeEntry(to...){
 *          { apiUri: makeAbsolute(to.params.apiUri)
 *      }
 *
 *      View has apiUri: https://api.example.com/tenant/4 to retrieve
 *
 *  Exit:
 *
 *      1. Outgoing link (eg route.$go)
 *      2. ** Need to construct Uri to include contracted apiUri
 *      3. Hand back off to router to match entry above
 *
 *  @example
 *
 *       User needs to be redirected to their home page and we
 *       know that they are https://api.example.com/tenant/4
 *
 *       route.go(toSitePath("http://api.example.com/tenant/4", "/home/a"))
 *
 * Other limitations. --Most-- All implementations client-side routers do not
 * deal with the issue of server-side (api) state as part of client state. This
 * implementation is basic, but works. These examples are independent of router
 * implementations (Vue, Angular) and could be further extracted.
 *
 * This class is implemented as a singleton. It allows for straightforward
 * injection in es6 code. This is particularly used in routers (eg vue-router)
 * and rather than use an independent IOC container (eg vue-injector) we
 * can use the es6 imports. This should simplify the code.
 *
 * This class will need to be instantiated with the Api URI once the DOM
 * has loaded from the link relation.
 *
 * @class UriMapping
 * @param {string} clientUri uri of serving out the client application (html)
 * @param {string} apiUri uri of the api being consumed by the  client (ie known link rel)
 */
class UriMapping {
    constructor (clientUri, apiUri) {

        if (!instance) {
            this.initialise(clientUri, apiUri);
            instance = this;
        }

        return instance;
    }

    /**
     * Used to (re)set the client and api Uri values. This is rarely used in an application setting. But
     * it is useful for tests.
     *
     * @param {string} clientUri uri of serving out the client application (html)
     * @param {string} apiUri uri of the api being consumed by the  client (ie known link rel)
     */
    initialise (clientUri, apiUri) {
        this.clientUri = clientUri;
        this.apiUri = apiUri;
    }

    /**
     * Remove the scheme and the authority and just return the path.
     *
     * @param {string} anApiUri a URI in the api namespace
     * @param {string} sitePrefix the client side routing part of the URL
     * @return {string} path from a uri
     */
    toSitePath (anApiUri, sitePrefix) {
        const apiPathSuffix = anApiUri.replace(this.apiUri, '');
        if (apiPathSuffix) {
            const sitePrefixWithoutTrailingSlash = sitePrefix.replace(/\/$/, '');
            if (/^\//.test(apiPathSuffix)) {
                return sitePrefixWithoutTrailingSlash + apiPathSuffix;
            } else {
                return sitePrefixWithoutTrailingSlash + '/' + apiPathSuffix;
            }
        } else {
            return sitePrefix;
        }
    }

    /**
     * Takes an Api Uri and strips the original Api Uri and ensures that it doesn't
     * have a leading slash
     *
     * @param {string} anApiUri absolute uri
     * @return {string} relative path or empty if nothing
     */
    makeRelative (anApiUri) {
        const apiPathSuffix = anApiUri.replace(this.apiUri, '');
        if (apiPathSuffix) {
            if (/^\//.test(apiPathSuffix)) {
                return apiPathSuffix.substring(1);
            } else {
                return apiPathSuffix;
            }
        } else {
            return '';
        }
    }

    /**
     * Converts a path to an absolute Uri based on the Api Uri.
     *
     * TODO: write test
     * @param {string} path absolute or relative path (with or without the api in the scheme/authority)
     * @return {string} an absolute Uri with a trailing slash
     */
    makeAbsolute (path) {
        const pathWithLeadingSlash = path.replace(/^\//, '');
        if (/\/$/.test(this.apiUri)) {
            return this.apiUri + pathWithLeadingSlash;
        } else {
            return this.apiUri + '/' + pathWithLeadingSlash;
        }
    }

    /**
     *
     * TODO: write test
     * @param {string} path absolute or relative path
     * @param {string} sitePrefix the client side routing part of the URL
     * @return {string} an API URI
     */
    fromSitePath (path, sitePrefix) {
        if (/\/$/.test(this.apiUri)) {
            if (/\/$/.test(sitePrefix)) {
                return this.apiUri + path.replace(sitePrefix, '');
            } else {
                return this.apiUri + path.replace(sitePrefix + '/', '');
            }
        } else {
            // no trailing slash on apiUri, make a Uri with a trailing
            // slash, with the prefix and the path
            return makeAbsolute(path.replace(sitePrefix, ''));
        }
    }

}

/*
 * Note the exports below should help us write simpler code even though there is a duplication in
 * the form of wrappers.
 *
 * These allow us function calls as imports on the singleton
 *
 * @example import {toSitePath, makeRelative } from '/UriMapping'
 */

export function toSitePath (anApiUri, sitePrefix) {
    if (instance) {
        return instance.toSitePath(anApiUri, sitePrefix);
    }
    log.error(`Mapper is not initialised. Instantiate first before calling this method`);
}

export function makeRelative (anApiUri) {
    if (instance) {
        return instance.makeRelative(anApiUri);
    }
    log.error(`Mapper is not initialised. Instantiate first before calling this method`);
}

export function makeAbsolute (path) {
    if (instance) {
        return instance.makeAbsolute(path);
    }
    log.error(`Mapper is not initialised. Instantiate first before calling this method`);
}

export function fromSitePath (path, sitePrefix) {
    if (instance) {
        return instance.fromSitePath(path, sitePrefix);
    }
    log.error(`Mapper is not initialised. Instantiate first before calling this method`);
}

/**
 *
 * @param {string} clientUri uri of serving out the client application (html)
 * @param {string} apiUri uri of the api being consumed by the  client (ie known link rel)
 * @return {UriMapping} singleton instance of UriMapping
 */
export function uriMapping (clientUri, apiUri) {
    new UriMapping(clientUri, apiUri);
    return instance;
}

export default UriMapping;

