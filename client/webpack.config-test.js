const nodeExternals = require('webpack-node-externals');

/**
 * @see https://github.com/zinserjan/mocha-webpack/blob/v2.0.0-beta.0/docs/installation/webpack-configuration.md
 */

module.exports = {
    mode: 'development',
    module: {
        rules: [
            /**
             * Remove CSS processing to remove error and speed up
             * @see https://github.com/zinserjan/mocha-webpack/blob/v2.0.0-beta.0/docs/installation/webpack-configuration.md#without-css-modules
             */
            {test: /\.scss$/, loader: 'null-loader'},
            {test: /\.css$/, loader: 'null-loader'},
        ]
    },
    output: {
        // use absolute paths in sourcemaps (important for debugging via IDE)
        devtoolModuleFilenameTemplate: '[absolute-resource-path]',
        devtoolFallbackModuleFilenameTemplate: '[absolute-resource-path]?[hash]'
    },
    target: 'node',  // webpack should compile node compatible code
    externals: [nodeExternals()], // in order to ignore all modules in node_modules folder
    devtool: 'inline-cheap-module-source-map'
};