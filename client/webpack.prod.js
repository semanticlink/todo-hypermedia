/* global require */
const merge = require('webpack-merge');
const common = require('./webpack.common.js');

module.exports = merge(common, {
    mode: 'production',
    devtool: 'source-map',
    output: {
        /**
         * Simple chunking strategy
         *
         * Need to move to 'manifest', 'vendor', etc
         chunkFilename: '[id].js',
         */
        chunkFilename: '[id].[chunkhash:6].js',
    },
    optimization: {
        splitChunks: {
            chunks: 'all'
        },
        namedChunks: true
    },
});