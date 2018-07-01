/**
 * Intercept incoming authentication callbacks with Tokens on the hash
 *
 */
import { authService } from './lib/AuthService';
authService.handleAuthentication();


// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import router from './router';
import { LEVEL, nodMaker, setLogLevel } from 'semanticLink';
import VueLocalStorage from 'vue-localstorage';

import App from './App.vue';
import Offline from './components/Offline.vue';
import Login from './components/Login.vue';
import AbstractTenant from './components/AbstractTenant.vue';

import DroppableModel from './components/DroppableModel';
import DraggableModel from './components/DraggableModel';
import DragAndDroppableModel from './components/DragAndDroppableModel';

import { setBearerTokenOnHeaders, setJsonWebTokenOnHeaders } from './lib/http-interceptors';
import BearerTokenService from './lib/BearerTokenService';
import AuthService from './lib/AuthService';

import BootstrapVue from 'bootstrap-vue';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';
import './styles/todo.css';

Vue.use(BootstrapVue);

/*
 * Add runtime dependencies
 */
require('./lib/uri-mappings');
require('./lib/http-interceptors');

setLogLevel(LEVEL.INFO);

Vue.config.productionTip = false;


/*
 * Network of data store
 *
 * This object must be added so that it can reference throughout the application. Apparently the
 * best way to do this is via plugin
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

/*
 * If there is an already set authorisation bearer token then use it by default
 */
if (BearerTokenService.token) {
    setBearerTokenOnHeaders(BearerTokenService.token);
}

if (AuthService.accessToken) {
    setJsonWebTokenOnHeaders(AuthService.accessToken);
}


//  TODO: understand inline-templates so that this does haven't to be registered globally
// @see https://stackoverflow.com/questions/46173821/for-recursive-components-make-sure-to-provide-the-name-option
Vue.component('draggable-model', DraggableModel);
Vue.component('droppable-model', DroppableModel);
Vue.component('drag-and-droppable-model', DragAndDroppableModel);


/**
 * This view sets up the application including the ondemand authentication (login) and
 * the application being offline
 */
new Vue({
    el: '#app',
    router,
    template: '<div><App/><Offline/><Login/></div>',
    components: {App, Offline, Login, AbstractTenant},
    created: function () {

        // TODO: work out why `import { apiUri, authenticatorUri } from './lib/uri-mappings';` doesn't work
        const apiUri = document.querySelector('HEAD link[rel="api"]').href;
        /**
         * The api representation is the 'top of the tree' in terms of the network of
         * data synchronisation with the server. Note that the in-memory resource does
         * have an internal {@link State} to track the need for synchronisation throughout
         * its life (including staleness)
         *
         * However, the data is not being fully or reliably propagated down to the children.
         * Until a better solution is found, we are injecting it from the previous state on the
         * state change events. See below.
         *
         * Note: this is currently found on `this.$root.$api`
         *
         * @type {ApiRepresentation}
         */
        this.$api = this.$api || nodMaker.makeSparseResourceFromUri(apiUri);

    }
});
