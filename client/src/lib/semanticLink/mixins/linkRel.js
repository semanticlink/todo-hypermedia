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


export const linkRelMixins = {
    filterCamelToDash,
    camelToDash,
    dashToCamel,
};