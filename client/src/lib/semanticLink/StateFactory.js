'use strict';
import State from './State';
import SemanticLink from './SemanticLink';
import {log} from 'logger';

const stateFlagName = Symbol('state');

/**
 * TODO: can't see why this is now needed with the use of static methods and es6 imports
 * TODO: This class was a originally need in the context of es5 and angular 1x
 */
class StateFactory {

    /**
     * Make a state object ready to be added to a resource and ensuring that {@link stateFlagName}
     * keys the object.
     *
     * This is a helper function because we can't simply add the {@link State}
     * object onto the resource using the object literal notation
     *
     * @param {stateFlagEnum=} state
     * @return {{Symbol(state): State}}
     */
    static make(state) {
        const obj = {};
        obj[stateFlagName] = new State(state);
        return obj;
    }

    /**
     * Get the state object on a resource
     * @param {*} resource
     * @return {State}
     * @throws
     */
    static get(resource) {
        if (!resource) {
            throw new Error('No resource to find state on');
        }

        if (!resource[stateFlagName]) {
            const hrefOrActual = SemanticLink.tryGetUri(resource, /self|canonical/) || JSON.stringify(resource);
            throw new Error(`No state found on resource '${hrefOrActual}'`);
        }

        return resource[stateFlagName];
    }

    /**
     * Get the state object on a resource and return the default value (undefined) if not found
     * @param {*} resource
     * @param {*=undefined} defaultValue
     * @return {State|*|undefined}
     */
    static tryGet(resource, defaultValue = undefined) {
        if (!resource) {
            log.debug('[State] No resource using default');
            return defaultValue;
        }

        if (!resource[stateFlagName]) {
            log.debug('[State] No state on resource using default');
            return defaultValue;
        }

        return resource[stateFlagName];
    }

    /**
     * Takes the state object off the object (if exists)
     * @param {*} resource
     * @return {*}
     */
    static delete(resource) {
        if (resource) {
            delete resource[stateFlagName];
        }
        return resource;
    }

}

export {StateFactory};
export default StateFactory;