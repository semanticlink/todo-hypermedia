<template>
    <div>
        <h1>Todos        </h1>

        <div v-for="(value, key) in this.todos.items">
            {{ value.name }}
        </div>

    </div>
</template>

<script>

    import { _, log, nodMaker, SemanticLink } from 'semanticLink';

    export default {
        props: {
            /**
             * @type {TenantRepresentation}
             */
            apiUri: {type: String}
        },
        data () {
            return {
                todos: {},
            };
        },
        created: function () {

            log.debug(`Loading selected organisation`);

            /**
             * use the organisation from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @returns {Promise|*}
             */
            const getUser = (apiResource) => {
                log.debug('Looking for tenant in', this.apiUri);

                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /tenants/))
                    .then(tenants => nodMaker.getCollectionResourceItemByUri(tenants, this.apiUri))
                    .then(tenant => nodMaker.getNamedCollectionResourceAndItems(tenant, 'todos', /todos/))
                    .then(todos => this.todos = todos)
                    .catch(err => log.error(err));
            };

            return getUser(this.$root.$api);

        },
        computed: {
            displayFields: function () {
                return (this.todos.items || {}).map(i => { name: i.name})
            }
        },
        methods: {
            createOnUser (userRepresentation, context) {
                log.debug(`Create on user: '${userRepresentation.name}' --> ${SemanticLink.getUri(context, /self/)}`);
            }
        }
    };
</script>
