import _ from '../mixins/index';

/**
 * Internal data structure for working out which active to perform on documents.
 *
 * ** DO NOT DELETE** this is the jsdoc
 *
 * @class SyncInfo
 * @property {LinkedRepresentation} resource
 * @property {*} document
 * @property {string} action containing 'create','update', 'delete'
 * @private
 */

/**
 * @class SyncResolver
 * @property  resolve
 * @property  remove
 * @property  add
 * @property  update
 */

/**
 * Used for provisioning the resources (network of data) based on providing new document (resources). Based
 * on a difference set this class synchronises between the client version and the api
 */


/**
 * @return {SyncResolver}
 */
export const defaultResolver = {
    resolve: u => u,
    remove: _.noop,
    add: _.noop,
    update: _.noop
};


