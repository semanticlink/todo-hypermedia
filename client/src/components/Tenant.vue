<template>
    <div class="hello">
        <h1>Organisations</h1>

        <div v-for="tenant in tenants.items" v-cloak>

            <drag-and-droppable-model
                    :msg="tenant.name"
                    :model="tenant"
                    :context="$root.$api"
                    media-type="application/json"
                    :dropped="createOrUpdateTenantOnRoot">
                <b-button @click="gotoTenant(tenant)">{{ tenant.name }}</b-button>
            </drag-and-droppable-model>
        </div>
    </div>
</template>

<script>
    import {nodMaker, SemanticLink, _} from 'semanticLink';
    import {log} from 'logger';
    import {nodSynchroniser} from 'semanticLink/NODSynchroniser';
    import {redirectToTenant, redirectToSelectTenant} from "router";
    import DragAndDroppableModel from './DragAndDroppableModel.vue'

    export default {
        components: {DragAndDroppableModel},
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

                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.tryGetCollectionResourceAndItems(apiResource, 'tenants', /tenants/))
                    .then(tenants => {
                        if (tenants && _(tenants.items).isEmpty()) {
                            log.info('No tenants found: redirect to search for tenant')
                            redirectToSelectTenant();
                        }
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
            createOrUpdateTenantOnRoot(tenantDocument, rootRepresentation) {
                log.debug(tenantDocument)
                nodSynchroniser.getResourceInNamedCollection(rootRepresentation, 'tenants', /tenants/, tenantDocument, [])
                    .then(resource => log.debug(resource))
                    .catch(err => {
                        this.$notify({
                            title: "Provisioning Error",
                            text: err.message,
                            type: 'error'
                        });
                        log.error(err);
                    });
            }

        },
    };
</script>
