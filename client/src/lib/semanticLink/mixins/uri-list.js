import {makeUri} from './uri';

/**
 * Returns an array of uris (aka uri-list)
 * @param {LinkedRepresentation[]|LinkedRepresentation|string|string[]} resourcesOrUris
 * @return {string[]} uriList
 * */
export const makeUriList = resourcesOrUris => {

    if (typeof resourceOrUri === 'string') {
        return [resourcesOrUris];
    } else if (Array.isArray(resourcesOrUris)) {
        return resourcesOrUris.map(resource => makeUri(resource));
    } else {
        return [makeUri(resourcesOrUris)];
    }

};

export const uriListMixins = {
    makeUriList,
};