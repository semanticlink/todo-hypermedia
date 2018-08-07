<template>
    <div class="hello">
        <h1>Organisations
            <drag-and-droppable-model
                    :model="this.$root.$api"
                    :context="this.$root.$api"
                    media-type="application/json"
                    :dropped="createTenantOnRoot">
                <b-button
                        variant="outline"
                        v-b-tooltip.hover.html.right
                        title="Drop on to create">
                    <add w="22px" h="22px"/>
                </b-button>
            </drag-and-droppable-model>
        </h1>
        <ul>
            <li v-for="tenant in tenants.items" v-cloak>
                <b-link @click="gotoTenant(tenant)">{{ tenant.name }}</b-link>
                <drag-and-droppable-model
                        :model="tenant"
                        :async="true"
                        media-type="application/json"
                        :dropped="createOrUpdateUsersOnTenant">
                    <b-button
                            @mousedown="hydrateTenant"
                            variant="outline"
                            v-b-tooltip.hover.html.right
                            title="Drag off to take a copy or drop on to update"
                    >
                        <add w="22px" h="22px" title="Drag"/>
                    </b-button>
                </drag-and-droppable-model>

            </li>
        </ul>
    </div>
</template>

<script>
    import {_} from 'semanticLink';
    import {getUri} from 'semantic-link';
    import {log} from 'logger';
    import {redirectToTenant} from "router";
    import DragAndDroppableModel from './DragAndDroppableModel.vue'
    import {getTenantAndTodos, getTenants, createOrUpdateUsersOnTenant, createTenantOnRoot} from '../domain/tenant';
    import bButton from 'bootstrap-vue/es/components/button/button';
    import bLink from 'bootstrap-vue/es/components/link/link';
    import Add from 'vue-ionicons/dist/md-cloud-upload.vue';
    import bTooltip from 'bootstrap-vue/es/components/tooltip/tooltip'
    import EventBus from "../lib/util/EventBus";


    export default {
        components: {DragAndDroppableModel, bButton, Add, bTooltip, bLink},
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
            createTenantOnRoot(tenantDocument, apiResource) {

                // Ensure the survey name is 'unique'
                tenantDocument.name = `${tenantDocument.name || 'New tenant'} (${Date.now() % 1000000})`;
                if ('links' in tenantDocument) {
                    tenantDocument.links[0].href = ''; // remove the self link
                }

                this.$notify('Starting create new tenant');

                createTenantOnRoot(apiResource, tenantDocument, {})
                    .then(this.notifySuccess)
                    .catch(this.notifyError);
            },
            createOrUpdateUsersOnTenant(tenantDocument) {

                createOrUpdateUsersOnTenant(this.$root.$api.tenants, tenantDocument, this.$root.$api, {})
                    .then(this.notifySuccess)
                    .catch(this.notifyError);

            },
            hydrateTenant: function () {
                getTenantAndTodos(this.$root.$api)
                    .then(() => EventBus.$emit('resource:ready'))
                    .catch(err => {
                        this.$notify({type: 'error', title: 'Could not load up the tenant'});
                        log.error(err);
                    });
            },
            notifyError(err) {
                const message = err.message || '';
                this.$notify({
                    title: "Provisioning Error",
                    text: message,
                    type: 'error'
                });
                log.error(err);
            },
            notifySuccess(resource) {
                this.$notify({
                    title: "Success update or create",
                    text: getUri(resource, /self/),
                    type: 'success'
                });
            }

        },
    };
</script>
