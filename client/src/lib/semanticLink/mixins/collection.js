import * as link from 'semantic-link';
import SparseResource from '../SparseResource';
import {log} from 'logger';

/**
 * Takes a resource and normalises it to a valid iterable. Particularly if you hand in a resource that isn't a
 * collection then it will return an empty iterable (array).
 * @param {*|LinkedRepresentation[]|CollectionRepresentation|undefined} objOrCollection takes a collection and returns the items if available
 * @return {LinkedRepresentation[]}
 */
export const normalise = objOrCollection => {
    if (!objOrCollection) {
        return [];
    } else if (Array.isArray(objOrCollection)) {
        return objOrCollection;
    } else if (objOrCollection.items) {
        return objOrCollection.items;
    } else {
        return [];
    }
};

const isLinkedRepresentation = resource => {
    return resource && resource.links;
};

/**
 * Find a resource item in a collection identified through a found link relation or resource attribute
 * that matches an item in the collection items.
 *
 * It looks for items:
 *
 *   1. matching link relation (default: canonical or self) by uri
 *   2. field attribute (default: title) on a resource by string
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @param {string|LinkedRepresentation} resourceIdentifier
 * @param {string|string[]} attributeNameOrLinkRelation
 * @return {LinkedRepresentation}
 */
export const findItemByUriOrName = (collection, resourceIdentifier, attributeNameOrLinkRelation) => {

    // if a linked representation is handed in (instead of a string) find its self link
    if (isLinkedRepresentation(resourceIdentifier)) {
        resourceIdentifier = link.getUri(resourceIdentifier, attributeNameOrLinkRelation || /self|canonical/, undefined);
    }

    // go through the collection and match the URI against either a link relation or attribute
    return normalise(collection)
        .find(item => link.getUri(item, attributeNameOrLinkRelation || /canonical|self/, undefined) === resourceIdentifier
            || item[attributeNameOrLinkRelation || SparseResource.mappedTitle] === resourceIdentifier);
};

/**
 * Find a resource item in a collection identified through a found link relation and resource attribute
 * that matches an item in the collection items.
 *
 * It looks for items with both:
 *
 *   1. matching link relation (default: canonical or self) by uri
 *   2. field attribute (default: title) on a resource by string
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @param {LinkedRepresentation} resource
 * @param {string|RegExp|string[]|RegExp[]} rel
 * @param {string} attributeName
 * @return {LinkedRepresentation|undefined}
 */
export const findResourceInCollectionByRelAndAttribute = (collection, resource, rel = /canonical|self/, attributeName = 'name') => {

    // if its not a resource return
    if (!isLinkedRepresentation(resource)) {
        // this is not a resource
        return undefined;
    }

    const uri = link.getUri(resource, /self|canonical/, undefined);

    // if we can't find values to match against return
    if (!uri || !resource[attributeName]) {
        return undefined;
    }

    // go through the collection and match the URI against either a link relation and attribute
    return normalise(collection)
        .find(item => item[attributeName] === resource[attributeName] && link.getUri(item, rel, undefined) === uri);
};

/**
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @param {string} uri
 * @param {string|RegExp} attributeNameOrLinkRelation
 */
export const findItemByUri = (collection, uri, attributeNameOrLinkRelation) =>
    normalise(collection).find(item => link.getUri(item, attributeNameOrLinkRelation || /self|canonical/) === uri);

/**
 * Finds a resource item in a collection by taking in the item itself and then search on a URI
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @param {LinkedRepresentation} representation
 * @param {string=} attributeNameOrLinkRelation
 * @return {LinkedRepresentation}
 */
export const findResourceInCollectionByUri = (collection, representation, attributeNameOrLinkRelation) => {
    const uri = link.getUri(representation, /canonical|self/, undefined);

    if (uri) {
        // we need to add self on the end because in the case of an attribute name it won't match against self
        const itemByUri = findItemByUriOrName(collection, uri, [attributeNameOrLinkRelation, /self|canonical/]);
        if (itemByUri) {
            return itemByUri;
        }
    }
};

