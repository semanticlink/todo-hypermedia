/* global console */

/**
 * @class Logger
 * @method debug
 * @method info
 * @method warn
 * @method error
 */

const LEVEL = {
    DEBUG: 'log',
    INFO: 'info',
    WARN: 'warn',
    ERROR: 'error'
};

/**
 * @extends Logger
 * A  basic Noop Console
 */
class ConsoleLogger {

    static debug () {
        log.log(LEVEL.DEBUG, arguments);
    }

    static info () {
        log.log(LEVEL.INFO, arguments);
    }

    static warn () {
        log.log(LEVEL.WARN, arguments);
    }

    static error () {
        log.log(LEVEL.ERROR, arguments);
    }

    static log (level, ...args) {
        if (typeof console !== 'undefined') {
            console[level].apply(console, ...args);
        }
    }
}

/**
 * There may be situations that the logger may not have a console object available
 */
const log = ConsoleLogger;

/**
 * @type Logger
 */
export default log;