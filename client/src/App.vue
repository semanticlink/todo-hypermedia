<template>
    <div id="app">
        <Spinner/>
        <Offline/>
        <Login/>
        <notifications/>
        <router-view></router-view>
        <div>
            <b-link :to="{ name: 'Home' }">Home</b-link>
        </div>
    </div>
</template>

<script>
    import Spinner from './components/Spinner.vue';
    import Offline from './components/Offline.vue';
    import Login from './components/Login.vue';
    import {nodMaker} from 'semanticLink';
    import * as link from 'semantic-link';

    export default {
        name: 'app',
        components: {Offline, Login, Spinner},
        created: function () {

            const apiUri = link.getUri('HEAD', 'api');

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
            this.$root.$api = this.$root.$api || nodMaker.makeSparseResourceFromUri(apiUri);

        }
    };
</script>
