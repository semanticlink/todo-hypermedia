/**
 * This is a documentation file
 */
/**
 *     *******************************************************************
 *     *                                                                 *
 *     *     This is a JSDoc documentation stub. Don't delete it.        *
 *     *                                                                 *
 *     *******************************************************************
 */

/**
 * A link representation.
 *
 * @class Link
 * @property {string} rel
 * @property {string} href
 * @property {string} type
 * @property {string} title
 */

/**
 * A representation of a resource with links.
 *
 * @class LinkedRepresentation
 * @property {Link[]} links
 */

/**
 * A representation of a collection of resources). The links
 * may contain links to 'next', 'previous', 'first' and 'last'.
 *
 * @class CollectionRepresentation
 * @extends LinkedRepresentation
 *
 * @property {LinkedRepresentation[]} items
 */

/**
 * A representation of a feed resource (i.e. a collection of resources). The links
 * may contain links to 'next', 'previous', 'first' and 'last'.
 *
 * @class FeedRepresentation
 * @extends LinkedRepresentation
 *
 * @property {FeedItemRepresentation[]} items WARNING: This field is used for both
 *   the wire level value as a {@link FeedItemRepresentation} and the in memory (client) representation as the
 *   resource of the given type with a base type of {@link LinkedRepresentation}
 */

/**
 * A representation of a resource with links.
 *
 * @class FeedItemRepresentation
 * @property {string} id Uri of the resource in the collection
 * @property {?string} title Optional title of the resource
 */

/**
 *
 * @class FormItemRepresentation
 * @property {string} id
 * @property {string} type
 * @property {string} name
 * @property {string} description
 * @property {FormItemRepresentation[]} items
 */

/**
 *
 * @class GroupFormItemRepresentation
 * @extends FormItemRepresentation
 * @property {FormItemRepresentation[]} items
 */

/**
 * A form used to describe the format of a representation for perform a POST.
 *
 * @class FormRepresentation
 * @extends LinkedRepresentation
 * @property {FormItemRepresentation[]} items
 */