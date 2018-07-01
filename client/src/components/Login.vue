<template>
    <transition name="slideup">
        <div>
            <!-- TODO: styling on a login -->
            <div> {{ error }}</div>

            <Form class="login"
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
    import EventBus, { loginConfirmed, loginRequired } from '../lib/util/EventBus';
    import { log } from 'logger';
    import {
        setBearerTokenOnHeaders,
        getAuthenticationScheme,
        JWT,
        BEARER, setJsonWebTokenOnHeaders
    } from '../lib/http-interceptors';
    import Form from './Form.vue';
    import AuthService from "../lib/AuthService";
    import FormService from "../lib/FormService";
    import BearerTokenService from "../lib/BearerTokenService";

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
             * Login/authenticate based on www-authenticate headers in the 401 response/error
             *
             * @param {AxiosError} error
             */
            loginRequired(error) {

                this.authenticated = false;

                if (isPerformingAuthentication) {
                    return;
                }

                if (getAuthenticationScheme(error) === JWT) {

                    // JSONWebToken authentication

                    /**
                     *  Use our JSONWebToken authentication scheme to get a token from external provider (Auth0)
                     */

                    AuthService.loadFrom401JsonWebTokenChallenge(error)
                        .then(configuration => {
                            let authService = new AuthService(configuration.data);
                            authService.login((err, authResult) => {

                                if (!authResult || !authResult.accessToken) {
                                    log.error('Json web token not returned on the key: \'accessToken\'');
                                }
                                setJsonWebTokenOnHeaders(authResult.accessToken);
                                BearerTokenService.token = authResult.accessToken;

                                EventBus.$emit(loginConfirmed);

                                isPerformingAuthentication = false;
                                this.authenticated = true;
                            })

                        })
                        .catch(this.onFailure);

                } else if (getAuthenticationScheme(error) === BEARER) {

                    // Bearer authentication

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
                     */
                    FormService.loadFormFrom401BearerChallenge(error)
                        .then(([form, collection]) => {
                            // these get bound to the form control that displays the login
                            this.formRepresentation = form;
                            this.representation = collection;
                            // only turn on the lock once we have the login form ready
                            isPerformingAuthentication = true;
                        })
                        // note in this work flow onSuccess is called by the form
                        .catch(this.onFailure);
                } else {

                    log.error('Unable to authenticate, no known www-authenticate type');
                }


            },
            onSubmit() {
                this.error = '';
            },
            /**
             * Assumes that the payment of the resource has token field available.
             *
             * @param {AxiosResponse} response
             */
            onSuccess(response) {

                if (!response.data.token) {
                    log.error('Bearer token not returned on the key: \'token\'');
                }

                setBearerTokenOnHeaders(response.data.token);
                BearerTokenService.token = response.data.token;

                EventBus.$emit(loginConfirmed);

                isPerformingAuthentication = false;
                this.authenticated = true;

            },
            onFailure(error) {

                this.$notify({
                    title: 'An error means that the login process won\'t work',
                    text: error || error.statusText || error.response.statusText,
                    type: 'error'
                });
            }
        }
    };
</script>

<style>

    /*
     *
     * field set
     *   legend (label)
     *   group
     *     input
     *     highlight
     *     bar
     *
     *  Basic styling taken from: https://codepen.io/joshadamous/pen/yyyqJZ
     *
     *  Note: to create a login closer to material need to use bootstrap-vue component input-group and rework the css
     *  TODO: write this in less
     *
     */

    * {
        box-sizing: border-box;
    }

    form.login {
        width: 380px;
        margin: 4em auto;
        padding: 3em 2em 2em 2em;
        background: #fafafa;
        border: 1px solid #ebebeb;
        box-shadow: rgba(0, 0, 0, 0.14902) 0px 1px 1px 0px, rgba(0, 0, 0, 0.09804) 0px 1px 2px 0px;
    }

    .login fieldset {
        position: relative;
        margin-bottom: 45px;
    }

    .login input {
        font-size: 18px;
        padding: 10px 10px 10px 5px;
        -webkit-appearance: none;
        display: block;
        background: #fafafa;
        color: #636363;
        width: 100%;
        border: none;
        border-radius: 0;
        border-bottom: 1px solid #757575;
    }

    .login input:focus {
        outline: none;
    }

    /* Label */

    .login legend {
        color: #999;
        font-size: 18px;
        font-weight: normal;
        position: absolute;
        pointer-events: none;
        left: 5px;
        top: -30px;
        /*top: 10px;*/
        transition: all 0.2s ease;
    }

    /* Underline */

    .login .bar {
        position: relative;
        display: block;
        width: 100%;
    }

    .login .bar:before, .bar:after {
        content: '';
        height: 2px;
        width: 0;
        bottom: 1px;
        position: absolute;
        background: #4a89dc;
        transition: all 0.2s ease;
    }

    .bar:before {
        left: 50%;
    }

    .bar:after {
        right: 50%;
    }

    /* active */

    input:focus ~ .bar:before, input:focus ~ .bar:after {
        width: 50%;
    }

    /* Highlight */

    .login .highlight {
        position: absolute;
        height: 60%;
        width: 100px;
        top: 25%;
        left: 0;
        pointer-events: none;
        opacity: 0.5;
    }

    /* active */

    .login input:focus ~ .highlight {
        animation: inputHighlighter 0.3s ease;
    }

    /* Animations */

    @keyframes inputHighlighter {
        from {
            background: #4a89dc;
        }
        to {
            width: 0;
            background: transparent;
        }
    }

    /* Button */

    .login .btn {
        position: relative;
        display: inline-block;
        padding: 12px 24px;
        margin: .3em 0 1em 0;
        width: 100%;
        vertical-align: middle;
        color: #fff;
        font-size: 16px;
        line-height: 20px;
        -webkit-font-smoothing: antialiased;
        text-align: center;
        letter-spacing: 1px;
        background: transparent;
        border: 0;
        border-bottom: 2px solid #3160B6;
        cursor: pointer;
        transition: all 0.15s ease;
    }

    .login .btn:focus {
        outline: 0;
    }

    /* Button modifiers */

    .login .btn-primary {
        background: #4a89dc;
        text-shadow: 1px 1px 0 rgba(39, 110, 204, .5);
    }

    .login .btn-primary:hover {
        background: #357bd8;
    }


</style>