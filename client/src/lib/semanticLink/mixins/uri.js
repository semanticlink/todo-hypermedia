import * as link from 'semantic-link';

/**
 * Returns a uri from a string or resource
 * @param {LinkedRepresentation|string} resourceOrUri
 * @return {string} uri
 * */
export const makeUri = resourceOrUri => {
    if (typeof resourceOrUri === 'string') {
        return resourceOrUri;
    }
    return link.getUri(resourceOrUri, /canonical|self/);
};


export const uriMixins = {
    makeUri
};