/**
 * @fileOverview
 * A utility class for manipulating a list of links that form a semantic interface to a resource.
 *
 * @return {Object} A view
 *
 * Example data of a collection Resource object with a array called links:
 *
 * <code>
 * {
 *  links: [
 *    { rel: "collection" type: "application/json", href: "http://localhost/orders/" },
 *    { rel: "self" type: "application/json", href: "http://localhost/orders/" },
 *    { rel: "item", type: "application/json", href: "http://localhost/orders/1"},
 *    { rel: "first", type: "application/json", href: "http://localhost/orders/1"},
 *    { rel: "item", type: "application/json", href: "http://localhost/orders/2"},
 *    { rel: "item", type: "application/json", href: "http://localhost/orders/3"},
 *    { rel: "last", type: "application/json", href: "http://localhost/orders/3"},
 *   ]
 * }
 * </code>
 *
 * Parameters
 * ==========
 *
 * The methods of this object tend to use the following parameters:
 *
 * links
 * -----
 *
 * This is the first parameter to most methods on this object. It
 * is an object with some form of semantic interface, that contains
 * links. The supported forms of this parameter are:
 * - the `<head>` element of a html DOM
 * - the magic identifier `HEAD` (as a synonym for the <head> element)
 * - an array of link objects with `rel`, `type` and `href` values
 * - an object with a `links` object which an array of link objects
 *
 * relationshipType
 * ----------------
 *
 * This parameter is a well known (or custom) relationship type.
 * e.g `collection`, `first`, `self`, `item`
 *
 * The relation type can be:
 * - an exact matching string
 * - a magic wildcard string `*`
 * - a regular expression
 *
 * mediaType
 * ---------
 *
 * This parameter is a well known mime type. e.g. `application/json`, `text/html` etc.
 *
 * The relation type can be:
 * - an exact matching string
 * - a magic wildcard string `*`
 * - a regular expression
 *
 * @see https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest
 */

// TODO: remove this dependency (two easy places to remove)
import $ from 'jquery';
import _ from './mixins/index';
import axios from 'axios';
import log from './Logger';

class SemanticLink {

    /**
     *
     * @param {Axios} http
     * @param {Logger} log
     */
    constructor (http, log) {
        this.log = log;
        this.http = http;
    }

    /**
     * A helper to determine if an object is a promise. WARNING: This will return
     * false if a deferred style object is provided.
     *
     * @param {*} obj
     * @return {boolean}
     * @private
     */
    static isPromiseLike (obj) {
        return obj && _(obj.then).isFunction();
    }

    /**
     * A helper to determine if an object is a deferred.
     *
     * @param {*} obj
     * @return {boolean}
     * @private
     */
    static isDeferredLike (obj) {
        return obj && !SemanticLink.isPromiseLike(obj) && _(obj.resolve).isFunction();
    }

    /**
     * Map the list of child <link> elements from the given DOM element into simple link objects.
     *
     * @param {Element} element
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string} mediaType
     * @return {Link[]} an array of links that match
     * @private
     */
    static filterDom (element, relationshipType, mediaType) {
        const links = $(element)
            .find('link')
            .filter('link[href][rel]')
            .map((index, link) => ({
                href: link.href,
                rel: link.rel,
                type: link.type
            }))
            .get();
        return SemanticLink.filterLinks(links, relationshipType, mediaType);
    }

    /**
     * Map the list of child <link> elements from the given JSON representation.
     *
     * @param {LinkedRepresentation} representation
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string} mediaType
     * @return {Link[]} an array of links that match
     * @private
     */
    static filterRepresentation (representation, relationshipType, mediaType) {
        if (!_(representation).isUndefined() && _.contains(_.keys(representation), 'links')) {
            return SemanticLink.filterLinks(representation.links, relationshipType, mediaType);
        }
        return []; // No links member on the object, so nothing matches
    }

    //  Public interface methods
    //  ========================

    /**
     * A utility helper function to match a relationship type of media type string
     *
     * Match a link string if:
     *   a regular  expression is used
     *   the string is a special case wildcard string of '*'
     *   the string matches the link string
     *
     * @param {string} linkString
     * @param {string|RegExp} matchString
     * @return {boolean}
     * @private
     */
    static matchParameter (linkString, matchString) { //todo Use the function in LinkedRepresentation
        return (linkString &&
            _(matchString).isRegExp() &&
            linkString.match(matchString)) ||
            matchString === '*' ||
            matchString === '*/*' ||
            linkString === '*/*' ||
            linkString === matchString;
    }

