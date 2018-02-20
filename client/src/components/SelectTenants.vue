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
                <button class="btn btn-lg btn-primary btn-block" @click="post(organisation)">Search</button>

            </div>

        </div>
    </div>
</template>

<script>

    import { log, nodMaker, SemanticLink, link, _ } from 'semanticLink';
    import { redirectToTenant, redirectToUser } from "../router";

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
             * Search for a tenant using the search form on the collection
             *
             * @param {TenantCollectionRepresentation} tenantCollection
             * @param tenantName
             * @returns {Promise} containing the search result collection
             */
            searchForTenant: function (tenantCollection, tenantName) {

                log.debug('Looking for search form on tenant collection');
                return nodMaker.getSingletonResource(tenantCollection, 'searchForm', /search/)
                    .then(tenantSearchRepresentation => {

                        let form = _(tenantSearchRepresentation.items).first();

                        if (form) {

                            /**
                             * Construct search based on the tenantSearchRepresentation. This is a simple form
                             * so we just grab the first item and haven't checks on type.
                             *
                             * In the future, we might ask for a field which is http://types/text
                             */
                            const searchForm = {};
                            searchForm[form.name] = tenantName;

                            return link.post(tenantSearchRepresentation, /submit/, 'application/json', searchForm);
                        } else {
                            throw new Error(`Search form item does not exist on ${link.getUri(tenantCollection, /search/)}.Do you have the correct headers set?`);
                        }
                    })
                    .then(createdResult => link.http.get(createdResult.headers.location))
                    .then(searchResult => searchResult.data);
            },

            /**
             * Strategy One: use the first tenant from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            strategyOneProvidedTenant(apiResource, nextStrategy) {
                log.debug("Looking for provided tenant");
                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/))
                    .then(tenantCollection => {
                        const item = _(tenantCollection).firstItem();
                        if (item) {
                            this.$localStorage.set("tenant", item.name);
                            redirectToTenant(item);
                        } else {
                            nextStrategy();
                        }
                    })
                    .catch(() => nextStrategy());
            },

            /**
             * Strategy Two: use tenant name from local storage to search
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            strategyTwoKnownTenant(apiResource, nextStrategy) {
                log.debug('Looking for tenants at root');

                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', /tenants/))
                    .then(tenantCollection => this.searchForTenant(tenantCollection, this.$localStorage.get('tenant')))
                    .then(tenantCollection => {
                        const item = _(tenantCollection).firstItem();
                        if (item) {
                            this.$localStorage.set("tenant", item.name);
                            redirectToTenant(item);
                        } else {
                            nextStrategy();
                        }

                    })
                    .catch(err => {
                        log.error(err);
                    });
            },

            /** Strategy Three: let the user enter in a tenant name and search
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            strategyThreeUserEntry(apiResource) {

                this.busy = false;

                // need to wait for the user to enter a tenant and click the button
                log.debug("Waiting for user to enter tenant name");

            },

            post(tenantName) {
                this.error = false;
                this.invalid = "";

                return nodMaker
                    .getResource(this.$root.$api)
                    .then(apiResource => nodMaker.getNamedCollectionResource(apiResource, 'tenants', 'tenants'))
                    .then(tenantCollection => {
                        this.searchForTenant(tenantCollection, tenantName)
                            .then(tenantSearchFeed => {
                                if (_(tenantSearchFeed).isCollectionEmpty()) {
                                    this.error = true;
                                    this.invalid = `Unable to find ${tenantName}`;
                                } else {
                                    nodMaker.makeCollectionItemsFromFeedAddedToCollection(tenantCollection, tenantSearchFeed);
                                    this.$localStorage.set("tenant", tenantName);
                                    redirectToTenant(_(tenantCollection.items).first());
                                }
                            })
                            .catch(() => {
                                this.error = true;
                                this.invalid = tenantName;
                            });
                    });

            }
        }
    };
</script>