/**
 * Finds a resource item in a collection by taking in the item itself and then search on either (a) a URI
 * or (b) a string value in `name` attribute.
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @param {LinkedRepresentation} representation
 * @param {string=} attributeNameOrLinkRelation
 * @return {LinkedRepresentation}
 */
export const findResourceInCollection = (collection, representation, attributeNameOrLinkRelation) => {

    const itemByUri = findResourceInCollectionByUri(collection, representation, attributeNameOrLinkRelation);
    if (itemByUri) {
        return itemByUri;
    }

    const name = representation[SparseResource.mappedTitle];
    if (name) {
        const itemByName = findItemByUriOrName(collection, name, SparseResource.mappedTitle);
        if (itemByName) {
            return itemByName;
        }
    }
};

/**
 * Returns items from the first collection that are not in the second collection - matching is done via identity
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} leftCollection
 * @param {CollectionRepresentation|LinkedRepresentation[]} rightCollection
 * @param {string=} attributeNameOrLinkRelation
 * @return {LinkedRepresentation[]}
 */
export const differenceCollection = (leftCollection, rightCollection, attributeNameOrLinkRelation) => {

    // guard for the reject
    leftCollection = leftCollection || [];

    // now iterate either through: collection, an array or guard of empty array
    return normalise(leftCollection).filter(item => {
        const uri = link.getUri(item, ['canonical', 'self']);
        return !findItemByUriOrName(rightCollection, uri, [attributeNameOrLinkRelation, 'canonical', 'self']);
    });
};

/**
 * Replaces an original array with the values of another array while returning
 * the reference of the original array.
 *
 * a = (a-a) + b
 *
 * @param {*[]} array
 * @param {*[]?} values
 * @return {*[]}
 */
export const spliceAll = (array, values) => {

    if (array) {
        array.splice.apply(array, [0, array.length].concat(values));
    } else {
        log.error('Array cannot be undefined');
    }

    return array;
};

/**
 * Adds to the end of the original array the values of another array.
 *
 * a = a + b
 *
 * @param array
 * @param values
 */
export const pushAll = (array, values) => {

    if (array) {
        array.push.apply(array, values);
    } else {
        log.error('Array cannot be undefined');
    }

    return array;
};

/**
 * Adds to the end of an array if the element does not already exist
 *
 * @param array
 * @param resource
 * @param attributeNameOrLinkRelation
 */
export const pushResource = (array, resource, attributeNameOrLinkRelation) => {

    array = array || [];

    const uri = link.getUri(resource, /self|canonical/);
    if (!findItemByUri(array, uri, attributeNameOrLinkRelation)) {
        array.push(resource);
    } else {
        log.error('[Collection] Array cannot be undefined');
    }
    return array;

};

/**
 * Covert an array of linked representations (collection items) and returns their self relation uri-list
 * @param {LinkedRepresentation[]} array
 * @return {string[]} uri-list form of an array
 */
export const mapUriList = array => {
    return (array || [])
        .map(item => link.getUri(item, /self|canonical/), undefined)
        .filter(item => !!item);
};

/**
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @returns {boolean}
 */
export const isCollectionEmpty = collection => {
    return normalise(collection).length === 0;
};

/**
 *
 * @param {CollectionRepresentation|LinkedRepresentation[]} collection
 * @returns {LinkedRepresentation}
 */
export const firstItem = collection => {
    const [head,] = normalise(collection);
    return head;
};

/**
 * @mixin
 */
export const CollectionMixins = {
    findItemByUri,
    findItemByUriOrName,
    findResourceInCollection,
    findResourceInCollectionByUri,
    findResourceInCollectionByRelAndAttribute,
    differenceByUriOrName: differenceCollection, // TODO: refactor public method
    spliceAll,
    pushAll,
    pushResource,
    mapUriList,
    isCollectionEmpty,
    firstItem
};

