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
            <li v-for="tenant in tenantCollection" v-cloak>
                <span>{{ tenant.name }}</span>
                <drag-and-droppable-model
                        :model="tenant"
                        :async="true"
                        media-type="application/json"
                        :dropped="createOrUpdateTenant">
                    <b-button
                            @mousedown="hydrateTenant(tenant)"
                            variant="outline"
                            v-b-tooltip.hover.html.right
                            title="Drag off to take a copy or drop on to update"
                    >
                        <add w="22px" h="22px" title="Drag"/>
                    </b-button>
                </drag-and-droppable-model>

                <ul>
                    <li v-for="todo in tenant.todos.items" v-cloak>
                        <b-link @click="gotoTodo(todo)">{{ todo.name }}</b-link>
                    </li>
                </ul>

            </li>
        </ul>
    </div>
</template>

<script>
    import {_} from 'semantic-network';
    import {getUri} from 'semantic-link';
    import {log} from 'logger';
    import {redirectToTodo} from 'router';
    import DragAndDroppableModel from '../DragAndDroppableModel.vue'
    import {syncTenant} from '../../domain/tenant';
    import bButton from 'bootstrap-vue/es/components/button/button';
    import bLink from 'bootstrap-vue/es/components/link/link';
    import Add from 'vue-ionicons/dist/md-cloud-upload.vue';
    import bTooltip from 'bootstrap-vue/es/components/tooltip/tooltip'
    import {eventBus} from 'semantic-link-utils/EventBus';
    import {getTenantsOnUser, getUserTenant} from 'domain/tenant';
    import {getTodosWithTagsOnTenantTodos} from 'domain/todo';
    import {isCollectionEmpty} from 'semantic-network/mixins/collection';


    export default {
        components: {DragAndDroppableModel, bButton, Add, bTooltip, bLink},
        data() {
            return {
                msg: 'Looking ...',
                error: false,
                invalid: '',
                busy: true,

                /**
                 * Tenants available for the user
                 */
                tenants: {},
            };
        },
        computed: {
            tenantCollection() {
                return this.tenants.items ? this.tenants.items : [];
            }
        },
        created: function () {

            log.info(`Loading tenants for user`);

            /**
             * Strategy One: use the first tenant from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param {CacheOptions?} options
             * @returns {Promise|*}
             */
            const loadTenantsWithTodoLists = (apiResource, options) => {

                return getTenantsOnUser(apiResource, options)
                    .then(tenants => {
                        if (isCollectionEmpty(tenants)) {
                            this.$notify({
                                title: "You no longer have any organisation to belong to",
                                type: 'info'
                            });
                            log.info('No tenants found');
                        }

                        // this is not a lazy-loading UI design (large sets will appear after time)
                        return getTodosWithTagsOnTenantTodos(tenants, options)
                        // lazy loading would require explicit setting inside the tenants
                            .then(() => this.$set(this, 'tenants', tenants)/*this.tenants = tenants*/)

                    })
                    .catch(err => {
                        this.$notify({
                            text: err,
                            type: 'error'
                        });
                        log.error(err);
                    });
            };

            return loadTenantsWithTodoLists(this.$root.$api);

        },
        methods: {
            /**
             * User has decided to browse to the todo list to work on it
             */
            gotoTodo(todo) {
                redirectToTodo(todo);
            },
            /**
             * Create a new tenant and clones existing lists/tags onto this tenant
             *
             * Note: demo version just creates its own new tenant with random numbers
             *
             * @param tenantDocument
             * @param apiResource
             */
            createTenantOnRoot(tenantDocument, apiResource) {

                // Ensure the survey name and code are 'unique' (To Be Deleted)
                tenantDocument.name = `${tenantDocument.name || 'New tenant'} (${Date.now() % 1000000})`;
                tenantDocument.code = `${Date.now() % 1000000}.${tenantDocument.code}`;
                if ('links' in tenantDocument) {
                    delete tenantDocument.links;
                }

                this.$notify('Starting create new tenant');

                this.createOrUpdateTenant(tenantDocument);
            },
            /**
             * Update an existing tenant with existing (or new) todo lists with tags)
             * @param tenantDocument
             */
            createOrUpdateTenant(tenantDocument) {

                syncTenant(this.$root.$api, tenantDocument)
                    .then(this.notifySuccess)
                    .catch(this.notifyError);

            },
            /**
             * Helper that when a tenant is on start drag that the entire graph is hydrated. Once that is done
             * it hands back off with an event that needs to be listened for.
             *
             * Note: communicating between events async requires this approach
             *
             * @param {TenantRepresentation} tenant user's tenant
             */
            hydrateTenant(tenant) {
                getUserTenant(tenant)
                    .then(() => eventBus.$emit('resource:ready'))
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
