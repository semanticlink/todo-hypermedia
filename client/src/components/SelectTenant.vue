<template>
    <div class="hello">
        <h1>Organisations</h1>

        <div v-for="tenant in tenants.items" v-cloak>

            <b-button @click="gotoTenant(tenant)">{{ tenant.name }}</b-button>

        </div>
    </div>
</template>

<script>
    import {_} from 'semantic-link-cache';
    import {log} from 'logger';
    import {redirectToTenant} from "router";
    import DragAndDroppableModel from './DragAndDroppableModel.vue'
    import {getTenants} from '../domain/tenant';
    import bButton from 'bootstrap-vue/es/components/button/button';

    export default {
        components: {DragAndDroppableModel, bButton},
        data() {
            return {
                msg: 'Looking ...',
                error: false,
                invalid: '',
                busy: true,
                me: {},
                tenants: {},
            };
        },
        created: function () {

            log.info(`Loading selected tenant from api`);

            /**
             * Strategy One: use the first tenant from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            const strategyOneProvidedTenant = (apiResource) => {
                log.info('Looking for provided tenant');

                return getTenants(apiResource, 'tenants', /tenants/)
                    .then(tenants => {
                        if (tenants && _(tenants.items).isEmpty()) {
                            this.$notify({
                                title: "You no longer have any organisation to belong to",
                                type: 'info'
                            });
                            log.info('No tenants found');
                        } else if ([tenants.items].length === 1) {
                            // there is only one tenant so let's go there
                            return this.gotoTenant(...tenants.items)
                        }
                        // else, load up the tenants for the user to select
                        this.tenants = tenants;
                    })
                    .catch(err => {
                        this.$notify({
                            title: response.statusText,
                            text: err,
                            type: 'error'
                        });
                        log.error(err);
                    });
            };

            return strategyOneProvidedTenant(this.$root.$api);

        },
        methods: {
            gotoTenant(organisation) {
                redirectToTenant(organisation);
            },
        },
    };
</script>
