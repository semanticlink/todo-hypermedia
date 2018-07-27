/* global console */

/**
 * @class Logger
 * @method debug
 * @method info
 * @method warn
 * @method error
 */

/**
 *
 * @type {{DEBUG: {console: string, level: number}, INFO: {console: string, level: number}, WARN: {console: string, level: number}, ERROR: {console: string, level: number}}} LogLevel
 */
const LEVEL = {
    DEBUG: {
        console: 'debug',
        level: 1
    },
    INFO: {
        console: 'info',
        level: 2
    },
    WARN: {
        console: 'warn',
        level: 3
    },
    ERROR: {
        console: 'error',
        level: 4
    }
};

/**
 * Default level is info and above
 * @type {number}
 */
let showLevel = LEVEL.INFO;

/**
 *
 * @param {LogLevel} level
 */
function setLogLevel(level) {
    showLevel = level;
}

/**
 * @extends Logger
 * A  basic Noop Console
 */
class ConsoleLogger {

    static debug() {
        log.log(LEVEL.DEBUG, arguments);
    }

    static info() {
        log.log(LEVEL.INFO, arguments);
    }

    static warn() {
        log.log(LEVEL.WARN, arguments);
    }

    static error() {
        log.log(LEVEL.ERROR, arguments);
    }

    /**
     * @private
     * @param level
     * @param args
     */
    static log(level, ...args) {
        if (typeof console !== 'undefined' && level.level >= showLevel.level) {
            console[level.console].apply(console, ...args);
        }
    }
}

/**
 * There may be situations that the logger may not have a console object available
 */
const log = ConsoleLogger;

export { setLogLevel, LEVEL };
/**
 * @type Logger
 */
export default log;