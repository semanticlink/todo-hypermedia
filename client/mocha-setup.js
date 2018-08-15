/* global require */
require('source-map-support').install({
    handleUncaughtExceptions: false,
    environment: 'node',
});

/**
 * Global include for the {@link SemanticLink.filter}
 * @constructor
 */
global.Element = () => {
};