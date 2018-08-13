/* global require */
const merge = require('webpack-merge');
const common = require('./webpack.common.js');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');

module.exports = merge(common, {
    mode: 'production',
    devtool: 'source-map',
    output: {
        /**
         * Simple chunking strategy
         *
         * Need to move to 'manifest', 'vendor', etc
         */
        chunkFilename: '[id].js',
        // chunkFilename: '[id].[chunkhash:6].js',
    },
    optimization: {
        splitChunks: {
            chunks: 'all'
        },
        namedChunks: true
    },
    plugins: [
        new CleanWebpackPlugin(['dist']),

        /**
         * Build the html files (app and api). These provide the <script src="*"/>  to include
         *
         * This is way more complicated with chunking. The best I could find was to exlude any
         * chunks that are the 'other'. This is *very* brittle.
         *
         * Chunks sort mode loads them on the page in sort of the right order. Again, *broken*.
         */
        new HtmlWebpackPlugin({
            title: 'app',
            excludeChunks: ['api', 'vendors~api'],
            chunksSortMode: 'id',
            filename: 'app.html',
            template: 'template.html',
            templateParameters: {
                title: 'Todo',
                rel: 'api',
                uri: 'http://locahost:5000/'
            }
        }),

        new HtmlWebpackPlugin({
            title: 'Resource',
            excludeChunks: ['app', 'vendors~app'],
            chunksSortMode: 'id',
            filename: 'api.html',
            template: 'template.html',
            templateParameters: {
                title: 'Resource Explorer',
                rel: 'self',
                uri: 'http://locahost:5000/'
            }
        }),

    ]
});