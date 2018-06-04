import { log } from './logger';

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
        case 'http://types/text':
            return 'text';
        case 'http://types/text/passwprd':
            return 'password';
        case 'http://types/text/email':
            return 'email';
        case 'http://types/check':
            return 'text';
        case 'http://types/date':
        case 'http://types/date/time':
            return 'date';
        default:
            log.warn(`Form type not found: '${type}'`)
            return 'text';
    }

};

export { mapApiToUiType };
