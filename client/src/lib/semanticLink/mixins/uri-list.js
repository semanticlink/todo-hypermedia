import {makeUri} from './uri';
import {normalise} from './collection';
import * as link from 'semantic-link/lib/index';

/**
 * Returns an array of uris (aka uri-list)
 * @param {LinkedRepresentation[]|LinkedRepresentation|string|string[]} resourcesOrUris
 * @return {string[]} uriList
 * */
export const mapResourceToUriList = resourcesOrUris => {

    if (typeof resourceOrUri === 'string') {
        return [resourcesOrUris];
    } else if (Array.isArray(resourcesOrUris)) {
        return resourcesOrUris.map(resource => makeUri(resource));
    } else {
        return [makeUri(resourcesOrUris)];
    }

};


/**
 * Covert an array of linked representations (collection items) and returns their self relation uri-list
 * @param {LinkedRepresentation[]} collectionOrItems
 * @return {string[]} uri-list form of an array
 */
export const mapCollectionItemsToUriList = collectionOrItems => {
    return normalise(collectionOrItems)
        .map(item => link.getUri(item, /self|canonical/), undefined)
        .filter(item => !!item);
};



export const uriListMixins = {
    makeUriList: mapResourceToUriList,
    mapUriList: mapCollectionItemsToUriList
};