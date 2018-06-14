import { log } from './logger';

/**
 * Text known input type from the server
 * @type {string}
 */
const TEXT = 'http://types/text';

/**
 * Maps the representation types to the known types that can be rendered (input not select at this stage)
 * @see https://bootstrap-vue.js.org/docs/components/form-input
 * TODO: move to util
 *
 * TODO:
 *   types: [ 'text', 'password', 'email', 'number', 'url', 'tel', 'date', `time`, 'range', 'color' ]
 *
 * TODO: implement mapping based on agent
 *
 *      Caveats with input types:
 *      - Not all browsers support all input types, nor do some types render in the same format across browser types/version.
 *      - Browsers that do not support a particular type will fall back to a text input type. As an example, Firefox desktop doesn't support date, datetime, or time, while Firefox mobile does.
 *      - Chrome lost support for datetime in version 26, Opera in version 15, and Safari in iOS 7. Instead of using datetime, since support should be deprecated, use date and time as two separate input types.
 *      - For date and time style input, where supported, the displayed value in the GUI may be different than what is returned by its value.
 *      - Regardless of input type, the value is always returned as a string representation.
 *
 * @param {string} type     * @returns {string}
 */
const mapApiToUiType = type => {
    switch (type) {
        case TEXT:
            return 'text';
        case 'http://types/text/password':
            return 'password';
        case 'http://types/text/email':
            return 'email';
        case 'http://types/check':
            return 'check';
        case 'http://types/date':
            return 'date';
        case 'http://types/date/time':
            return 'datetime';
        case 'http://types/select':
            return 'select';
        default:
            log.warn(`Form type not found: '${type}'`);
            return 'text';
    }

};

/**
 * Maps the completed flag (display  helper) to a state of the todo.
 *
 * This shows how creating batch processing in this way the client starts to know too much. We could, however, increasing
 * loosely couple this by at least retrieving this information from the server.
 *
 * @param bool
 * @returns {string}
 */
const mapCompletedToState = bool => {
    return bool
        ? 'http://example.com/todo/state/complete'
        : 'http://example.com/todo/state/open';
};

const mapStateToCompleted = state => {
    return state === 'http://example.com/todo/state/complete';
};

export { mapApiToUiType, mapCompletedToState, mapStateToCompleted, TEXT };
