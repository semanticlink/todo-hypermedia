/* global require */
const path = require('path');
var nodeExternals = require('webpack-node-externals');

/**
 * @see https://github.com/zinserjan/mocha-webpack/blob/v2.0.0-beta.0/docs/installation/webpack-configuration.md
 * @see https://github.com/zinserjan/mocha-webpack-example/blob/master/webpack.config-test.js
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
            {
                // Only run `.js` files through Babel
                test: /\.js$/,
                exclude: /node_modules/,
                loader: 'babel-loader'
            },
            {

                test: /\.tsx?$/,
                exclude: /node_modules/,
                loader: 'ts-loader'
            },
        ]
    },
    output: {
        // use absolute paths in sourcemaps (important for debugging via IDE)
        devtoolModuleFilenameTemplate: '[absolute-resource-path]',
        devtoolFallbackModuleFilenameTemplate: '[absolute-resource-path]?[hash]'
    },
    target: 'node',  // webpack should compile node compatible code
    devtool: 'source-map',
    // externals: [nodeExternals()],
    resolve: {
        alias: {
            // vendor checked in libraries (perhaps we can dependency manage these?
            // we use this library a lot so we will treat it like an independent library in the imports
            'semantic-link-cache': path.resolve(__dirname, 'src/lib/semantic-link-cache'),
            'semantic-link-utils': path.resolve(__dirname, 'src/lib/semantic-link-utils'),

            domain: path.resolve(__dirname, 'src/domain'),
            router: path.resolve(__dirname, 'src/router'),
            logger: path.resolve(__dirname, 'node_modules/semantic-link/lib/logger'),
        },
        extensions: [
            /**
             * Plain old javascript (well, es6+)
             */
            '.js',
            /**
             * typescript
             */
            '.ts'
        ]
    }
};