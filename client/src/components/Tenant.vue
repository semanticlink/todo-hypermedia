<template>
    <div class="hello">
        <h1>Organisations</h1>

        <div v-for="tenant in tenants.items" v-cloak>

            <drag-and-droppable-model
                    :model="hydrateTenant(tenant)"
                    media-type="application/json"
                    :dropped="createOrUpdateUsersOnTenant"
            >
                <b-button @click="gotoTenant(tenant)">{{ tenant.name }}</b-button>
            </drag-and-droppable-model>
        </div>
    </div>
</template>

<script>
    import {nodMaker, SemanticLink, _} from 'semanticLink';
    import {log} from 'logger';
    import {nodSynchroniser} from 'semanticLink/NODSynchroniser';
    import {redirectToTenant} from "router";
    import DragAndDroppableModel from './DragAndDroppableModel.vue'
    import {getTenantAndTodos} from '../domain/tenant';
    import {pooledTagResourceResolver} from '../domain/tags';
    import {getUri} from 'semantic-link';
    import bButton from 'bootstrap-vue/es/components/button/button';
    import {findResourceInCollection} from "semanticLink/mixins/collection";

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

                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.tryGetCollectionResourceAndItems(apiResource, 'tenants', /tenants/))
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
                    .catch(err => {
                        this.$notify({
                            title: "Provisioning Error",
                            text: err.message,
                            type: 'error'
                        });
                        log.error(err);
                    });
            },
            createOrUpdateUsersOnTenant(tenantDocument) {

                const tenantRepresentation = findResourceInCollection(this.$root.$api.tenants, tenantDocument, 'self');


                log.debug(`Update tenant ${tenantRepresentation.name} --> ${SemanticLink.getUri(tenantRepresentation, 'self')}`);


                const syncUsersStrategy = (tenantRepresentation, tenantDocument, strategies, options) => {
                    return nodSynchroniser.getNamedCollection(tenantRepresentation, 'users', /users/, tenantDocument, strategies, options);
                };

                const syncTodosStrategy = (userRepresentation, userDocument, strategies, options) => {
                    return nodSynchroniser.getNamedCollection(userRepresentation, 'todos', /todos/, userDocument, strategies, options);
                };

                const syncTagsStrategy = (todoRepresentation, todoDocument, strategies, options) => {
                    return nodSynchroniser.patchUriListOnNamedCollection(
                        todoRepresentation,
                        'tags',
                        /tags/,
                        [...todoDocument.tags.items].map(item => getUri(item, 'self')),
                        {
                            ...options,
                            ...{contributeonly: true},
                            ...pooledTagResourceResolver(this.$root.$api)
                        });
                };

                nodSynchroniser
                    .getResource(
                        tenantRepresentation,
                        tenantDocument,
                        [
                            (tenantRepresentation, tenantDocument, options) => syncUsersStrategy(
                                tenantRepresentation,
                                tenantDocument,
                                [
                                    (usersRepresentation, usersDocument, options) => syncTodosStrategy(
                                        usersRepresentation,
                                        usersDocument,
                                        [
                                            (todoRepresentation, todoDocument, options) => syncTagsStrategy(todoRepresentation, todoDocument, [], options)
                                        ],
                                        options)
                                ],
                                options)
                        ],
                        {})
                    .catch(err => {
                        this.$notify({
                            title: "Provisioning Error",
                            text: err.message,
                            type: 'error'
                        });
                        log.error(err);
                    });
            },
            hydrateTenant: function (tenantRepresentation) {
                return getTenantAndTodos(this.$root.$api)
                    .then(() => tenantRepresentation)
                    .catch(err => {
                        this.$notify({type: 'error', title: 'Could not load up the tenant'});
                        log.error(err);
                    });
            }

        },
    };
</script>
