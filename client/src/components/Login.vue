<template>
    <transition name="slideup">
        <div class="login-indicator" v-if="!authenticated">
            <div class="col-sm-4 col-sm-offset-4">
                <h2>Log In</h2>
                <div class="alert alert-danger" v-if="error">
                    <p>{{ error }}</p>
                </div>
                <div class="form-group">
                    <input
                            type="text"
                            class="form-control"
                            placeholder="Enter your email"
                            v-model="credentials.email"
                    >
                </div>
                <div class="form-group">
                    <input
                            type="password"
                            class="form-control"
                            placeholder="Enter your password"
                            v-model="credentials.password"
                    >
                </div>
                <button class="btn btn-primary" @click="submit()">Login</button>
            </div>
        </div>

    </transition>
</template>

<script>
    import EventBus, { loginConfirmed, loginRequired } from '../lib/util/EventBus';
    import { link, SemanticLink, nodMaker } from 'semanticLink';
    import { log } from 'logger';
    import { getBearerLinkRelation, setBearerToken } from '../lib/http-interceptors';

    /**
     * This is a simple bool 'lock'. When the event is triggered we don't
     * want to flood the user with authentication dialogs, rather just present one
     * dialog that gives them the opportunity to make attempts to authenticate.
     *
     * This method could instead create a singleton style promise that is returned
     * to all callers. The first call which would create the promise would service
     * and resolve that promise for all subsequent callers.
     */
    let isPerformingAuthentication;

    /**
     * Holds reference to the authentication resource return from the api
     *
     * TODO: attach into this.$root.$api
     *
     *  @type {LinkedRepresentation}
     */
    let authenticatorResource;

    /**
     * Holds the value of the authentication link relation in the api that points
     * to the authentication resource
     * @type {string}
     */
    let authenticationRel;

    /**
     * Login:
     *    - waits for the unauthorised event (triggered by no network)
     */
    export default {
        data() {
            return {
                authenticated: true,
                // We need to initialize the component with any
                // properties that will be used in it
                credentials: {
                    email: '',
                    password: ''
                },
                error: ''
            };
        },
        mounted() {
            EventBus.$on(loginRequired, this.loginRequired);
        },
        methods: {
            loginRequired(error) {

                authenticationRel = getBearerLinkRelation(error);

                this.authenticated = false;

                if (isPerformingAuthentication) {
                    return;
                }
                isPerformingAuthentication = true;
            },
            submit() {
                const vm = this;

                const credentials = {
                    email: this.credentials.email,
                    password: this.credentials.password,
                    grant_type: 'password'
                };

                const makeFormEncoded = (obj) => {
                    let str = [];
                    for (let key in obj) {
                        if (obj.hasOwnProperty(key)) {
                            str.push(encodeURIComponent(key) + '=' + encodeURIComponent(obj[key]));
                        }
                    }
                    return str.join('&');
                };

                const authenticatorUri = SemanticLink.getUri(this.$root.$api, authenticationRel);

                // authenticatorUri should really be in the $api or part of the www-authenticate header
                link.post(
                    nodMaker.makeSparseResourceFromUri(authenticatorUri),
                    'self',
                    'application/x-www-form-urlencoded',
                    makeFormEncoded(credentials))
                    .then(response => {

                        if (!response.data.token) {
                            log.error('Bearer token not returned on the key: \'token\'');
                        }
                        setBearerToken(response.data.token);

                        EventBus.$emit(loginConfirmed);
                        isPerformingAuthentication = false;
                        vm.authenticated = true;
                    })
                    .catch(failureResponse => {
                        vm.error = '';
                        if (failureResponse && failureResponse.status) {
                            log.warn('Authentication failed (' + failureResponse.status + '): ' + failureResponse.statusText);
                        } else {
                            log.warn('Authentication: failed', arguments);
                        }
                        vm.error = 'Invalid email/password';
                    });
            }

        }
    };
</script>

<style>
    .login-indicator {
        background: #E8EDE9;
        border-radius: 3px;
        border-bottom: 2px solid #d3d9d4;
        padding: 15px 5px;
        text-align: center;
        width: 100%;
        margin: 5px 0;
        font-family: sans-serif;
        color: #202126;
        box-sizing: border-box;
    }

    .slideup-enter-active, .slideup-leave-active {
        transition: transform .2s, opacity .2s;
        transform: none;
        opacity: 1;
    }

    .slideup-enter, .slideup-leave-active {
        opacity: 0;
        transform: translateY(-100%);
    }
</style>