/* global require */
const merge = require('webpack-merge');
const common = require('./webpack.common.js');

module.exports = merge(common, {
    output: {
        /**
         * In app and api, we make these available under the same path
         */
        publicPath: '/dist/',
    },
    mode: 'development',
    devtool: 'inline-source-map',
    devServer: {
        historyApiFallback: true,
        noInfo: true,
        // force 8080 for now because it needs to be well known for the API to pick up the development version
        port: 8080,
    }
});