<template>
    <transition name="slideup">
        <div>
            <!-- TODO: styling on a login -->
            <div> {{ error }}</div>

            <Form class="login-indicator"
                  :representation="representation"
                  :formRepresentation="formRepresentation"
                  :on-submit="onSubmit"
                  :on-success="onSuccess"
                  :on-failure="onFailure"
                  no-cancel
                  v-if="!authenticated"/>

        </div>
    </transition>
</template>

<script>
    import axios from 'axios';
    import EventBus, { loginConfirmed, loginRequired } from '../lib/util/EventBus';
    import { link } from 'semanticLink';
    import { log } from 'logger';
    import { getAuthenticationUri, getBearerLinkRelation, setBearerToken } from '../lib/http-interceptors';
    import Form from './Form.vue';

    /**
     * This is a simple boolean 'lock'. When the event is triggered we don't
     * want to flood the user with authentication dialogs, rather just present one
     * dialog that gives them the opportunity to make attempts to authenticate.
     *
     * This method could instead create a singleton style promise that is returned
     * to all callers. The first call which would create the promise would service
     * and resolve that promise for all subsequent callers.
     */
    let isPerformingAuthentication;


    /**
     * Login:
     *    - waits for the unauthorised event (triggered by no network)
     */
    export default {
        components: {Form},
        data() {
            return {
                authenticated: true,
                /**
                 * @type {CollectionRepresentation}
                 */
                representation: {},
                /**
                 * @type {FormRepresentation}
                 */
                formRepresentation: {},
                /**
                 * Error message to the user
                 * @type {string}
                 */
                error: ''
            };
        },
        mounted() {
            EventBus.$on(loginRequired, this.loginRequired);
        },
        methods: {
            /**
             * Contains our 401 Error
             *
             * Very simple (and awful) implementation.
             *
             * The www-authenticate header has the required information:
             *
             *   - uri of api
             *   - link relation to post to
             *
             *      1. GET the resource at the authenticate uri
             *      2. GET the resource with link rel
             *      3. GET the 'create-form' for the login
             *      3a. Display the login form in the GUI from these attributes and update/enter
             *      4. POST the login form back on the uri of referring collection
             *
             *
             * @param {AxiosError} error
             */
            loginRequired(error) {

                this.authenticated = false;

                if (isPerformingAuthentication) {
                    return;
                }
                axios.get(getAuthenticationUri(error))
                    .then(response => link.get(response.data, getBearerLinkRelation(error)))
                    .then(authenticateCollection => {
                        this.representation = authenticateCollection.data;
                        return link.get(authenticateCollection.data, /create-form/);
                    })
                    .then(authenticateLoginRepresentation => {
                        this.formRepresentation = authenticateLoginRepresentation.data;
                        // only turn on the lock once we have the login form ready
                        isPerformingAuthentication = true;
                    })
                    .catch(/** @type {AxiosError|AxiosResponse} */error => {
                        this.$notify({
                            title: 'An error means that the login process won\'t work',
                            text: error.statusText || error.response.statusText,
                            type: 'error'
                        });
                    });

            },
            onSubmit() {
                this.error = '';
            },
            onSuccess(response) {

                if (!response.data.token) {
                    log.error('Bearer token not returned on the key: \'token\'');
                }
                setBearerToken(response.data.token);
                // save bearer token so that when users do a full refresh
                // we can save the token across a refresh
                this.$localStorage.set('auth', response.data.token);

                EventBus.$emit(loginConfirmed);

                isPerformingAuthentication = false;
                this.authenticated = true;

            },
            onFailure(error) {

                this.$notify({text: 'Authentication: failed'})
                this.error = '';
                if (error && error.status) {
                    log.warn('Authentication failed (' + error.status + '): ' + error.statusText);
                } else {
                    log.warn('Authentication: failed', arguments);
                }
                this.error = 'Invalid email/password';
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
        /*text-align: center;*/
        width: 60%;
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