<template>
    
    <b-form @submit="submit" v-if="authenticated">
        <b-button type="submit" variant="primary">Logout</b-button>
    </b-form>

</template>

<script>
    import EventBus, { loginConfirmed } from '../lib/util/EventBus';
    import BearerTokenService from '../lib/BearerTokenService';
    import AuthService from '../lib/AuthService';

    /**
     * Logout:
     *    - removes the authentication token from local storage
     *
     * Note: this should be deleting an authentication token
     */
    export default {
        name: "Logout",
        data() {
            return {
                authenticated: false
            };
        },
        mounted() {
            EventBus.$on(loginConfirmed, this.loginConfirmed);
            this.loginConfirmed();
        },
        methods: {
            loginConfirmed() {
                this.authenticated = BearerTokenService.token || AuthService.isAuthenticated;
            },
            submit() {
                BearerTokenService.clear();
                AuthService.clearSession();
                this.authenticated = false;
            }

        }
    };
</script>