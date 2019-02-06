/**
 * Intercept incoming authentication callbacks with Tokens on the hash
 *
 */
import AuthService, {authService} from './lib/AuthService';

authService.handleAuthentication();


// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import router from './router';
import {LogLevel, setLogLevel, log} from 'logger';
import VueLocalStorage from 'vue-localstorage';

import {uriMapping} from 'semantic-link-utils/UriMapping';
import {filter} from 'semantic-link';

import App from './components/app/App.vue';

import {setJwtOnHeaders, setInterceptors} from 'semantic-link-utils/http-interceptors';
import {setEventBus} from 'semantic-link-utils/EventBus';
import EventBus from './lib/EventBus';

import BootstrapVue from 'bootstrap-vue';
import Notifications from 'vue-notification';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import './styles/todo.css';

Vue.use(BootstrapVue);
Vue.use(Notifications);

setLogLevel(LogLevel.Debug);
log.debug('[App] Set log level to DEBUG');

Vue.config.productionTip = false;

/*
 * Network of data store (application cache)
 *
 * This object must be added so that it can reference throughout the application. Apparently the
 * best way to do this is via plugin
 *
 * You can now get at the cache from `this.$root.$api`
 *
 * @see https://stackoverflow.com/questions/37711756/how-to-setup-a-global-store-object
 */
let store;
const apiPlugin = {
    store,
    install(Vue) {
        // attach to the root view
        // access via this.$root.$api
        Vue.prototype.$api = store;
    }
};

Vue.use(apiPlugin);

/**
 * Allows us to access local storage via `this.$localStorage`
 */
Vue.use(VueLocalStorage, {name: 'localStorage'});

setInterceptors();
setEventBus(EventBus);
setJwtOnHeaders(AuthService.accessToken);

/**
 * This view sets up the application including the on-demand authentication (login) and
 * the application being offline
 */
new Vue({
    el: '#app',
    router,
    template: '<App/>',
    components: {App},
    created() {

        /**
         * Get the starting URL from the base HTML
         * <head>
         *  <title>Todo App</title>
         *  <link rel="api" href="http://localhost:5000/"/>
         * </head>
         */
        const [api,] = filter('HEAD', 'api');
        const apiUri = api.href;
        const clientUri = window.location.href;

        log.info(`[App] Client uri: '${clientUri}'`);
        log.info(`[App] Api uri: '${apiUri}'`);

        uriMapping(clientUri, apiUri);

        /**
         * Setup the global options for the api application cache.
         *
         * Because we are do lazy loading, changes to representations (ie new attributes on sparsely
         * hydrated resources) are not propagated to the ui. Vue2 is particularly poor in certain
         * situations. By providing `set` we overcome this problem without having to bind explicitly
         * to view-level properties upon each request/promise. Vue3 shouldn't have this problem.
         * Angular1 has this issue.
         *
         * @type {CacheOptions}
         */
        this.$root.options = {set: Vue.set};

    }
});
