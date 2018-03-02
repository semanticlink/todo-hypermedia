<template>
    <div class="row">
        <div class="col-xs-12 col-sm-9 col-md-7 col-lg-6">

            <div class="account-wall" v-if="!busy">

                <h1 class="text-center login-title">Enter your organisation to continue</h1>

                <div class="alert alert-danger" v-if="error">
                    <p>{{ invalid }}</p>
                </div>
                <div class="form-group">
                    <input
                            type="text"
                            class="form-control"
                            placeholder="Enter your organisation"
                            required
                            autofocus
                            v-model="organisation"
                    >
                </div>
                <button class="btn btn-lg btn-primary btn-block" @click="searchForTenant(organisation)">Search</button>

            </div>

        </div>
    </div>
</template>

<script>

    import { _ } from 'semanticLink';
    import { log } from 'logger';
    import { redirectToTenant, redirectToUser } from "router";
    import { searchForTenant, getTenants } from "domain/tenant";

    export default {
        props: {
            apiUri: { type: String }
        },
        data() {
            return {
                error: false,
                invalid: "",
                busy: true,
                organisation: null
            };
        },
        created: function () {

            log.debug(`Loading selected organisation`);

            /**
             * The 'global' cache of representation synchronised from the server.
             *
             * @type {ApiRepresentation}
             */
            let api = this.$root.$api;

            return this.strategyOneProvidedTenant(api, () =>
                this.strategyTwoKnownTenant(api, () =>
                    this.strategyThreeUserEntry(api)
                )
            );

        },
        methods: {

            /**
             * Helper function used in strategies that redirects to tenant if the tenant name is located in a search
             * @param {ApiRepresentation} apiResource
             * @param {string} tenantName
             * @param nextStrategy
             * @returns {Promise|*}
             */
            retrieve(apiResource, tenantName, nextStrategy) {
                return getTenants(apiResource)
                    .then(tenantCollection => {
                        if (tenantCollection) {
                            return searchForTenant(tenantCollection, tenantName);
                        } else {
                            return nextStrategy();
                        }
                    })
                    .then(searchCollection => this.storeTenantNameAndRedirect(searchCollection, nextStrategy));
            },

            /**
             * Helper function that redirects to the first tenant found in the collection or moves to next strategy
             * @param {CollectionRepresentation} tenantSearchCollection
             * @param nextStrategy
             * @returns {Promise|*}
             */
            storeTenantNameAndRedirect(tenantSearchCollection, nextStrategy) {
                const tenant = _(tenantSearchCollection).firstItem();
                if (tenant) {
                    log.debug(`Tenant stored: ${tenant.name}`);
                    this.$localStorage.set("tenant", tenant.name);
                    redirectToTenant(tenant);
                } else {
                    return nextStrategy();
                }
            },

            /**
             * Strategy One: use the first tenant from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            strategyOneProvidedTenant(apiResource, nextStrategy) {
                log.debug("1. Retrieving tenant from tenant list");

                return getTenants(apiResource)
                    .then(tenantCollection => this.storeTenantNameAndRedirect(tenantCollection, nextStrategy))
                    .catch(() => nextStrategy());
            },

            /**
             * Strategy Two: use 'known' tenant name from local storage to search.
             *
             * If not known - move to next
             * If no tenants listed - move to next
             *
             * Currently - just loads the first tenant [KLUDGE]
             *
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            strategyTwoKnownTenant(apiResource, nextStrategy) {

                log.debug('2. Searching for known tenant');

                const knownTenant = this.$localStorage.get('tenant');

                if (!knownTenant) {
                    return nextStrategy();
                }

                log.debug(`Searching for known tenant ${knownTenant}`);

                return this.retrieve(apiResource, knownTenant, nextStrategy)
                    .catch(() => nextStrategy());
            },

            /** Strategy Three: let the user enter in a tenant name and search
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            strategyThreeUserEntry(apiResource) {

                this.busy = false;

                // need to wait for the user to enter a tenant and click the button
                // ui will then call {@link searchForTenant}
                log.debug("3. Waiting for user to enter tenant name");

            },

            searchForTenant(tenantName) {
                this.error = false;
                this.invalid = "";

                const strategy = () => {
                    this.error = true;
                    this.invalid = `Unable to find ${tenantName}`;
                };

                return this.retrieve(this.$root.$api, tenantName, strategy)
                    .catch(() => {
                        this.error = true;
                        this.invalid = tenantName;
                    });

            }
        }
    };
</script>
