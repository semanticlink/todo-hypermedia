<template>
    <transition name="slideup">
        <b-form @submit="submit" class="login-indicator" v-if="!authenticated">
            <b-form-group id="exampleInputGroup1"
                          label="Email address:"
                          label-for="exampleInput1"
                          description="We'll never share your email with anyone else.">
                <b-form-input id="exampleInput1"
                              type="email"
                              v-model="credentials.email"
                              required
                              placeholder="Enter your email">
                </b-form-input>
            </b-form-group>
            <b-form-group id="exampleInputGroup2"
                          label="Your Password:"
                          label-for="exampleInput2">
                <b-form-input id="exampleInput2"
                              type="password"
                              v-model="credentials.password"
                              required
                              placeholder="Enter your password">
                </b-form-input>
            </b-form-group>
            <b-button type="submit" variant="primary">Login</b-button>
        </b-form>

    </transition>
</template>

<script>
    import EventBus, { loginConfirmed, loginRequired } from '../lib/util/EventBus';
    import { link, SemanticLink, nodMaker } from 'semanticLink';
    import { log } from 'logger';
    import { getAuthenticationUri, getBearerLinkRelation, setBearerToken } from '../lib/http-interceptors';

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
     * Holds the error from the 401 handler
     * @type {AxiosError}
     */
    let error;

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
            /**
             * @param {AxiosError} err
             */
            loginRequired(err) {

                error = err;

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
                    password: this.credentials.password
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

                /**
                 * Very simple (and awful) implementation.
                 *
                 * The www-authenticate header has the required information:
                 *
                 *   - uri of api
                 *   - link relation to post to
                 *
                 *      1. GET the resource at the authenticate uri
                 *      2. Construct a login object
                 *      3. POST the login form back on the uri of link relation
                 *
                 *
                 * A better server implementation would:
                 *
                 *  - return an authenticate resource
                 *  - create form for the login (telling where to submit)
                 *  - we then need to match our client form with the server login form (or display the login form)
                 *
                 */

                return nodMaker
                    .getResource(nodMaker.makeSparseResourceFromUri(getAuthenticationUri(error)))
                    .then(authenticator =>
                        link.post(
                            authenticator,
                            getBearerLinkRelation(error),
                            'application/x-www-form-urlencoded',
                            makeFormEncoded(credentials))
                            .then(response => {

                                if (!response.data.token) {
                                    log.error('Bearer token not returned on the key: \'token\'');
                                }
                                setBearerToken(response.data.token);
                                // save bearer token so that when users do a full refresh
                                // we can save the token across a refresh
                                vm.$localStorage.set('auth', response.data.token);

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
                            }));


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