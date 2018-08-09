import _ from 'underscore';

/**
 *
 * @param {*[]|LinkedRepresentation[]} resource
 * @return {*[]} copy detached
 */
export const detach = resource => {
    if (!resource) {
        return [];
    } else if (resource.items) {
        return resource.items.map(item => Object.assign({}, item));
    } else if (Array.isArray(resource)) {
        return resource.map(item => Object.assign({}, item));
    } else {
        return [];
    }
};

// TODO: just check that the dash '-' doesn't need escaping (webstorm says it doesn't)
export const dashToCamel = str => str.replace(/(-[a-z])/g, $1 => $1.toUpperCase().replace('-', ''));

export function extendResource(obj) {
    // stolen from https://gist.github.com/kurtmilam/1868955
    let source,

        isAssign = (oProp, sProp) => (_.isUndefined(oProp) || _.isNull(oProp) || _.isNull(sProp) || _.isDate(sProp)),

        procAssign = (oProp, sProp, propName) => {
            // Perform a straight assignment
            // Assign for object properties & return for array members
            obj[propName] = _(sProp).clone();
            return obj[propName];
        },

        hasObject = (oProp, sProp) => (_.isObject(oProp) || _.isObject(sProp)),

        procObject = (oProp, sProp, propName) => {
            // extend oProp if both properties are objects
            if (!_.isObject(oProp) || !_.isObject(sProp)) {
                throw new Error('Trying to combine an object with a non-object (' + propName + ')');
            }
            // Assign for object properties & return for array members
            obj[propName] = extendResource(oProp, sProp);
            return obj[propName];
        },

        procMain = (propName) => {
            const oProp = obj[propName],
                sProp = source[propName];

            // The order of the 'if' statements is critical

            // Cases in which we want to perform a straight assignment
            if (isAssign(oProp, sProp)) {
                procAssign(oProp, sProp, propName);
            }
            // At least one property is an object
            else if (hasObject(oProp, sProp)) {
                procObject(oProp, sProp, propName);
            }
            // Everything else
            else {
                // Let's be optimistic and perform a straight assignment
                procAssign(oProp, sProp, propName);
            }
        },

        procAll = (src) => {
            source = src;
            Object.keys(source).forEach(procMain);
        };

    _.each(Array.prototype.slice.call(arguments, 1), procAll);

    return obj;
}

/**
 * Merge a resource with a new document returning the existing resource with only fields updated in the
 * fields (whitelist) from the document.
 *
 * @param resource
 * @param document
 * @param fields
 * @return {*} containing a document
 */
export const mergeByFields = (resource, document, fields) => {
    let documentToMerge = _(document).pick((key, value) => _(fields).contains(value));
    // do a deep merge into a new document
    return extendResource({}, resource, documentToMerge);
};

export const compactObject = resource => _(resource).omit(val => {
    return !!(_.isObject(val) && _.isEmpty(val)) || _.isUndefined(val);
});

export const RepresentationMixins = {
    detach,
    dashToCamel,
    extendResource,
    mergeByFields,
    compactObject,
};