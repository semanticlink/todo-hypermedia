<template>
    <div id="app">
        <Spinner/>
        <Offline/>
        <Login/>
        <notifications/>
        <router-view></router-view>
        <div>
            <router-link :to="{ name: routeName.Admin }" v-if="$route.fullPath !== routePath.Admin">Admin</router-link>
        </div>
    </div>
</template>

<script>
    import Spinner from './components/Spinner.vue';
    import Offline from './components/Offline.vue';
    import Login from './components/Login.vue';
    import * as cache from 'semanticLink/NODMaker';
    import * as link from 'semantic-link';
    import {routeName, routePath} from "router";

    export default {
        name: 'app',
        components: {Offline, Login, Spinner},
        data() {
            return {
                routeName, routePath
            }
        },
        created: function () {

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
             * Note: this is currently found on `this.$root.$api` because it was pre-registered
             *
             * @example setup example
             *
             *   let store;
             *
             *   const apiPlugin = {
             *       store,
             *       install(Vue) {
             *           // attach to the root view
             *           // access via this.$root.$api
             *           Vue.prototype.$api = store;
             *       }
             *   };
             *
             *   Vue.use(apiPlugin);
             *
             * @example usage
             *
             *  getUri(this.$root.$api, 'self') --> a uri
             *
             * @type {ApiRepresentation}
             */
            this.$root.$api = this.$root.$api || cache.makeSparseResourceFromUri(link.getUri('HEAD', 'api'));

        }
    };
</script>
