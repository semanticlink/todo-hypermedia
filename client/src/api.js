/**
 * Intercept incoming authentication callbacks with Tokens on the hash
 *
 */
import {authService} from 'semantic-link-utils/AuthService';

authService.handleAuthentication();
authService.scheduleRenewal();

// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import {LogLevel, setLogLevel, log} from 'logger';
import VueLocalStorage from 'vue-localstorage';

import Offline from './components/Offline.vue';
import Login from './components/Login.vue';
import Resource from './components/Resource.vue';
import Spinner from './components/Spinner.vue';

import {setJsonWebTokenOnHeaders} from 'semantic-link-utils/http-interceptors';
import AuthService from 'semantic-link-utils/AuthService';

import BootstrapVue from 'bootstrap-vue';
import Notifications from 'vue-notification';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';

Vue.use(BootstrapVue);
Vue.use(Notifications);

import EventBus from './lib/EventBus';
import {setInterceptors, setEventBus} from 'semantic-link-utils/http-interceptors';

setInterceptors({queue401s: false});
setEventBus(EventBus);

setLogLevel(LogLevel.Debug);
log.debug('Set log level to DEBUG');

Vue.config.productionTip = false;

/**
 * Allows us to access local storage via `this.$localStorage`
 */
Vue.use(VueLocalStorage, {name: 'localStorage'});


if (AuthService.accessToken) {
    setJsonWebTokenOnHeaders(AuthService.accessToken);
}

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
