<template>
    <b-form @submit="submit" v-if="authenticated">
        <b-button type="submit" variant="primary">Logout</b-button>
    </b-form>

</template>

<script>
    import EventBus, { loginConfirmed } from '../lib/util/EventBus';

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
                this.authenticated = this.$localStorage.get('auth');
            },
            submit() {
                this.$localStorage.remove('auth');
                this.authenticated = false;
            }

        }
    };
</script>