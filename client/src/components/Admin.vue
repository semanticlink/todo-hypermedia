<template>
    <div class="hello">
        <h1>Organisations</h1>

        <div v-for="tenant in tenants.items" v-cloak>

            <drag-and-droppable-model
                    :model="hydrateTenant(tenant)"
                    media-type="application/json"
                    :dropped="createOrUpdateUsersOnTenant">
                <b-button @click="gotoTenant(tenant)">{{ tenant.name }}</b-button>
            </drag-and-droppable-model>
        </div>
    </div>
</template>

<script>
    import {_} from 'semanticLink';
    import {log} from 'logger';
    import {nodSynchroniser} from 'semanticLink/NODSynchroniser';
    import {redirectToTenant} from "router";
    import DragAndDroppableModel from './DragAndDroppableModel.vue'
    import {getTenantAndTodos, getTenants, createOrUpdateUsersOnTenant} from '../domain/tenant';
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

                return getTenants(apiResource)
                    .then(tenants => {
                        if (tenants && _(tenants.items).isEmpty()) {
                            this.$notify({
                                title: "You no longer have any organisation to belong to",
                                type: 'info'
                            });
                            log.info('No tenants found');
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
            createOrUpdateTenantOnRoot(tenantDocument) {

                nodSynchroniser.getResourceInNamedCollection(this.$root.$api, 'tenants', /tenants/, tenantDocument, [])
                    .then(resource => log.debug(resource))
                    .catch(err => logError);
            },
            createOrUpdateUsersOnTenant(tenantDocument) {

                createOrUpdateUsersOnTenant(this.$root.$api.tenants, tenantDocument, this.$root.$api, {})
                    .catch(err => logError);

            },
            hydrateTenant: function (tenantRepresentation) {
                return getTenantAndTodos(this.$root.$api)
                    .then(() => tenantRepresentation)
                    .catch(err => {
                        this.$notify({type: 'error', title: 'Could not load up the tenant'});
                        log.error(err);
                    });
            },
            logError(err) {
                this.$notify({
                    title: "Provisioning Error",
                    text: err.message,
                    type: 'error'
                });
                log.error(err);
            }

        },
    };
</script>
