/**
 * Takes an array of potentially camel cased strings and only returns those that have a dash in
 * the form
 *
 * @example
 *   [questionType, type] --> [question-type, type]
 *
 * @param {string[]} array
 * @return {string[]}
 */
export const filterCamelToDash = array =>
    (array || [])
        .filter(item => (/[A-Z]/).test(item))
        .map(item => item.replace(/([A-Z])/g, $1 => '-' + $1.toLowerCase()));

export const camelToDash = array => {

    if (typeof array === 'string') {
        return array.replace(/([A-Z])/g, $1 => '-' + $1.toLowerCase());
    }
    return array.map(item => item.replace(/([A-Z])/g, $1 => '-' + $1.toLowerCase()));
};

// TODO: just check that the dash '-' doesn't need escaping (webstorm says it doesn't)
export const dashToCamel = str => str.replace(/(-[a-z])/g, $1 => $1.toUpperCase().replace('-', ''));

/**
 * Takes a string or a Regexp and makes camel cased strings.
 *
 * @example
 *
 *      test -> test
 *      /test/ -> test
 *      /test/g -> test
 *      /create-form/ -> createForm
 *
 * @param {RelationshipType} rel
 * @returns {string}
 */
export function relTypeToCamel(rel) {

    if (typeof rel === 'string') {
        return rel;
    }

    if (rel instanceof RegExp) {
        return rel.toString()
        // remove the regexp aspects eg /test/gi -> test
            .replace(/\/[gi]*/g, '')
            // replace create-form --> createForm
            .replace(/(-[a-z])/g, $1 => $1.toUpperCase()
                .replace('-', ''));

    }

    throw new Error(`Rel type of array not parsable to be converted: '${typeof rel}'`);
}


export const linkRelMixins = {
    filterCamelToDash,
    camelToDash,
    dashToCamel,
};