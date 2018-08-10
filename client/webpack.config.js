/* global module __dirname require process */

const path = require('path');
const webpack = require('webpack');
const merge = require('webpack-merge');
const VueLoaderPlugin = require('vue-loader/lib/plugin');

/**
 *  Configuration of the javascript and css for:
 *      - single-page app client
 */
const clientConfig = {
    entry: {
        // single-page app
        app: './src/app',
        api: './src/api'
    },
    output: {
        path: path.join(__dirname, 'dist'),
        /**
         * In webapp and admin, we make these available under the same path
         */
        publicPath: '/dist/',
        /**
         * Note: currently web and admin all have cache busting on the query params and we
         * don't want to rewrite the .cshtml/.aspx files. The chunks, however, are not cache
         * busted in the application and thus need hashes.
         */
        filename: '[name].js',
        chunkFilename: '[id].[chunkhash:6].js',
        /**
         * This is required so that 'require'js modules are umd loaded
         */
        libraryTarget: 'umd'
    },
    externals: {
        /**
         * Bottleneck has these transitive dependencies for
         */
        'hiredis': 'hiredis',
        'redis': 'redis'
    },
    resolve: {
        alias: {
            // vendor checked in libraries (perhaps we can dependency manage these?
            // we use this library alot so we will treat it like an independent library in the imports
            'semantic-link-cache': path.resolve(__dirname, 'src/lib/semantic-link-cache'),

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
            '.ts',
            '.tsx'
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
        new VueLoaderPlugin()
    ],
    // node: {
    //     // used so that bottleneck can be loaded (peer dependency of redis/hiredis and transitives)
    //     net: 'empty',
    //     tls: 'empty',
    //     redis: 'empty'
    // },

};

const commonConfig = {
    devServer: {
        historyApiFallback: true,
        noInfo: true,
        // force 8080 for now because it needs to be well known for the API to pick up the development version
        port: 8080
    },
    performance: {
        hints: false
    },
    devtool: '#eval-source-map'
};

if (process.env.NODE_ENV === 'production') {
    commonConfig.devtool = '#source-map';
    commonConfig.plugins = (commonConfig.plugins || []).concat([
        new webpack.DefinePlugin({
            'process.env': {
                NODE_ENV: '"production"'
            }
        }),
        new webpack.optimize.UglifyJsPlugin({
            sourceMap: true,
            compress: {
                warnings: false
            }
        }),
        new webpack.LoaderOptionsPlugin({
            minimize: true
        })
    ]);
}

if (process.env.NODE_ENV === 'development') {
    commonConfig.plugins = (commonConfig.plugins || []).concat([]);
}

module.exports = merge(commonConfig, clientConfig);