<template>
    <div>
        <h1>User:
            <draggable-model
                    :model="user"
                    :msg="user.name">
            </draggable-model>
            <droppable-model
                    :context="user"
                    :dropped="createOnUser">
            </droppable-model>
        </h1>

        <div v-for="(value, key) in displayFields">
            {{ key }}: {{ value }}
        </div>

    </div>
</template>

<script>

    import { _, log, nodMaker, SemanticLink } from 'semanticLink';

    export default {
        name: 'User',
        props: {
            apiUri: {type: String}
        },
        data () {
            return {
                user: {},
            };
        },
        created: function () {

            log.debug(`Loading selected organisation`);

            /**
             * use the organisation from a provided list (when authenticated)
             * @param {ApiRepresentation} apiResource
             * @param nextStrategy
             * @returns {Promise|*}
             */
            const getUser = (apiResource, nextStrategy = () => {}) => {
                log.debug('Looking for companies in', this.apiUri);
                log.debug('Looking for companies in', this.apiUri);

                /*
                 * This may error, at this point it is likely that it is not authentiated. If the
                 * 401 interception is broken then it will fall through to the catch and the err
                 * will contain rather than an error the actual resource.
                 *
                 *
                 */
                return nodMaker
                    .getResource(apiResource)
                    .then(apiResource => nodMaker.getSingletonResource(apiResource, 'me', /me/)
                        .then(() => apiResource))
                    .then(apiResource => nodMaker.getCollectionResource(apiResource, 'tenants', /organisations/))
                    .then(organisations => nodMaker.getCollectionResource(organisations, 'companies', /companies/))
                    .then(companies => nodMaker.getCollectionResource(companies, 'users', /users/))
                    .then(users => nodMaker.getCollectionResourceItemByUri(users, this.apiUri))
                    .then(user => {
                        this.user = user;
                        console.log('test');
                    })
                    .catch(err => {
                        log.error(err);
                        return nextStrategy();
                    });
            };

            return getUser(this.$root.$api);

        },
        computed: {
            displayFields: function () {
                return _(this.user).omit('links', 'id');
            }
        },
        methods: {
            createOnUser (userRepresentation, context) {
                log.debug(`Create on user: '${userRepresentation.name}' --> ${SemanticLink.getUri(context, /self/)}`);
            }
        }
    };
</script>
