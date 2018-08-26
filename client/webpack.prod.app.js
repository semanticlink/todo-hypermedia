/* global require */
/* global __dirname */
const merge = require('webpack-merge');
const common = require('./webpack.prod.js');
const CleanWebpackPlugin = require('clean-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const path = require('path');
let FaviconsWebpackPlugin = require('favicons-webpack-plugin')

module.exports = merge(common, {
    entry: {
        // single-page app
        app: './src/app',
    },
    output: {
        /**
         * In app and api, we make these available under the same path
         */
        path: path.resolve(__dirname, 'dist/app'),
    },
    plugins: [
        new CleanWebpackPlugin(['dist/app']),

        /**
         * Build the html files for Todo app. These provide the <script src="*"/>  to include
         *
         * This is way more complicated with chunking. The best I could find was to exclude any
         * chunks that are the 'other'. This is *very* brittle.
         *
         * Chunks sort mode loads them on the page in sort of the right order. Again, *broken*.
         */
        new HtmlWebpackPlugin({
            title: 'Resource',
            chunksSortMode: 'id',
            hash: true,
            filename: 'index.html',
            template: 'template.html',
            templateParameters: {
                title: 'Todo',
                rel: 'api',
                uri: 'https://api.todo.semanticlink.io/'
            }
        }),

        new FaviconsWebpackPlugin(path.resolve(__dirname, 'logo.png')),

    ]
});