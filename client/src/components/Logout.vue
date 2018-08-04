<template>

    <b-form @submit="submit" v-if="authenticated">
        <b-button variant="primary" @click="submit">Logout</b-button>
    </b-form>

</template>

<script>
    import EventBus, {authConfirmed} from '../lib/util/EventBus';
    import AuthService from '../lib/AuthService';
    import {clearJsonWebTokenOnHeaders} from "../lib/http-interceptors";

    /**
     * Logout:
     *    - removes the authentication token from local storage
     *
     * Note: this should be deleting an authentication token
     */
    export default {
        name: "Logout",
        props: {
            /**
             * Callback once logout has completed. Used for post logout actions on
             * the parent context.
             *
             * @example show a notify and redirect
             */
            onLogout: {
                type: Function,
                default: () => () => {
                }
            }
        },
        data() {
            return {
                authenticated: false
            };
        },
        mounted() {
            EventBus.$on(authConfirmed, this.loginConfirmed);
            this.loginConfirmed();
        },
        methods: {
            loginConfirmed() {
                this.authenticated = AuthService.isAuthenticated;
            },
            submit() {
                AuthService.logout();
                clearJsonWebTokenOnHeaders();
                this.authenticated = false;
                this.onLogout();
            }

        }
    };
</script>