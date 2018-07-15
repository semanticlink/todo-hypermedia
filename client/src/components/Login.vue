<template>
    <transition name="slideup">
        <div>
            <!-- TODO: styling on a login -->
            <div> {{ error }}</div>

        </div>
    </transition>
</template>

<script>
    import EventBus, { loginConfirmed, loginRequired } from '../lib/util/EventBus';
    import { log } from 'logger';
    import {
        getAuthenticationScheme,
        JWT,
        setJsonWebTokenOnHeaders,
        getAuthenticationRealm,
        API_AUTH0_REALM, renewToken
    } from '../lib/http-interceptors';
    import Form from './Form.vue';
    import AuthService from "../lib/AuthService";

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

                const authenticationScheme = getAuthenticationScheme(error);

                if (authenticationScheme === JWT) {

                    // JSONWebToken authentication

                    /**
                     *  Use our jwt authentication scheme to get a token from external provider (Auth0)
                     */

                    AuthService.loadFrom401JsonWebTokenChallenge(error)
                        .then(configuration => {


                            /** @type {Auth0ConfigurationRepresentation} */
                            const cfg = configuration.data;

                            if (false && renewToken(error)){

                                log.info('Renewing token');

                                AuthService
                                    .makeFromRepresentation(cfg)
                                    .renewToken();

                            } else {

                                const authenticationRealm = getAuthenticationRealm(error);

                                if (authenticationRealm === cfg.realm && authenticationRealm === API_AUTH0_REALM) {

                                    AuthService
                                        .makeFromRepresentation(cfg)
                                        .login((err, authResult) => {

                                            if (!err) {

                                                if (!authResult || !authResult.accessToken) {
                                                    log.error('Json web token not returned on the key: \'accessToken\'');
                                                } else {
                                                    // note: can't get this working as a promise so needs to be a callback
                                                    this.onSuccess(authResult.accessToken);
                                                }

                                            } else {
                                                this.onFailure(err);
                                            }

                                        })

                                } else {
                                    return Promise.reject(`[Authenticator] Realm mismatch: '${API_AUTH0_REALM}'`);
                                }
                            }


                        })
                        .catch(this.onFailure);

                } else {

                    log.error(`[Authenticator] www-authenticate type unknown: '${authenticationScheme}'`);
                }


            },
            onSubmit() {
                this.error = '';
            },
            /**
             * @param {string} accessToken
             */
            onSuccess(accessToken) {

                setJsonWebTokenOnHeaders(accessToken);

                EventBus.$emit(loginConfirmed);

                isPerformingAuthentication = false;
                this.authenticated = true;

            },
            onFailure(error) {
                log.error(error);
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