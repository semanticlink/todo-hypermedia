<template>
    <transition name="slideup">
        <div>
            <!-- TODO: styling on a login -->
            <div> {{ error }}</div>

        </div>
    </transition>
</template>

<script>
    import {eventBus} from 'semantic-link-utils/EventBus';
    import {authRequired, authConfirmed} from 'semantic-link-utils/authEvent';
    import {log} from 'logger';
    import {
        getAuthenticationScheme,
        JWT,
        setJsonWebTokenOnHeaders,
        getAuthenticationRealm,
        API_AUTH0_REALM,
        renewToken
    } from 'semantic-link-utils/http-interceptors';
    import Form from './Form.vue';
    import AuthService from "semantic-link-utils/AuthService";

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
                 * @type {string}
                 */
                error: ''
            };
        },
        mounted() {
            eventBus.$on(authRequired, this.loginRequired);
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

                            if (renewToken(error)) {

                                log.info('Renewing token');

                                /**
                                 * KLUDGE: don't understand why we need to clear out the session but somehow
                                 * the underlying auth0 code works to show the login.
                                 */
                                AuthService.clearSession();

                                AuthService
                                    .makeFromRepresentation(cfg)
                                    .renewToken()
                                    .then(authResult => this.onSuccess(authResult.accessToken))
                                    .catch(this.onFailure);

                            } else {

                                const authenticationRealm = getAuthenticationRealm(error);

                                if (authenticationRealm === cfg.realm && authenticationRealm === API_AUTH0_REALM) {

                                    AuthService
                                        .makeFromRepresentation(cfg)
                                        .login()
                                        .then(authResult => {

                                            if (authResult) {
                                                return this.onSuccess(authResult.accessToken);
                                            } else {
                                                log.debug('[Authenticator] Json web token not returned on the key: \'accessToken\'');
                                                this.onSuccess(undefined);
                                            }

                                        })
                                        .catch(this.onFailure)

                                } else {
                                    log.error(`[Authenticator] Realm mismatch: '${API_AUTH0_REALM}'`);
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

                if (accessToken) {
                    setJsonWebTokenOnHeaders(accessToken);
                }

                isPerformingAuthentication = false;
                this.authenticated = true;

                eventBus.$emit(authConfirmed);

            },
            /**
             *
             * @param {Error} error
             */
            onFailure(error) {

                // TODO: does this need trigger log in again?
                // eventBus.$on(loginRequired, this.loginRequired);

                log.error(error);

                this.$notify({
                    title: 'An error means that the login process won\'t work',
                    text: error.message,
                    type: 'error'
                });
            }
        }
    };
</script>
