/* global require */
const merge = require('webpack-merge');
const common = require('./webpack.common.js');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const path = require('path');

module.exports = merge(common, {
    mode: 'development',
    devtool: 'inline-source-map',
    devServer: {
        historyApiFallback: true,
        noInfo: true,
        // force 8080 for now because it needs to be well known for the API to pick up the development version
        port: 8080,
    },
    entry: {
        // single-page app
        app: './src/app',
        // api client app that is loaded on html representation of resources
        api: './src/api'
    },
    output: {
        /**
         * In app and api, we make these available under the same path
         */
        path: path.resolve(__dirname, 'build'),
        // publicPath: '/build/',
        /**
         * The chunks, however, are not cache
         * busted in the application and thus need hashes.
         */
        filename: '[name].js',
    },
    plugins: [
        new CleanWebpackPlugin(['build']),

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
            chunks: ['app',],
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
            chunks: ['api'],
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