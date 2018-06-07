// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.conf with an alias.
import Vue from 'vue';
import { LEVEL, setLogLevel } from 'semanticLink';
import VueLocalStorage from 'vue-localstorage';

import Offline from './components/Offline.vue';
import Login from './components/Login.vue';
import Resource from './components/Resource.vue';

import { setBearerToken } from './lib/http-interceptors';

import BootstrapVue from 'bootstrap-vue';
import Notifications from 'vue-notification';

import 'bootstrap/dist/css/bootstrap.css';
import 'bootstrap-vue/dist/bootstrap-vue.css';

Vue.use(BootstrapVue);
Vue.use(Notifications);

require('./lib/http-interceptors');

setLogLevel(LEVEL.INFO);

Vue.config.productionTip = false;

/**
 * Allows us to access local storage via `this.$localStorage`
 */
Vue.use(VueLocalStorage, {name: 'localStorage'});

/*
 * If there is an already set authorisation bearer token then use it by default
 */
if (Vue.localStorage.get('auth')) {
    setBearerToken(Vue.localStorage.get('auth'));
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
    components: {Offline, Login, Resource},
    template: '<div><Resource :apiUri="this.apiUri"></Resource><Offline/><Login/><notifications/></div>'
});
