<template>
    <div class="hello">
        <h1>Organisations
            <droppable-model
                    :context="$root.$api"
                    :dropped="createOrUpdateTenantOnRoot">
            </droppable-model>
        </h1>

        <ul>
            <li v-for="tenant in tenants.items">
                <a v-on:click="gotoTenant(tenant)">{{ tenant.name }}</a>
                <drag-and-droppable-model
                        :msg="tenant.name"
                        :model="tenant"
                        :context="$root.$api"
                        :dropped="createOrUpdateTenantOnRoot">
                </drag-and-droppable-model>
            </li>
        </ul>
    </div>
</template>

<script>
    import { nodMaker, SemanticLink, _ } from 'semanticLink';
    import { log } from 'logger';
    import { toSitePath } from '../lib/util/UriMapping';
    import { nodSynchroniser } from 'semanticLink/NODSynchroniser';
    import { redirectToTenant, redirectToSelectTenant } from "router";

    export default {
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
                    .catch(err => log.error(err));
            };

            return strategyOneProvidedTenant(this.$root.$api);

        },
        methods: {
            gotoTenant(organisation) {
                redirectToTenant(organisation);
            },
            createOrUpdateTenantOnRoot(tenantDocument, rootRepresentation) {
                nodSynchroniser.getResourceInNamedCollection(rootRepresentation, 'tenants', /tenants/, tenantDocument, [])
                    .then(resource => log.debug(resource))
                    .catch(err => log.error(err));
            }

        },
    };
</script>
