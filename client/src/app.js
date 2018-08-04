/**
 * Intercept incoming authentication callbacks with Tokens on the hash
 *
 */
import {authService} from './lib/AuthService';

authService.handleAuthentication();


// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import router from './router';
import {LEVEL, setLogLevel} from 'semanticLink';
import VueLocalStorage from 'vue-localstorage';

import App from './App.vue';

import {setJsonWebTokenOnHeaders, setInterceptors} from './lib/http-interceptors';
import AuthService from './lib/AuthService';

import BootstrapVue from 'bootstrap-vue';
import Notifications from 'vue-notification';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import './styles/todo.css';

Vue.use(BootstrapVue);
Vue.use(Notifications);

/*
 * Add runtime dependencies
 */
require('./lib/uri-mappings');

setLogLevel(LEVEL.DEBUG);

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

setInterceptors({queue401s: false});

if (AuthService.accessToken) {
    setJsonWebTokenOnHeaders(AuthService.accessToken);
}


/**
 * This view sets up the application including the ondemand authentication (login) and
 * the application being offline
 */
new Vue({
    el: '#app',
    router,
    template: '<App/>',
    components: {App}
});
