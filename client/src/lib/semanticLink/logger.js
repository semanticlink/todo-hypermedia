/**
 * @class Logger
 * @method debug
 * @method info
 * @method warn
 * @method error
 */

/**
 * @extends Logger
 * A  basic Noop Console
 */
class NoopConsole {
    debug () {
    }

    info () {
    }

    warn () {
    }

    error () {
    }
}

/**
 * There may be situations that the logger may not have a console object available
 */
const log = (typeof console === 'undefined') ? new NoopConsole() : console;

/**
 * @type Logger
 */
export default log;