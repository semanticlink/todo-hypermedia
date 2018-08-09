import * as link from 'semantic-link';

/**
 * A set of comparators for matching resources in the network of data differencer (@link NODDifferencer}
 *
 * You can also add your own. TODO: no registration, just code them in
 *
 * @example
 *
 * Specific maters for role-filters on a report template. It requires that both the role and the filter match
 * from the link relations:
 *
 * {
 *
 *    "links": [
 *        {
 *            "rel": "self",
 *            "href": "http://localhost:1080/role/filter/408"
 *        },
 *        {
 *            "rel": "up",
 *            "href": "http://localhost:1080/report/template/4991"
 *        },
 *        {
 *            "rel": "filter",
 *            "href": "http://localhost:1080/filter/1"
 *        },
 *        {
 *            "rel": "role",
 *            "href": "http://localhost:1080/role/11"
 *
 *    ]
 * }
 *
 *  byLinkRelation(lvalue, rvalue) {
 *      return link.matches(lvalue, /^role$/) === link.matches(rvalue, /^role$/) &&
 *          link.matches(lvalue, /^filter$/) === link.matches(rvalue, /^filter$/);
 *  }
 *
 */
export default class Comparator {

    /**
     * Match on the canonical or self link relation on the resources
     *
     * @param {LinkedRepresentation} lvalue
     * @param {LinkedRepresentation} rvalue
     * @return {boolean}
     */
    static canonicalOrSelf (lvalue, rvalue) {
        const lUri = link.getUri(lvalue, /self|canonical/);
        const rUri = link.getUri(rvalue, /self|canonical/);
        return !!lUri && lUri === rUri;
    }

    /**
     * Simple match on the name attribute on the resources
     *
     * @param {Link} lvalue
     * @param {Link} rvalue
     * @return {boolean}
     * @deprecated use title, older implementations required name for backwards compatibility
     */
    static name (lvalue, rvalue) {
        return !!lvalue.name && lvalue.name && lvalue.name === rvalue.name;
    }

    /**
     * Simple match on the title attribute on the resources
     *
     * @param {Link} lvalue
     * @param {Link} rvalue
     * @return {boolean}
     */
    static title (lvalue, rvalue) {
        return !!lvalue.title && lvalue.title && lvalue.title === rvalue.title;
    }
}

export { Comparator };