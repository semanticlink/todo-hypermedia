/* global module  */
/* global __dirname  */
/* global require */

const path = require('path');
const VueLoaderPlugin = require('vue-loader/lib/plugin');

module.exports = {
    resolve: {
        alias: {
            // vendor checked in libraries (perhaps we can dependency manage these?
            // we use this library a lot so we will treat it like an independent library in the imports
            'semantic-link-cache': path.resolve(__dirname, 'src/lib/semantic-link-cache'),
            'semantic-link-utils': path.resolve(__dirname, 'src/lib/semantic-link-utils'),

            domain: path.resolve(__dirname, 'src/domain'),
            router: path.resolve(__dirname, 'src/router'),
            logger: path.resolve(__dirname, 'node_modules/semantic-link/lib/logger'),
            /*
             * Allow for runtime compiling of vue templates
             */
            'vue$': 'vue/dist/vue.esm.js'
        },
        extensions: [
            /**
             * Plain old javascript (well, es6+)
             */
            '.js',
            /**
             * vue (combine with resolve>alias above)
             */
            '.vue',
            /**
             * typescript
             */
            '.ts'
        ]
    },
    module: {
        rules: [
            {
                test: /\.vue$/,
                loader: 'vue-loader',
            },
            {
                test: /\.css$/,
                use: [
                    'vue-style-loader',
                    'css-loader'
                ]
            },
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
    optimization: {
        namedModules: true,
    },
    plugins: [
        /**
         * @see https://vue-loader.vuejs.org/guide/
         */
        new VueLoaderPlugin(),
    ],
};