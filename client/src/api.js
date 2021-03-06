/**
 * Intercept incoming authentication callbacks with Tokens on the hash
 *
 */
import AuthService, {authService} from './lib/AuthService';

authService.handleAuthentication();

// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import {LogLevel, setLogLevel, log} from 'logger';
import VueLocalStorage from 'vue-localstorage';

import Offline from './components/Offline.vue';
import Login from './components/authentication/Login.vue';
import Resource from './components/api/Resource.vue';
import Spinner from './components/Spinner.vue';

import {setJwtOnHeaders, setInterceptors} from 'semantic-link-utils/http-interceptors';
import {setEventBus} from 'semantic-link-utils/EventBus';
import EventBus from './lib/EventBus';

import BootstrapVue from 'bootstrap-vue';
import Notifications from 'vue-notification';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';

Vue.use(BootstrapVue);
Vue.use(Notifications);

setLogLevel(LogLevel.Debug);
log.debug('[Api] Set log level to DEBUG');

Vue.config.productionTip = false;

/**
 * Allows us to access local storage via `this.$localStorage`
 */
Vue.use(VueLocalStorage, {name: 'localStorage'});

setJwtOnHeaders(AuthService.accessToken);
setEventBus(EventBus);
setInterceptors({queue401s: true});

/**
 * This view sets up the application including the ondemand authentication (login) and
 * the application being offline
 */
new Vue({
    el: '#app',
    data: {
        apiUri: document.querySelector('HEAD link[rel="self"]').href
    },
    components: {Offline, Login, Resource, Spinner},
    template: '<div><Spinner/><Resource :apiUri="this.apiUri"></Resource><Offline/><Login/><notifications/></div>'
});
