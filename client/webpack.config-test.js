/* global require */
const merge = require('webpack-merge');
const common = require('./webpack.common.js');

/**
 * @see https://github.com/zinserjan/mocha-webpack/blob/v2.0.0-beta.0/docs/installation/webpack-configuration.md
 * @see https://github.com/zinserjan/mocha-webpack-example/blob/master/webpack.config-test.js
 */

module.exports =  merge(common, {
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
    devtool: 'inline-cheap-module-source-map'
});