    /**
     * Get an array of links that match the given relationship type and media type,
     * where the link has a valid href.
     *
     * If the relationship types is an array then the matching types will be retuned
     * in array order.
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipTypes
     * @param {?string} mediaType
     * @return {Array.<Link>} an array of links that match
     * @private
     */
    static filterLinks (links, relationshipTypes, mediaType) { //todo Use the function in LinkedRepresentation
        return _(_(relationshipTypes).isArray() ? relationshipTypes : [relationshipTypes])
            .chain()
            .map(relationshipType => {
                if (_.isArray(links)) {
                    return _.filter(links, link => {
                        const linkKeys = _.keys(link);
                        if (_(linkKeys).contains('href')) {
                            if (_(linkKeys).contains('rel')) {
                                if (!SemanticLink.matchParameter(link.rel, relationshipType)) {
                                    return false; // relationship type doesn't match
                                }
                            }
                            if (_(linkKeys).contains('type')) {
                                if (!SemanticLink.matchParameter(link.type, mediaType)) {
                                    return false; // media type doesn't match
                                }
                            }
                            return true; // it seems to match, and it has an url.
                        }
                        return false; // no match;
                    });
                }
                // log.warn('Array input expected - filterLinks');
                return []; // No links match the filter requirements.
            })
            .flatten()
            .value();
    }

    /**
     * Query whether the 'links' has one or more link elements that match the given criteria.
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {String=} mediaType
     * @return {Boolean} Whether there is one or more matching links
     */
    static matches (links, relationshipType, mediaType) {
        return !_(SemanticLink.filter(links, relationshipType, mediaType)).isEmpty();
    }

