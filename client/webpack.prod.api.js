/* global require */
/* global __dirname */
const merge = require('webpack-merge');
const common = require('./webpack.prod.js');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const path = require('path');

module.exports = merge(common, {
    entry: {
        // api client app that is loaded on html representation of resources
        api: './src/api'
    },
    output: {
        /**
         * In app and api, we make these available under the same path
         */
        path: path.resolve(__dirname, 'dist/api'),
    },
    plugins: [
        new CleanWebpackPlugin(['dist/api']),
    ]
});