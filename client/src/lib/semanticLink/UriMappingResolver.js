/**
 * A resolving cache of key/value mapping to/from a network of data (NOD) URI to/from a document URI.
 *
 * We use this to resolve transitive references in a network of data when creating, updating and deleting.
 * For example, we are cloning a resource that is linked to another resource. In the new resource, it is not
 * linked to the original other resource but to the equivalent other. We need to be to map these going forward.
 *
 * @example
 *
 * Document: A exists, B references A
 * NOD: Create A` (hold reference A` through A), Create B` (but now B` references A, replace A with A`)
 *
 */
class UriMappingResolver {

    constructor () {
        /**
         * A simple 'document' uri map to a 'network of data' URI map.
         * @type {{}}
         */
        this.resolutionsMap = {};
    }

    /**
     * Update a mapping to/from a network of data (NOD) URI to/from a document URI.
     *
     * @param {string} documentUri
     * @param {string}  nodUri
     */
    update (documentUri, nodUri) {
        this.resolutionsMap[documentUri] = nodUri;
    }

    /**
     * Add a mapping to/from a network of data (NOD) URI to/from a document URI.
     *
     * @param {string} documentUri
     * @param {string}  nodUri
     */
    add (documentUri, nodUri) {
        this.resolutionsMap[documentUri] = nodUri;
    }

    /**
     * Signal to the resolver that a mapping is no longer relevant.
     * Remove based on the document URI a mapping to/from a network of data (NOD) URI to/from a document URI.
     *
     * @param {string} documentUri
     */
    remove (documentUri) {
        for (let key in this.resolutionsMap) {
            if (this.resolutionsMap[key] === documentUri) {
                delete this.resolutionsMap[key];
            }
        }
    }

    /**
     * Returns the network of data (NOD) URI based on a document URI or if not found itself
     *
     * @param {string} documentURI
     * @returns {*}
     */
    resolve (documentURI) {
        return this.resolutionsMap[documentURI] || documentURI;
    }

    /**
     * Helper to print out the resolutions map
     * @returns {{}} cloned version of the resolutions map
     */
    out () {
        return {...this.resolutionsMap};
    }

}

export let uriMappingResolver = new UriMappingResolver();