    /**
     * Filter the list of links based on a relationship type and media type.
     * The result is an array of links objects.
     *
     * The results are not sorted. When multiple link entries are matched
     * then the order should not be assumed.
     *
     * Given a set of links (which can be in several forms), generate a
     * list of filtered links that match the given relation type and media type
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} arg
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {String=} mediaType
     * @return {Array.<Link>} an array of links that match
     */
    static filter (arg, relationshipType, mediaType) {
        mediaType = mediaType || '*/*';

        if (_(arg).isArray()) {
            // filter an array of JSON link objects
            return SemanticLink.filterLinks(arg, relationshipType, mediaType);

        } else if (arg === 'HEAD') {
            // Filter 'link' elements from the 'head' element of the DOM, this is a
            // shortcut method so the caller doesn't have to express "$('HEAD')[0]"
            return SemanticLink.filterDom($('head')[0], relationshipType, mediaType);

        } else if (_(arg).isElement()) {
            // Filter 'link' elements from the DOM
            return SemanticLink.filterDom(arg, relationshipType, mediaType);

        } else {
            // Filter based on a representation with an array on 'links'
            return SemanticLink.filterRepresentation(arg, relationshipType, mediaType);
        }
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @param { function(string, string, string):string|function():string } failAction a default action when the link is not present
     * @return {string} The uri of the relationship
     */
    static getUrl (links, relationshipType, mediaType, failAction) {
        const candidateLinks = SemanticLink.filter(links, relationshipType, mediaType);
        if (!_(candidateLinks).isEmpty()) {
            return _(candidateLinks).first().href;
        } else {
            return failAction(links, relationshipType, mediaType);
        }
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string} relationshipType
     * @param {string=} mediaType
     * @return {string}
     */
    static getTitle (links, relationshipType, mediaType) {
        const candidateLinks = SemanticLink.filter(links, relationshipType, mediaType);
        if (!_(candidateLinks).isEmpty()) {
            return _(candidateLinks).first().title;
        } else {
            // logError(links, relationshipType, mediaType);
            return '';
        }
    }

    /**
     * Get the first 'href' that matches the filter criteria.
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @return {string} The URI
     */
    static getUri (links, relationshipType, mediaType) {
        return SemanticLink.getUrl(links, relationshipType, mediaType, () => {
            //logError(links, relationshipType, mediaType);
            return '';
        });
    }

    /**
     * Get the first 'href' that matches the filter criteria, or return undefined if there is no match
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @param defaultValue
     * @return {?string}
     */
    static tryGetUri (links, relationshipType, mediaType, defaultValue = undefined) {
        return SemanticLink.getUrl(links, relationshipType, mediaType, () => defaultValue);
    }

    /**
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {?string} mediaType
     * @return {String}
     * @private
     */
    static makeNotFoundMessage (links, relationshipType, mediaType) {
        const allLinks = SemanticLink.filter(links, '*', '*');
        let mediaTypeDetails = ' (' + mediaType + ')';
        if (!mediaType) {
            mediaTypeDetails = '';
        }

        return 'The semantic interface \'' + relationshipType + '\'' + mediaTypeDetails + ' is not available. ' +
            allLinks.length + ' available links include ' +
            _(allLinks)
                .map(link => {
                    if (link.type && link.type !== '*' && link.type !== '*/*') {
                        return '"' + link.rel + '" (' + link.type + ')';
                    } else if (link.rel && link.rel.match(/self|canonical/)) {
                        return '"self: ' + link.href + '"';
                    } else {
                        return '"' + link.rel + '"';
                    }
                })
                .join(', ');
    }

    /**
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {?string} mediaType
     * @private
     */
    logError (links, relationshipType, mediaType) {
        if (_(links).isNull() || _(links).isUndefined()) {
            this.log.error('Null or invalid object provided with semantic links information');
        } else {
            this.log.error(SemanticLink.makeNotFoundMessage(links, relationshipType, mediaType));
        }
    }

    /**
     * HTTP/xhr utilities that wrap http. It particularly ensures `Accept` headers are set.
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string} mediaType
     * @param {string} verb (GET|PUT|POST|DELETE)
     * @param {*} data
     * @param {?Promise=} cancellable
     * @return {Promise}
     * @private
     */
    link (links, relationshipType, mediaType, verb, data, cancellable) {
        let link = _(SemanticLink.filter(links, relationshipType, mediaType)).first();
        if (link && link.href) {
            return this.http(Object.assign({},
                {
                    method: verb,
                    url: link.href,
                    timeout: cancellable || 0,
                    data: data,
                },
                data && (verb === 'DELETE' || verb === 'POST') && mediaType ? {headers: {'Content-Type': mediaType}} : {}))
                /**
                 * Currently axios is our library and it throws a {@link AxiosPromise.<T>} which includes a request, response
                 * and config. We will only return the response to work with implementation that from older http requests.
                 */
                .catch(err =>{
                    throw err.response;
                });
        } else {
            this.logError(links, relationshipType, mediaType);
            return Promise.reject('The resource doesn\'t support the required interface');
        }
    }

    /**
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {?string} mediaType
     * @param {string} verb (GET|PUT|POST|DELETE)
     * @param {*} data
     * @param {?Promise} cancellable
     * @param {*} defaultValue The data to return if the named relationship isn't available
     * @return {Promise}
     * @private
     */
    tryLink (links, relationshipType, mediaType, verb, data, cancellable, defaultValue) {
        const link = _(SemanticLink.filter(links, relationshipType, mediaType)).first();
        if (link && link.href) {
            return this.http(Object.assign({},
                {
                    method: verb,
                    url: link.href,
                    timeout: cancellable || 0,
                    data: data,
                },
                data && verb === 'DELETE' && mediaType ? {headers: {'Content-Type': mediaType}} : {}));
        } else {
            return Promise.resolve({
                status: 200,
                headers: [],
                data: defaultValue
            });
        }
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {(string|RegExp)} relationshipType
     * @param {string=} mediaType
     * @param {?Promise} cancellable
     * @return {Promise} a promise
     */
    get (links, relationshipType, mediaType, cancellable) {
        // The mediaType parameter is optional - add it in as undefined
        if (SemanticLink.isPromiseLike(mediaType) || SemanticLink.isDeferredLike(mediaType)) {
            Array.prototype.splice.call(arguments, 2, 0, undefined);
            return this.get.apply(this.get, arguments);
        }
        // TODO: retire deferred
        if (SemanticLink.isDeferredLike(cancellable)) {
            this.log.error('Expected a promise but got a deferred');
        }
        return this.link(links, relationshipType, mediaType, 'GET', null, cancellable);
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @param {?Promise} cancellable
     * @param {*} defaultValue the value to return if the link is not present
     * @return {*}
     */
    tryGet (links, relationshipType, mediaType, cancellable, defaultValue) {
        // The mediaType parameter is optional - add it in as undefined
        if (SemanticLink.isPromiseLike(mediaType) || SemanticLink.isDeferredLike(mediaType)) {
            Array.prototype.splice.call(arguments, 2, 0, undefined);
            return this.tryGet.apply(this.get, arguments);
        }
        // TODO: retire deferred
        if (SemanticLink.isDeferredLike(cancellable)) {
            this.log.error('Expected a promise but got a deferred');
        }
        return this.tryLink(links, relationshipType, mediaType, 'GET', null, cancellable, defaultValue);
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string} mediaType
     * @param {*} data
     * @return {Promise}
     */
    put (links, relationshipType, mediaType, data) {
        return this.link(links, relationshipType, mediaType, 'PUT', data);
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @param {*} data
     * @return {Promise}
     */

    post (links, relationshipType, mediaType, data) {
        return this.link(links, relationshipType, mediaType, 'POST', data);
    }

    /**
     *
     * @param {(string|Element|Link[]|LinkedRepresentation)} links
     * @param {string|RegExp|string[]|RegExp[]} relationshipType
     * @param {string=} mediaType
     * @param {*=} data
     * @return {Promise}http://localhost:8000/client/src/index.html#/survey/specification/a/survey/3209
     */
    delete (links, relationshipType, mediaType, data) {
        return this.link(links, relationshipType, mediaType, 'DELETE', data);
    }

}

let link = new SemanticLink(axios, log);

export { link, SemanticLink };
export default SemanticLink;